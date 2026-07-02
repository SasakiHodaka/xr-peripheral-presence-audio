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
        public SceneTokenScriptedConversation scriptedConversation;
        public bool logTokens = true;
        public bool logOnlyDuringExperimentSession = true;
        public bool showDebugHud = true;
        public bool enableTokenSelection = false;
        [Range(0f, 1f)]
        public float minimumTransmissionPriority = 0.35f;

        private readonly List<SceneToken> latestTokens = new List<SceneToken>();
        private readonly List<SceneToken> generatedTokens = new List<SceneToken>();
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
            scriptedConversation = GetComponent<SceneTokenScriptedConversation>();
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

            if (scriptedConversation == null)
            {
                scriptedConversation = GetComponent<SceneTokenScriptedConversation>();
            }

            EnsureAudioListener();
        }

        private void Update()
        {
            HandleResponseInput();

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

            GUILayout.BeginArea(new Rect(16f, 16f, 720f, 560f), GUI.skin.box);
            GUILayout.Label("Scene Tokens");

            if (decoderRenderer != null)
            {
                GUILayout.Label("Condition: " + decoderRenderer.renderCondition);
            }

            if (metrics != null)
            {
                GUILayout.Label(metrics.Summary);
                GUILayout.Label(metrics.SelectionSummary);
            }

            if (experimentSession != null)
            {
                GUILayout.Label(experimentSession.Summary);

                if (logOnlyDuringExperimentSession && !experimentSession.IsRunning)
                {
                    GUILayout.Label("Logging paused until the experiment session starts.");
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Participant", GUILayout.Width(80f));
                experimentSession.participantId = GUILayout.TextField(experimentSession.participantId, GUILayout.Width(90f));
                GUILayout.Label("Session", GUILayout.Width(55f));
                experimentSession.sessionId = GUILayout.TextField(experimentSession.sessionId, GUILayout.Width(140f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(experimentSession.IsRunning ? "Stop Session" : "Start Session", GUILayout.Width(120f)))
                {
                    if (experimentSession.IsRunning)
                    {
                        experimentSession.StopSession("hud_stop");
                    }
                    else
                    {
                        experimentSession.StartSession();
                    }
                }

                if (GUILayout.Button("Next Condition", GUILayout.Width(120f)))
                {
                    experimentSession.AdvanceCondition("hud_next");
                }

                if (GUILayout.Button("Restart", GUILayout.Width(80f)))
                {
                    experimentSession.RestartSession();
                }
                GUILayout.EndHorizontal();
            }

            if (scriptedConversation != null)
            {
                GUILayout.Label(scriptedConversation.Summary);
            }

            DrawResponseControls();

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

                GUILayout.Label(string.Format(
                    "  urgency={0} target={1} priority={2:F2} selected={3} reason={4}",
                    token.urgency,
                    string.IsNullOrEmpty(token.targetObjectId) ? "-" : token.targetObjectId,
                    token.priority,
                    token.selectedForTransmission,
                    token.selectionReason));

                if (!string.IsNullOrEmpty(token.utteranceText))
                {
                    GUILayout.Label("  \"" + token.utteranceText + "\"");
                }
            }

            GUILayout.EndArea();
        }

        private void HandleResponseInput()
        {
            if (experimentSession == null || !experimentSession.IsRunning)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow)) RecordDirectionResponse(SceneTokenDirection.FRONT.ToString());
            if (Input.GetKeyDown(KeyCode.RightArrow)) RecordDirectionResponse(SceneTokenDirection.RIGHT.ToString());
            if (Input.GetKeyDown(KeyCode.DownArrow)) RecordDirectionResponse(SceneTokenDirection.BACK.ToString());
            if (Input.GetKeyDown(KeyCode.LeftArrow)) RecordDirectionResponse(SceneTokenDirection.LEFT.ToString());

            if (Input.GetKeyDown(KeyCode.J)) RecordSpeakerResponse("A");
            if (Input.GetKeyDown(KeyCode.K)) RecordSpeakerResponse("B");
            if (Input.GetKeyDown(KeyCode.L)) RecordSpeakerResponse("C");
        }

        private void DrawResponseControls()
        {
            if (experimentSession == null)
            {
                return;
            }

            GUILayout.Space(8f);
            GUILayout.Label("Participant Responses");
            GUILayout.Label(GetResponseTargetSummary());

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("FRONT", GUILayout.Width(80f))) RecordDirectionResponse(SceneTokenDirection.FRONT.ToString());
            if (GUILayout.Button("FRONT_RIGHT", GUILayout.Width(105f))) RecordDirectionResponse(SceneTokenDirection.FRONT_RIGHT.ToString());
            if (GUILayout.Button("RIGHT", GUILayout.Width(80f))) RecordDirectionResponse(SceneTokenDirection.RIGHT.ToString());
            if (GUILayout.Button("BACK_RIGHT", GUILayout.Width(105f))) RecordDirectionResponse(SceneTokenDirection.BACK_RIGHT.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("BACK", GUILayout.Width(80f))) RecordDirectionResponse(SceneTokenDirection.BACK.ToString());
            if (GUILayout.Button("BACK_LEFT", GUILayout.Width(105f))) RecordDirectionResponse(SceneTokenDirection.BACK_LEFT.ToString());
            if (GUILayout.Button("LEFT", GUILayout.Width(80f))) RecordDirectionResponse(SceneTokenDirection.LEFT.ToString());
            if (GUILayout.Button("FRONT_LEFT", GUILayout.Width(105f))) RecordDirectionResponse(SceneTokenDirection.FRONT_LEFT.ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Speaker A", GUILayout.Width(100f))) RecordSpeakerResponse("A");
            if (GUILayout.Button("Speaker B", GUILayout.Width(100f))) RecordSpeakerResponse("B");
            if (GUILayout.Button("Speaker C", GUILayout.Width(100f))) RecordSpeakerResponse("C");
            GUILayout.EndHorizontal();

            GUILayout.Label("Keys: arrows=FRONT/RIGHT/BACK/LEFT, J/K/L=speaker A/B/C");
        }

        private string GetResponseTargetSummary()
        {
            if (experimentSession == null || !experimentSession.IsRunning)
            {
                return "Target: session is not running";
            }

            var target = GetPrimarySpeakingToken();
            if (target == null)
            {
                return "Target: no active speaker, response will be ambiguous";
            }

            if (IsAmbiguousSpeakingTarget())
            {
                return "Target: overlap/no single target, response will be ambiguous";
            }

            return string.Format(
                "Target: speaker={0} direction={1} semantic={2}",
                target.speakerId,
                target.direction,
                target.semanticToken);
        }

        private void RecordDirectionResponse(string response)
        {
            if (experimentSession == null)
            {
                return;
            }

            var expected = GetPrimarySpeakingToken();
            experimentSession.RecordDirectionResponse(
                response,
                expected != null ? expected.direction : string.Empty,
                IsAmbiguousSpeakingTarget());
        }

        private void RecordSpeakerResponse(string response)
        {
            if (experimentSession == null)
            {
                return;
            }

            var expected = GetPrimarySpeakingToken();
            experimentSession.RecordSpeakerResponse(
                response,
                expected != null ? expected.speakerId : string.Empty,
                IsAmbiguousSpeakingTarget());
        }

        private SceneToken GetPrimarySpeakingToken()
        {
            SceneToken firstSpeaking = null;

            for (var i = 0; i < latestTokens.Count; i++)
            {
                var token = latestTokens[i];
                if (token == null || token.speakingState != SceneSpeakingState.SPEAKING.ToString())
                {
                    continue;
                }

                if (firstSpeaking == null)
                {
                    firstSpeaking = token;
                }

                if (token.turnState == SceneTurnState.TURN_HOLDER.ToString())
                {
                    return token;
                }
            }

            return firstSpeaking;
        }

        private bool IsAmbiguousSpeakingTarget()
        {
            var speakingCount = 0;
            for (var i = 0; i < latestTokens.Count; i++)
            {
                var token = latestTokens[i];
                if (token != null && token.speakingState == SceneSpeakingState.SPEAKING.ToString())
                {
                    speakingCount++;
                }
            }

            return speakingCount != 1;
        }

        private void GenerateTokens()
        {
            latestTokens.Clear();
            generatedTokens.Clear();

            if (listener == null || speakers == null)
            {
                return;
            }

            var speakingCount = ConversationAnalyzer.CountSpeakingSpeakers(speakers);

            for (var i = 0; i < speakers.Length; i++)
            {
                var speaker = speakers[i];
                if (speaker == null)
                {
                    continue;
                }

                var token = CreateToken(speaker, speakingCount);
                generatedTokens.Add(token);

                if (ShouldTransmitToken(token))
                {
                    token.selectedForTransmission = true;
                    latestTokens.Add(token);
                }
                else
                {
                    token.selectedForTransmission = false;
                }

                if (ShouldWriteExperimentData() && logTokens && logger != null)
                {
                    logger.Write(token);
                }
            }

            if (decoderRenderer != null)
            {
                decoderRenderer.Render(latestTokens);
            }

            if (ShouldWriteExperimentData() && metrics != null)
            {
                metrics.Observe(generatedTokens, latestTokens);
            }
        }

        private bool ShouldWriteExperimentData()
        {
            if (!logOnlyDuringExperimentSession || experimentSession == null)
            {
                return true;
            }

            return experimentSession.IsRunning;
        }

        private void EnsureAudioListener()
        {
            if (listener == null)
            {
                return;
            }

            if (FindObjectOfType<AudioListener>() == null)
            {
                listener.gameObject.AddComponent<AudioListener>();
            }

            AudioListener.volume = 1f;
        }

        private SceneToken CreateToken(SpeakerObject speaker, int speakingCount)
        {
            var range = DistanceAnalyzer.CalculateHorizontalRange(listener, speaker.transform);
            var azimuth = DirectionAnalyzer.CalculateSignedAzimuth(listener, speaker.transform);
            var isSpeaking = speaker.IsSpeaking;

            return new SceneToken
            {
                speakerId = speaker.speakerId,
                azimuth = azimuth,
                range = range,
                direction = DirectionAnalyzer.QuantizeDirection(azimuth).ToString(),
                distance = DistanceAnalyzer.QuantizeDistance(range).ToString(),
                speakingState = ConversationAnalyzer.QuantizeSpeakingState(isSpeaking).ToString(),
                turnState = ConversationAnalyzer.QuantizeTurnState(isSpeaking, speakingCount).ToString(),
                semanticToken = isSpeaking ? speaker.semanticToken.ToString() : SceneSemanticToken.NONE.ToString(),
                urgency = isSpeaking ? speaker.urgency.ToString() : SceneUrgency.LOW.ToString(),
                targetObjectId = isSpeaking ? speaker.targetObjectId : string.Empty,
                utteranceText = isSpeaking ? speaker.utteranceText : string.Empty,
                semanticConfidence = isSpeaking ? speaker.semanticConfidence : 0f,
                priority = CalculatePriority(isSpeaking, speaker.semanticToken, speaker.urgency, speakingCount),
                selectedForTransmission = true,
                selectionReason = enableTokenSelection ? string.Empty : "selection_disabled",
                condition = decoderRenderer != null ? decoderRenderer.renderCondition.ToString() : string.Empty,
                participantId = experimentSession != null ? experimentSession.participantId : string.Empty,
                sessionId = experimentSession != null ? experimentSession.sessionId : string.Empty,
                trialIndex = experimentSession != null ? experimentSession.TrialIndex : 0,
                trialElapsed = experimentSession != null ? experimentSession.TrialElapsedSeconds : 0f,
                timestamp = Time.time
            };
        }

        private bool ShouldTransmitToken(SceneToken token)
        {
            if (!enableTokenSelection || token == null)
            {
                if (token != null)
                {
                    token.selectionReason = "selection_disabled";
                }

                return true;
            }

            if (token.semanticToken == SceneSemanticToken.EMERGENCY.ToString() ||
                token.urgency == SceneUrgency.CRITICAL.ToString())
            {
                token.selectionReason = "critical";
                return true;
            }

            if (token.semanticToken == SceneSemanticToken.WARNING.ToString() ||
                token.semanticToken == SceneSemanticToken.INSTRUCTION.ToString())
            {
                token.selectionReason = "important_intent";
                return true;
            }

            if (token.speakingState == SceneSpeakingState.SPEAKING.ToString() &&
                token.priority >= minimumTransmissionPriority)
            {
                token.selectionReason = "priority";
                return true;
            }

            token.selectionReason = "low_priority";
            return false;
        }

        private static float CalculatePriority(bool isSpeaking, SceneSemanticToken semanticToken, SceneUrgency urgency, int speakingCount)
        {
            var score = 0f;

            if (isSpeaking)
            {
                score += 0.25f;
            }

            if (speakingCount > 1 && isSpeaking)
            {
                score += 0.1f;
            }

            switch (semanticToken)
            {
                case SceneSemanticToken.EMERGENCY:
                    score += 0.55f;
                    break;
                case SceneSemanticToken.WARNING:
                    score += 0.45f;
                    break;
                case SceneSemanticToken.INSTRUCTION:
                    score += 0.35f;
                    break;
                case SceneSemanticToken.QUESTION:
                case SceneSemanticToken.ANSWER:
                    score += 0.25f;
                    break;
                case SceneSemanticToken.AGREEMENT:
                case SceneSemanticToken.DISAGREEMENT:
                    score += 0.15f;
                    break;
                case SceneSemanticToken.CHAT:
                    score += 0.1f;
                    break;
            }

            switch (urgency)
            {
                case SceneUrgency.CRITICAL:
                    score += 0.45f;
                    break;
                case SceneUrgency.HIGH:
                    score += 0.35f;
                    break;
                case SceneUrgency.MEDIUM:
                    score += 0.2f;
                    break;
                case SceneUrgency.LOW:
                    score += 0.05f;
                    break;
            }

            return Mathf.Clamp01(score);
        }
    }
}
