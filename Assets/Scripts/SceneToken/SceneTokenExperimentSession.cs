using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenExperimentSession : MonoBehaviour
    {
        public SceneTokenConditionController conditionController;
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenEventLogger eventLogger;
        public SceneTokenScriptedConversation scriptedConversation;
        public bool startScriptedConversationWithTrial = true;
        public SceneTokenRenderCondition[] conditionOrder =
        {
            SceneTokenRenderCondition.TRADITIONAL,
            SceneTokenRenderCondition.DIRECTION_DISTANCE,
            SceneTokenRenderCondition.FULL_SCENE_TOKEN
        };
        public bool autoAdvanceCondition = true;
        public bool loopConditions;
        public float conditionDurationSeconds = 30f;
        public KeyCode startStopKey = KeyCode.Space;
        public KeyCode nextConditionKey = KeyCode.N;
        public KeyCode restartKey = KeyCode.R;
        public string participantId = "P00";
        public string sessionId;

        private bool isRunning;
        private int currentConditionIndex;
        private int trialIndex;
        private float trialStartTime;
        private string lastStatusMessage = "Ready";

        public bool IsRunning
        {
            get { return isRunning; }
        }

        public int TrialIndex
        {
            get { return trialIndex; }
        }

        public float TrialElapsedSeconds
        {
            get { return trialIndex > 0 ? Time.time - trialStartTime : 0f; }
        }

        public string LastStatusMessage
        {
            get { return lastStatusMessage; }
        }

        public bool HasCompletedAllConditions
        {
            get { return !isRunning && trialIndex >= GetConditionCount() && !loopConditions; }
        }

        public SceneTokenRenderCondition CurrentCondition
        {
            get { return GetCurrentCondition(); }
        }

        public string Summary
        {
            get
            {
                var condition = GetCurrentCondition();
                var elapsed = isRunning ? TrialElapsedSeconds : 0f;
                return string.Format(
                    "session={0} participant={1} trial={2}/{3} condition={4} running={5} t={6:F1}/{7:F1}",
                    string.IsNullOrEmpty(sessionId) ? "(not set)" : sessionId,
                    participantId,
                    trialIndex,
                    GetConditionCount(),
                    condition,
                    isRunning ? "yes" : "no",
                    elapsed,
                    conditionDurationSeconds);
            }
        }

        private void Reset()
        {
            conditionController = GetComponent<SceneTokenConditionController>();
            decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            eventLogger = GetComponent<SceneTokenEventLogger>();
            scriptedConversation = GetComponent<SceneTokenScriptedConversation>();
        }

        private void Awake()
        {
            if (conditionController == null)
            {
                conditionController = GetComponent<SceneTokenConditionController>();
            }

            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            if (eventLogger == null)
            {
                eventLogger = GetComponent<SceneTokenEventLogger>();
            }

            if (scriptedConversation == null)
            {
                scriptedConversation = GetComponent<SceneTokenScriptedConversation>();
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }

            ApplyCurrentCondition(false);
        }

        private void Update()
        {
            if (startStopKey != KeyCode.None && Input.GetKeyDown(startStopKey))
            {
                if (isRunning)
                {
                    StopSession("manual_stop");
                }
                else
                {
                    StartSession();
                }
            }

            if (restartKey != KeyCode.None && Input.GetKeyDown(restartKey))
            {
                RestartSession();
            }

            if (nextConditionKey != KeyCode.None && Input.GetKeyDown(nextConditionKey))
            {
                AdvanceCondition("manual_next");
            }

            if (!isRunning || !autoAdvanceCondition)
            {
                return;
            }

            if (Time.time - trialStartTime >= Mathf.Max(1f, conditionDurationSeconds))
            {
                AdvanceCondition("auto_next");
            }
        }

        public void StartSession()
        {
            if (isRunning)
            {
                return;
            }

            currentConditionIndex = Mathf.Clamp(currentConditionIndex, 0, GetConditionCount() - 1);
            isRunning = true;
            trialStartTime = Time.time;
            lastStatusMessage = "Session started";
            WriteEvent("session_start", GetSessionPayload("start"));
            BeginTrial();
        }

        public void StopSession(string reason)
        {
            if (!isRunning)
            {
                return;
            }

            EndTrial(reason);
            isRunning = false;
            lastStatusMessage = "Session stopped: " + reason;
            WriteEvent("session_stop", GetSessionPayload(reason));
        }

        public void RestartSession()
        {
            if (isRunning)
            {
                StopSession("restart");
            }

            currentConditionIndex = 0;
            trialIndex = 0;
            StartSession();
        }

        public void AdvanceCondition(string reason)
        {
            if (isRunning)
            {
                EndTrial(reason);
            }

            currentConditionIndex++;
            if (currentConditionIndex >= GetConditionCount())
            {
                if (!loopConditions)
                {
                    currentConditionIndex = GetConditionCount() - 1;
                    if (isRunning)
                    {
                        lastStatusMessage = "Session completed";
                        WriteEvent("session_stop", GetSessionPayload("completed"));
                        isRunning = false;
                    }
                    return;
                }

                currentConditionIndex = 0;
            }

            if (isRunning)
            {
                BeginTrial();
            }
            else
            {
                ApplyCurrentCondition(true);
            }
        }

        public void RecordDirectionResponse(string response, string expected, bool ambiguous)
        {
            RecordResponse("response_direction", response, expected, ambiguous);
        }

        public void RecordSpeakerResponse(string response, string expected, bool ambiguous)
        {
            RecordResponse("response_speaker", response, expected, ambiguous);
        }

        private void RecordResponse(string eventType, string response, string expected, bool ambiguous)
        {
            if (!isRunning)
            {
                lastStatusMessage = "Response ignored: session is not running";
                return;
            }

            var normalizedResponse = string.IsNullOrEmpty(response) ? string.Empty : response;
            var normalizedExpected = string.IsNullOrEmpty(expected) ? string.Empty : expected;
            var isCorrect = !ambiguous && normalizedResponse == normalizedExpected;

            lastStatusMessage = string.Format(
                "{0} response={1} expected={2} correct={3}",
                eventType,
                normalizedResponse,
                ambiguous ? "AMBIGUOUS" : normalizedExpected,
                isCorrect ? "yes" : "no");

            WriteEvent(
                eventType,
                string.Format(
                    "sessionId={0};participantId={1};trial={2};condition={3};response={4};expected={5};isCorrect={6};ambiguous={7};responseLatency={8:F3}",
                    sessionId,
                    participantId,
                    trialIndex,
                    GetCurrentCondition(),
                    normalizedResponse,
                    normalizedExpected,
                    isCorrect ? "true" : "false",
                    ambiguous ? "true" : "false",
                    TrialElapsedSeconds));
        }

        private void BeginTrial()
        {
            trialIndex++;
            trialStartTime = Time.time;
            ApplyCurrentCondition(true);
            lastStatusMessage = "Trial " + trialIndex + " started: " + GetCurrentCondition();
            WriteEvent("trial_start", GetSessionPayload("start"));

            if (startScriptedConversationWithTrial && scriptedConversation != null)
            {
                scriptedConversation.StartScript();
            }
        }

        private void EndTrial(string reason)
        {
            lastStatusMessage = "Trial " + trialIndex + " ended: " + reason;
            WriteEvent("trial_stop", GetSessionPayload(reason));

            if (startScriptedConversationWithTrial && scriptedConversation != null)
            {
                scriptedConversation.StopScript("trial_stop");
            }
        }

        private void ApplyCurrentCondition(bool logConditionChange)
        {
            var condition = GetCurrentCondition();

            if (conditionController != null)
            {
                conditionController.SetCondition(condition);
            }
            else if (decoderRenderer != null)
            {
                decoderRenderer.renderCondition = condition;
            }

            if (logConditionChange)
            {
                WriteEvent("condition_active", GetSessionPayload("active"));
            }
        }

        private SceneTokenRenderCondition GetCurrentCondition()
        {
            if (conditionOrder == null || conditionOrder.Length == 0)
            {
                return SceneTokenRenderCondition.FULL_SCENE_TOKEN;
            }

            currentConditionIndex = Mathf.Clamp(currentConditionIndex, 0, conditionOrder.Length - 1);
            return conditionOrder[currentConditionIndex];
        }

        private int GetConditionCount()
        {
            return conditionOrder != null && conditionOrder.Length > 0 ? conditionOrder.Length : 1;
        }

        private string GetSessionPayload(string reason)
        {
            return string.Format(
                "sessionId={0};participantId={1};trial={2};condition={3};reason={4};elapsed={5:F3}",
                sessionId,
                participantId,
                trialIndex,
                GetCurrentCondition(),
                reason,
                TrialElapsedSeconds);
        }

        private void WriteEvent(string eventType, string value)
        {
            if (eventLogger != null)
            {
                eventLogger.WriteEvent(eventType, value);
            }
        }
    }
}
