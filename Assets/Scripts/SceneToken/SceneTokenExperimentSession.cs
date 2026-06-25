using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenExperimentSession : MonoBehaviour
    {
        public SceneTokenConditionController conditionController;
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenEventLogger eventLogger;
        public SceneTokenRenderCondition[] conditionOrder =
        {
            SceneTokenRenderCondition.TRADITIONAL,
            SceneTokenRenderCondition.DIRECTION_ONLY,
            SceneTokenRenderCondition.DIRECTION_DISTANCE,
            SceneTokenRenderCondition.DIRECTION_DISTANCE_SPEAKING,
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

        public string Summary
        {
            get
            {
                var condition = GetCurrentCondition();
                var elapsed = isRunning ? TrialElapsedSeconds : 0f;
                return string.Format(
                    "session={0} participant={1} trial={2} condition={3} running={4} t={5:F1}/{6:F1}",
                    string.IsNullOrEmpty(sessionId) ? "(not set)" : sessionId,
                    participantId,
                    trialIndex,
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

        private void BeginTrial()
        {
            trialIndex++;
            trialStartTime = Time.time;
            ApplyCurrentCondition(true);
            WriteEvent("trial_start", GetSessionPayload("start"));
        }

        private void EndTrial(string reason)
        {
            WriteEvent("trial_stop", GetSessionPayload(reason));
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
