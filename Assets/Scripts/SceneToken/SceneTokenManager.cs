using System.Collections.Generic;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenManager : MonoBehaviour
    {
        public Transform listener;
        public SpeakerObject[] speakers;
        public float tokenUpdateInterval = 0.1f;
        public SceneTokenLogger logger;
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenMetrics metrics;
        public SceneTokenExperimentSession experimentSession;
        public bool logTokens = true;
        public bool showDebugHud = true;

        private readonly List<SceneToken> latestTokens = new List<SceneToken>();
        private float nextTokenTime;

        public IReadOnlyList<SceneToken> LatestTokens
        {
            get { return latestTokens; }
        }

        private void Reset()
        {
            listener = Camera.main != null ? Camera.main.transform : null;
            speakers = FindObjectsOfType<SpeakerObject>();
            logger = GetComponent<SceneTokenLogger>();
            decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            metrics = GetComponent<SceneTokenMetrics>();
            experimentSession = GetComponent<SceneTokenExperimentSession>();
        }

        private void Awake()
        {
            if (listener == null && Camera.main != null)
            {
                listener = Camera.main.transform;
            }

            if (logger == null)
            {
                logger = GetComponent<SceneTokenLogger>();
            }

            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            if (metrics == null)
            {
                metrics = GetComponent<SceneTokenMetrics>();
            }

            if (experimentSession == null)
            {
                experimentSession = GetComponent<SceneTokenExperimentSession>();
            }
        }

        private void Update()
        {
            if (Time.time < nextTokenTime)
            {
                return;
            }

            nextTokenTime = Time.time + Mathf.Max(0.02f, tokenUpdateInterval);
            GenerateTokens();
        }

        private void OnGUI()
        {
            if (!showDebugHud)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 560f, 320f), GUI.skin.box);
            GUILayout.Label("Scene Tokens");

            if (decoderRenderer != null)
            {
                GUILayout.Label("Condition: " + decoderRenderer.renderCondition);
            }

            if (metrics != null)
            {
                GUILayout.Label(metrics.Summary);
            }

            if (experimentSession != null)
            {
                GUILayout.Label(experimentSession.Summary);
            }

            for (var i = 0; i < latestTokens.Count; i++)
            {
                var token = latestTokens[i];
                GUILayout.Label(string.Format(
                    "{0}: {1} {2} {3} {4} {5} az={6:F1} range={7:F2}",
                    token.speakerId,
                    token.direction,
                    token.distance,
                    token.speakingState,
                    token.turnState,
                    token.semanticToken,
                    token.azimuth,
                    token.range));

                if (!string.IsNullOrEmpty(token.utteranceText))
                {
                    GUILayout.Label("  \"" + token.utteranceText + "\"");
                }
            }

            GUILayout.EndArea();
        }

        private void GenerateTokens()
        {
            latestTokens.Clear();

            if (listener == null || speakers == null)
            {
                return;
            }

            var speakingCount = CountSpeakingSpeakers();

            for (var i = 0; i < speakers.Length; i++)
            {
                var speaker = speakers[i];
                if (speaker == null)
                {
                    continue;
                }

                var token = CreateToken(speaker, speakingCount);
                latestTokens.Add(token);

                if (logTokens && logger != null)
                {
                    logger.Write(token);
                }
            }

            if (decoderRenderer != null)
            {
                decoderRenderer.Render(latestTokens);
            }

            if (metrics != null)
            {
                metrics.Observe(latestTokens);
            }
        }

        private int CountSpeakingSpeakers()
        {
            var count = 0;

            for (var i = 0; i < speakers.Length; i++)
            {
                if (speakers[i] != null && speakers[i].IsSpeaking)
                {
                    count++;
                }
            }

            return count;
        }

        private SceneToken CreateToken(SpeakerObject speaker, int speakingCount)
        {
            var offset = speaker.transform.position - listener.position;
            var flatOffset = Vector3.ProjectOnPlane(offset, Vector3.up);
            var range = flatOffset.magnitude;
            var azimuth = CalculateSignedAzimuth(flatOffset);
            var isSpeaking = speaker.IsSpeaking;

            return new SceneToken
            {
                speakerId = speaker.speakerId,
                azimuth = azimuth,
                range = range,
                direction = QuantizeDirection(azimuth).ToString(),
                distance = QuantizeDistance(range).ToString(),
                speakingState = isSpeaking ? SceneSpeakingState.SPEAKING.ToString() : SceneSpeakingState.SILENT.ToString(),
                turnState = QuantizeTurnState(isSpeaking, speakingCount).ToString(),
                semanticToken = isSpeaking ? speaker.semanticToken.ToString() : SceneSemanticToken.NONE.ToString(),
                utteranceText = isSpeaking ? speaker.utteranceText : string.Empty,
                semanticConfidence = isSpeaking ? speaker.semanticConfidence : 0f,
                condition = decoderRenderer != null ? decoderRenderer.renderCondition.ToString() : string.Empty,
                timestamp = Time.time
            };
        }

        private float CalculateSignedAzimuth(Vector3 flatOffset)
        {
            if (flatOffset.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            var listenerForward = Vector3.ProjectOnPlane(listener.forward, Vector3.up).normalized;
            var listenerRight = Vector3.ProjectOnPlane(listener.right, Vector3.up).normalized;
            var forwardDot = Vector3.Dot(listenerForward, flatOffset.normalized);
            var rightDot = Vector3.Dot(listenerRight, flatOffset.normalized);
            return Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;
        }

        private static SceneTokenDirection QuantizeDirection(float azimuth)
        {
            if (azimuth >= -22.5f && azimuth < 22.5f) return SceneTokenDirection.FRONT;
            if (azimuth >= 22.5f && azimuth < 67.5f) return SceneTokenDirection.FRONT_RIGHT;
            if (azimuth >= 67.5f && azimuth < 112.5f) return SceneTokenDirection.RIGHT;
            if (azimuth >= 112.5f && azimuth < 157.5f) return SceneTokenDirection.BACK_RIGHT;
            if (azimuth >= -67.5f && azimuth < -22.5f) return SceneTokenDirection.FRONT_LEFT;
            if (azimuth >= -112.5f && azimuth < -67.5f) return SceneTokenDirection.LEFT;
            if (azimuth >= -157.5f && azimuth < -112.5f) return SceneTokenDirection.BACK_LEFT;
            return SceneTokenDirection.BACK;
        }

        private static SceneTokenDistance QuantizeDistance(float range)
        {
            if (range < 1.5f) return SceneTokenDistance.NEAR;
            if (range < 3f) return SceneTokenDistance.MID;
            return SceneTokenDistance.FAR;
        }

        private static SceneTurnState QuantizeTurnState(bool isSpeaking, int speakingCount)
        {
            if (!isSpeaking) return SceneTurnState.LISTENER;
            if (speakingCount == 1) return SceneTurnState.TURN_HOLDER;
            return SceneTurnState.OVERLAPPER;
        }
    }
}
