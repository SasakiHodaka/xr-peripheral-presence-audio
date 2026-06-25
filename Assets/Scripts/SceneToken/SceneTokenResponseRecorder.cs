using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenResponseRecorder : MonoBehaviour
    {
        public SceneTokenManager tokenManager;
        public SceneTokenExperimentSession experimentSession;
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenEventLogger eventLogger;
        public SpeakerObject[] speakers;
        public bool recordOnlyDuringExperimentSession = true;
        public bool showResponseHud = true;

        private string activeTargetSpeakerId;
        private float activeTargetStartTime;

        private static readonly SceneTokenDirection[] DirectionOrder =
        {
            SceneTokenDirection.FRONT_LEFT,
            SceneTokenDirection.FRONT,
            SceneTokenDirection.FRONT_RIGHT,
            SceneTokenDirection.LEFT,
            SceneTokenDirection.RIGHT,
            SceneTokenDirection.BACK_LEFT,
            SceneTokenDirection.BACK,
            SceneTokenDirection.BACK_RIGHT
        };

        private void Reset()
        {
            tokenManager = GetComponent<SceneTokenManager>();
            experimentSession = GetComponent<SceneTokenExperimentSession>();
            decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            eventLogger = GetComponent<SceneTokenEventLogger>();
            speakers = FindObjectsOfType<SpeakerObject>();
        }

        private void Awake()
        {
            if (tokenManager == null)
            {
                tokenManager = GetComponent<SceneTokenManager>();
            }

            if (experimentSession == null)
            {
                experimentSession = GetComponent<SceneTokenExperimentSession>();
            }

            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            if (eventLogger == null)
            {
                eventLogger = GetComponent<SceneTokenEventLogger>();
            }

            if (speakers == null || speakers.Length == 0)
            {
                speakers = FindObjectsOfType<SpeakerObject>();
            }
        }

        private void Update()
        {
            UpdateActiveTarget();
            RecordDirectionKeys();
            RecordSpeakerKeys();
        }

        private void OnGUI()
        {
            if (!showResponseHud)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 450f, 620f, 210f), GUI.skin.box);
            GUILayout.Label("Participant Response");
            GUILayout.Label("Direction: numpad 7/8/9/4/6/1/2/3, Speaker: F1/F2/F3");

            if (!ShouldRecordResponse())
            {
                GUILayout.Label("Responses paused until the experiment session starts.");
            }

            GUILayout.Label("Direction guess");
            GUILayout.BeginHorizontal();
            for (var i = 0; i < DirectionOrder.Length; i++)
            {
                var direction = DirectionOrder[i];
                if (GUILayout.Button(direction.ToString(), GUILayout.Width(72f)))
                {
                    RecordDirectionResponse(direction, "hud");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Speaker guess");
            GUILayout.BeginHorizontal();
            if (speakers != null)
            {
                for (var i = 0; i < speakers.Length; i++)
                {
                    var speaker = speakers[i];
                    if (speaker == null)
                    {
                        continue;
                    }

                    if (GUILayout.Button(speaker.speakerId, GUILayout.Width(72f)))
                    {
                        RecordSpeakerResponse(speaker.speakerId, "hud");
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void RecordDirectionKeys()
        {
            if (Input.GetKeyDown(KeyCode.Keypad8)) RecordDirectionResponse(SceneTokenDirection.FRONT, "keypad8");
            if (Input.GetKeyDown(KeyCode.Keypad9)) RecordDirectionResponse(SceneTokenDirection.FRONT_RIGHT, "keypad9");
            if (Input.GetKeyDown(KeyCode.Keypad6)) RecordDirectionResponse(SceneTokenDirection.RIGHT, "keypad6");
            if (Input.GetKeyDown(KeyCode.Keypad3)) RecordDirectionResponse(SceneTokenDirection.BACK_RIGHT, "keypad3");
            if (Input.GetKeyDown(KeyCode.Keypad2)) RecordDirectionResponse(SceneTokenDirection.BACK, "keypad2");
            if (Input.GetKeyDown(KeyCode.Keypad1)) RecordDirectionResponse(SceneTokenDirection.BACK_LEFT, "keypad1");
            if (Input.GetKeyDown(KeyCode.Keypad4)) RecordDirectionResponse(SceneTokenDirection.LEFT, "keypad4");
            if (Input.GetKeyDown(KeyCode.Keypad7)) RecordDirectionResponse(SceneTokenDirection.FRONT_LEFT, "keypad7");
        }

        private void RecordSpeakerKeys()
        {
            if (Input.GetKeyDown(KeyCode.F1)) RecordSpeakerByIndex(0, "f1");
            if (Input.GetKeyDown(KeyCode.F2)) RecordSpeakerByIndex(1, "f2");
            if (Input.GetKeyDown(KeyCode.F3)) RecordSpeakerByIndex(2, "f3");
        }

        private void RecordSpeakerByIndex(int index, string source)
        {
            if (speakers == null || index < 0 || index >= speakers.Length || speakers[index] == null)
            {
                return;
            }

            RecordSpeakerResponse(speakers[index].speakerId, source);
        }

        public void RecordDirectionResponse(SceneTokenDirection direction, string source)
        {
            var target = GetCurrentTargetToken();
            WriteResponse(
                "response_direction",
                direction.ToString(),
                target != null ? target.direction : string.Empty,
                IsTargetAmbiguous(),
                source);
        }

        public void RecordSpeakerResponse(string speakerId, string source)
        {
            var target = GetCurrentTargetToken();
            WriteResponse(
                "response_speaker",
                speakerId,
                target != null ? target.speakerId : string.Empty,
                IsTargetAmbiguous(),
                source);
        }

        private void WriteResponse(string responseType, string responseValue, string targetValue, bool ambiguous, string source)
        {
            if (eventLogger == null || !ShouldRecordResponse())
            {
                return;
            }

            eventLogger.WriteEvent(responseType, BuildPayload(responseValue, targetValue, ambiguous, source));
        }

        private bool ShouldRecordResponse()
        {
            if (!recordOnlyDuringExperimentSession || experimentSession == null)
            {
                return true;
            }

            return experimentSession.IsRunning;
        }

        private string BuildPayload(string responseValue, string targetValue, bool ambiguous, string source)
        {
            var sessionId = experimentSession != null ? experimentSession.sessionId : string.Empty;
            var participantId = experimentSession != null ? experimentSession.participantId : string.Empty;
            var trialIndex = experimentSession != null ? experimentSession.TrialIndex : 0;
            var trialElapsed = experimentSession != null ? experimentSession.TrialElapsedSeconds : 0f;
            var condition = decoderRenderer != null ? decoderRenderer.renderCondition.ToString() : string.Empty;
            var hasTarget = !string.IsNullOrEmpty(targetValue);
            var isCorrect = hasTarget && !ambiguous && responseValue == targetValue;
            var responseLatency = GetResponseLatencySeconds();

            return string.Format(
                "sessionId={0};participantId={1};trial={2};trialElapsed={3:F3};condition={4};response={5};target={6};isCorrect={7};ambiguous={8};responseLatency={9:F3};source={10}",
                sessionId,
                participantId,
                trialIndex,
                trialElapsed,
                condition,
                responseValue,
                targetValue,
                isCorrect ? "true" : "false",
                ambiguous ? "true" : "false",
                responseLatency,
                source);
        }

        private void UpdateActiveTarget()
        {
            var target = GetCurrentTargetToken();
            var targetSpeakerId = target != null && !IsTargetAmbiguous() ? target.speakerId : string.Empty;

            if (activeTargetSpeakerId == targetSpeakerId)
            {
                return;
            }

            activeTargetSpeakerId = targetSpeakerId;
            activeTargetStartTime = !string.IsNullOrEmpty(activeTargetSpeakerId) ? Time.time : 0f;
        }

        private float GetResponseLatencySeconds()
        {
            if (string.IsNullOrEmpty(activeTargetSpeakerId) || activeTargetStartTime <= 0f)
            {
                return -1f;
            }

            return Mathf.Max(0f, Time.time - activeTargetStartTime);
        }

        private SceneToken GetCurrentTargetToken()
        {
            if (tokenManager == null || tokenManager.LatestTokens == null)
            {
                return null;
            }

            SceneToken speakingToken = null;
            for (var i = 0; i < tokenManager.LatestTokens.Count; i++)
            {
                var token = tokenManager.LatestTokens[i];
                if (token == null || token.speakingState != SceneSpeakingState.SPEAKING.ToString())
                {
                    continue;
                }

                if (token.turnState == SceneTurnState.TURN_HOLDER.ToString())
                {
                    return token;
                }

                if (speakingToken == null)
                {
                    speakingToken = token;
                }
            }

            return speakingToken;
        }

        private bool IsTargetAmbiguous()
        {
            if (tokenManager == null || tokenManager.LatestTokens == null)
            {
                return true;
            }

            var speakingCount = 0;
            for (var i = 0; i < tokenManager.LatestTokens.Count; i++)
            {
                var token = tokenManager.LatestTokens[i];
                if (token != null && token.speakingState == SceneSpeakingState.SPEAKING.ToString())
                {
                    speakingCount++;
                }
            }

            return speakingCount != 1;
        }
    }
}
