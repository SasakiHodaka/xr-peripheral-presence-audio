using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenMetrics : MonoBehaviour
    {
        public string fileNamePrefix = "scene_token_metrics";
        public float summaryWindowSeconds = 1f;
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenExperimentSession experimentSession;

        private int windowTokenCount;
        private int windowJsonBytes;
        private int windowCompactBytes;
        private int windowObjectMetadataBytes;
        private float windowStartTime;
        private string filePath;

        public float TokensPerSecond { get; private set; }
        public float JsonBytesPerSecond { get; private set; }
        public float CompactBytesPerSecond { get; private set; }
        public float ObjectMetadataBytesPerSecond { get; private set; }
        public float CompactSavingsRatio { get; private set; }

        public string Summary
        {
            get
            {
                return string.Format(
                    "tokens/s={0:F1} json={1:F0}B/s compact={2:F0}B/s objectMeta={3:F0}B/s saving={4:P0}",
                    TokensPerSecond,
                    JsonBytesPerSecond,
                    CompactBytesPerSecond,
                    ObjectMetadataBytesPerSecond,
                    CompactSavingsRatio);
            }
        }

        private void Start()
        {
            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            if (experimentSession == null)
            {
                experimentSession = GetComponent<SceneTokenExperimentSession>();
            }

            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(Application.persistentDataPath, fileNamePrefix + "_" + timestamp + ".csv");
            File.WriteAllText(filePath, "timestamp,sessionId,participantId,trialIndex,trialElapsed,condition,tokensPerSecond,jsonBytesPerSecond,compactBytesPerSecond,objectMetadataBytesPerSecond,compactSavingsRatio\n");
            windowStartTime = Time.unscaledTime;
        }

        public void Observe(IReadOnlyList<SceneToken> tokens)
        {
            if (tokens == null)
            {
                return;
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                windowTokenCount++;
                windowJsonBytes += EstimateJsonBytes(token);
                windowCompactBytes += EstimateCompactSceneTokenBytes(token);
                windowObjectMetadataBytes += EstimateObjectMetadataBytes();
            }

            var elapsed = Time.unscaledTime - windowStartTime;
            if (elapsed < Mathf.Max(0.1f, summaryWindowSeconds))
            {
                return;
            }

            TokensPerSecond = windowTokenCount / elapsed;
            JsonBytesPerSecond = windowJsonBytes / elapsed;
            CompactBytesPerSecond = windowCompactBytes / elapsed;
            ObjectMetadataBytesPerSecond = windowObjectMetadataBytes / elapsed;
            CompactSavingsRatio = ObjectMetadataBytesPerSecond > 0f
                ? 1f - CompactBytesPerSecond / ObjectMetadataBytesPerSecond
                : 0f;

            WriteMetricsRow();

            windowTokenCount = 0;
            windowJsonBytes = 0;
            windowCompactBytes = 0;
            windowObjectMetadataBytes = 0;
            windowStartTime = Time.unscaledTime;
        }

        private static int EstimateJsonBytes(SceneToken token)
        {
            if (token == null)
            {
                return 0;
            }

            var jsonLike = string.Format(
                "{{\"s\":\"{0}\",\"d\":\"{1}\",\"r\":\"{2}\",\"sp\":\"{3}\",\"t\":\"{4}\",\"m\":\"{5}\",\"c\":{6:F2}}}",
                token.speakerId,
                token.direction,
                token.distance,
                token.speakingState,
                token.turnState,
                token.semanticToken,
                token.semanticConfidence);

            return Encoding.UTF8.GetByteCount(jsonLike);
        }

        private static int EstimateCompactSceneTokenBytes(SceneToken token)
        {
            if (token == null)
            {
                return 0;
            }

            // timestamp delta(2) + speaker(1) + direction(1) + distance(1)
            // + speaking(1) + turn(1) + semantic(1) + confidence quantized(1)
            return 9;
        }

        private static int EstimateObjectMetadataBytes()
        {
            // speaker id(1) + azimuth/elevation/distance/gain as 32-bit floats.
            return 17;
        }

        private void WriteMetricsRow()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var condition = decoderRenderer != null ? decoderRenderer.renderCondition.ToString() : string.Empty;
            var sessionId = experimentSession != null ? experimentSession.sessionId : string.Empty;
            var participantId = experimentSession != null ? experimentSession.participantId : string.Empty;
            var trialIndex = experimentSession != null ? experimentSession.TrialIndex : 0;
            var trialElapsed = experimentSession != null ? experimentSession.TrialElapsedSeconds : 0f;
            var row = string.Format(
                "{0:F3},{1},{2},{3},{4:F3},{5},{6:F2},{7:F2},{8:F2},{9:F2},{10:F4}\n",
                Time.time,
                EscapeCsv(sessionId),
                EscapeCsv(participantId),
                trialIndex,
                trialElapsed,
                condition,
                TokensPerSecond,
                JsonBytesPerSecond,
                CompactBytesPerSecond,
                ObjectMetadataBytesPerSecond,
                CompactSavingsRatio);
            File.AppendAllText(filePath, row);
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
