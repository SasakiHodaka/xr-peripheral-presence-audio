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

        private int windowTokenCount;
        private int windowGeneratedTokenCount;
        private int windowSelectedTokenCount;
        private int windowDroppedTokenCount;
        private int windowImportantTokenCount;
        private int windowImportantTokenSentCount;
        private int windowJsonBytes;
        private int windowSelectedJsonBytes;
        private int windowCompactBytes;
        private int windowSelectedCompactBytes;
        private int windowObjectMetadataBytes;
        private float windowStartTime;
        private string filePath;

        public float TokensPerSecond { get; private set; }
        public float GeneratedTokensPerSecond { get; private set; }
        public float SelectedTokensPerSecond { get; private set; }
        public float JsonBytesPerSecond { get; private set; }
        public float SelectedJsonBytesPerSecond { get; private set; }
        public float CompactBytesPerSecond { get; private set; }
        public float SelectedCompactBytesPerSecond { get; private set; }
        public float ObjectMetadataBytesPerSecond { get; private set; }
        public float CompactSavingsRatio { get; private set; }
        public float TokenDropRatio { get; private set; }
        public float ImportantTokenSendRatio { get; private set; }
        public float SelectionSavingsRatio { get; private set; }

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

        public string SelectionSummary
        {
            get
            {
                return string.Format(
                    "generated/s={0:F1} selected/s={1:F1} drop={2:P0} importantSend={3:P0} selectedJson={4:F0}B/s selectionSaving={5:P0}",
                    GeneratedTokensPerSecond,
                    SelectedTokensPerSecond,
                    TokenDropRatio,
                    ImportantTokenSendRatio,
                    SelectedJsonBytesPerSecond,
                    SelectionSavingsRatio);
            }
        }

        private void Start()
        {
            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(Application.persistentDataPath, fileNamePrefix + "_" + timestamp + ".csv");
            File.WriteAllText(filePath, "timestamp,condition,tokensPerSecond,jsonBytesPerSecond,compactBytesPerSecond,objectMetadataBytesPerSecond,compactSavingsRatio,generatedTokensPerSecond,selectedTokensPerSecond,selectedJsonBytesPerSecond,selectedCompactBytesPerSecond,tokenDropRatio,importantTokenSendRatio,selectionSavingsRatio\n");
            windowStartTime = Time.unscaledTime;
        }

        public void Observe(IReadOnlyList<SceneToken> tokens)
        {
            Observe(tokens, tokens);
        }

        public void Observe(IReadOnlyList<SceneToken> generatedTokens, IReadOnlyList<SceneToken> selectedTokens)
        {
            if (generatedTokens == null)
            {
                return;
            }

            for (var i = 0; i < generatedTokens.Count; i++)
            {
                var token = generatedTokens[i];
                windowTokenCount++;
                windowGeneratedTokenCount++;
                windowJsonBytes += EstimateJsonBytes(token);
                windowCompactBytes += EstimateCompactSceneTokenBytes(token);
                windowObjectMetadataBytes += EstimateObjectMetadataBytes();

                if (IsImportantToken(token))
                {
                    windowImportantTokenCount++;
                }
            }

            if (selectedTokens != null)
            {
                windowSelectedTokenCount += selectedTokens.Count;

                for (var i = 0; i < selectedTokens.Count; i++)
                {
                    var token = selectedTokens[i];
                    windowSelectedJsonBytes += EstimateJsonBytes(token);
                    windowSelectedCompactBytes += EstimateCompactSceneTokenBytes(token);

                    if (IsImportantToken(token))
                    {
                        windowImportantTokenSentCount++;
                    }
                }
            }

            windowDroppedTokenCount += Mathf.Max(0, generatedTokens.Count - (selectedTokens != null ? selectedTokens.Count : 0));

            var elapsed = Time.unscaledTime - windowStartTime;
            if (elapsed < Mathf.Max(0.1f, summaryWindowSeconds))
            {
                return;
            }

            TokensPerSecond = windowTokenCount / elapsed;
            GeneratedTokensPerSecond = windowGeneratedTokenCount / elapsed;
            SelectedTokensPerSecond = windowSelectedTokenCount / elapsed;
            JsonBytesPerSecond = windowJsonBytes / elapsed;
            SelectedJsonBytesPerSecond = windowSelectedJsonBytes / elapsed;
            CompactBytesPerSecond = windowCompactBytes / elapsed;
            SelectedCompactBytesPerSecond = windowSelectedCompactBytes / elapsed;
            ObjectMetadataBytesPerSecond = windowObjectMetadataBytes / elapsed;
            CompactSavingsRatio = ObjectMetadataBytesPerSecond > 0f
                ? 1f - CompactBytesPerSecond / ObjectMetadataBytesPerSecond
                : 0f;
            TokenDropRatio = windowGeneratedTokenCount > 0
                ? (float)windowDroppedTokenCount / windowGeneratedTokenCount
                : 0f;
            ImportantTokenSendRatio = windowImportantTokenCount > 0
                ? (float)windowImportantTokenSentCount / windowImportantTokenCount
                : 1f;
            SelectionSavingsRatio = windowJsonBytes > 0
                ? 1f - (float)windowSelectedJsonBytes / windowJsonBytes
                : 0f;

            WriteMetricsRow();

            windowTokenCount = 0;
            windowGeneratedTokenCount = 0;
            windowSelectedTokenCount = 0;
            windowDroppedTokenCount = 0;
            windowImportantTokenCount = 0;
            windowImportantTokenSentCount = 0;
            windowJsonBytes = 0;
            windowSelectedJsonBytes = 0;
            windowCompactBytes = 0;
            windowSelectedCompactBytes = 0;
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
                "{{\"s\":\"{0}\",\"d\":\"{1}\",\"r\":\"{2}\",\"sp\":\"{3}\",\"t\":\"{4}\",\"m\":\"{5}\",\"u\":\"{6}\",\"o\":\"{7}\",\"p\":{8:F2},\"c\":{9:F2}}}",
                token.speakerId,
                token.direction,
                token.distance,
                token.speakingState,
                token.turnState,
                token.semanticToken,
                token.urgency,
                token.targetObjectId,
                token.priority,
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
            // + speaking(1) + turn(1) + semantic(1) + urgency(1)
            // + target id(1) + priority/confidence quantized(2)
            return 12;
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
            var row = string.Format(
                "{0:F3},{1},{2:F2},{3:F2},{4:F2},{5:F2},{6:F4},{7:F2},{8:F2},{9:F2},{10:F2},{11:F4},{12:F4},{13:F4}\n",
                Time.time,
                condition,
                TokensPerSecond,
                JsonBytesPerSecond,
                CompactBytesPerSecond,
                ObjectMetadataBytesPerSecond,
                CompactSavingsRatio,
                GeneratedTokensPerSecond,
                SelectedTokensPerSecond,
                SelectedJsonBytesPerSecond,
                SelectedCompactBytesPerSecond,
                TokenDropRatio,
                ImportantTokenSendRatio,
                SelectionSavingsRatio);
            File.AppendAllText(filePath, row);
        }

        private static bool IsImportantToken(SceneToken token)
        {
            if (token == null)
            {
                return false;
            }

            return token.semanticToken == SceneSemanticToken.EMERGENCY.ToString() ||
                   token.semanticToken == SceneSemanticToken.WARNING.ToString() ||
                   token.semanticToken == SceneSemanticToken.INSTRUCTION.ToString() ||
                   token.urgency == SceneUrgency.HIGH.ToString() ||
                   token.urgency == SceneUrgency.CRITICAL.ToString();
        }
    }
}
