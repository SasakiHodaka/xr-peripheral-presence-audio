using System;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenScriptedConversation : MonoBehaviour
    {
        [Serializable]
        public class ScriptedUtterance
        {
            public string speakerId = "A";
            public float startTime;
            public float duration = 2f;
            public SceneSemanticToken semanticToken = SceneSemanticToken.CHAT;
            [TextArea]
            public string utteranceText;
            [Range(0f, 1f)]
            public float semanticConfidence = 1f;
        }

        public SpeakerObject[] speakers;
        public SceneTokenExperimentSession experimentSession;
        public SceneTokenEventLogger eventLogger;
        public bool autoStartWithExperimentSession = true;
        public bool loopScript = true;
        public KeyCode startStopKey = KeyCode.T;
        public KeyCode stopKey = KeyCode.Y;
        public ScriptedUtterance[] utterances =
        {
            new ScriptedUtterance
            {
                speakerId = "A",
                startTime = 0.5f,
                duration = 2.0f,
                semanticToken = SceneSemanticToken.QUESTION,
                utteranceText = "Can you check this part?",
                semanticConfidence = 0.95f
            },
            new ScriptedUtterance
            {
                speakerId = "B",
                startTime = 3.0f,
                duration = 2.0f,
                semanticToken = SceneSemanticToken.ANSWER,
                utteranceText = "Yes, that part is correct.",
                semanticConfidence = 0.95f
            },
            new ScriptedUtterance
            {
                speakerId = "A",
                startTime = 5.4f,
                duration = 1.8f,
                semanticToken = SceneSemanticToken.INSTRUCTION,
                utteranceText = "Move this object to the front.",
                semanticConfidence = 0.9f
            },
            new ScriptedUtterance
            {
                speakerId = "C",
                startTime = 6.2f,
                duration = 2.0f,
                semanticToken = SceneSemanticToken.WARNING,
                utteranceText = "Be careful, that is the wrong direction.",
                semanticConfidence = 0.9f
            },
            new ScriptedUtterance
            {
                speakerId = "B",
                startTime = 9.0f,
                duration = 1.6f,
                semanticToken = SceneSemanticToken.AGREEMENT,
                utteranceText = "I agree with that plan.",
                semanticConfidence = 0.85f
            }
        };

        private bool isRunning;
        private bool wasExperimentRunning;
        private int lastExperimentTrialIndex;
        private float scriptStartTime;

        public bool IsRunning
        {
            get { return isRunning; }
        }

        public string Summary
        {
            get
            {
                return string.Format(
                    "scriptedConversation={0} t={1:F1}/{2:F1}",
                    isRunning ? "running" : "stopped",
                    isRunning ? GetElapsedTime() : 0f,
                    GetScriptDuration());
            }
        }

        private void Reset()
        {
            speakers = FindObjectsOfType<SpeakerObject>();
            experimentSession = GetComponent<SceneTokenExperimentSession>();
            eventLogger = GetComponent<SceneTokenEventLogger>();
        }

        private void Awake()
        {
            if (speakers == null || speakers.Length == 0)
            {
                speakers = FindObjectsOfType<SpeakerObject>();
            }

            if (experimentSession == null)
            {
                experimentSession = GetComponent<SceneTokenExperimentSession>();
            }

            if (eventLogger == null)
            {
                eventLogger = GetComponent<SceneTokenEventLogger>();
            }
        }

        private void Update()
        {
            UpdateKeyboardState();
            UpdateExperimentSessionState();

            if (!isRunning)
            {
                return;
            }

            ApplyScriptAtTime(GetElapsedTime());
        }

        public void StartScript()
        {
            isRunning = true;
            scriptStartTime = Time.time;
            WriteEvent("script_start", "duration=" + GetScriptDuration().ToString("F3"));
        }

        public void StopScript(string reason)
        {
            if (!isRunning)
            {
                return;
            }

            ClearSpeakers();
            WriteEvent("script_stop", string.Format("reason={0};elapsed={1:F3}", reason, GetElapsedTime()));
            isRunning = false;
        }

        private void UpdateKeyboardState()
        {
            if (startStopKey != KeyCode.None && Input.GetKeyDown(startStopKey))
            {
                if (isRunning)
                {
                    StopScript("manual_toggle");
                }
                else
                {
                    StartScript();
                }
            }

            if (stopKey != KeyCode.None && Input.GetKeyDown(stopKey))
            {
                StopScript("manual_stop");
            }
        }

        private void UpdateExperimentSessionState()
        {
            if (!autoStartWithExperimentSession || experimentSession == null)
            {
                return;
            }

            if (experimentSession.IsRunning && !wasExperimentRunning)
            {
                StartScript();
            }
            else if (experimentSession.IsRunning && experimentSession.TrialIndex != lastExperimentTrialIndex)
            {
                StartScript();
            }

            if (!experimentSession.IsRunning && wasExperimentRunning)
            {
                StopScript("session_stop");
            }

            wasExperimentRunning = experimentSession.IsRunning;
            lastExperimentTrialIndex = experimentSession.TrialIndex;
        }

        private void ApplyScriptAtTime(float elapsed)
        {
            var duration = GetScriptDuration();
            if (duration <= 0f)
            {
                ClearSpeakers();
                return;
            }

            if (elapsed > duration)
            {
                if (!loopScript)
                {
                    StopScript("completed");
                    return;
                }

                scriptStartTime = Time.time;
                elapsed = 0f;
                WriteEvent("script_loop", "duration=" + duration.ToString("F3"));
            }

            var activeSpeakers = new bool[speakers != null ? speakers.Length : 0];

            for (var i = 0; i < utterances.Length; i++)
            {
                var utterance = utterances[i];
                if (utterance == null)
                {
                    continue;
                }

                if (elapsed < utterance.startTime || elapsed > utterance.startTime + Mathf.Max(0f, utterance.duration))
                {
                    continue;
                }

                ApplyUtterance(utterance, activeSpeakers);
            }

            for (var i = 0; i < activeSpeakers.Length; i++)
            {
                if (!activeSpeakers[i] && speakers[i] != null)
                {
                    speakers[i].SetSpeaking(false);
                }
            }
        }

        private void ApplyUtterance(ScriptedUtterance utterance, bool[] activeSpeakers)
        {
            var speakerIndex = FindSpeakerIndex(utterance.speakerId);
            if (speakerIndex < 0)
            {
                return;
            }

            var speaker = speakers[speakerIndex];
            if (speaker == null)
            {
                return;
            }

            speaker.semanticToken = utterance.semanticToken;
            speaker.utteranceText = utterance.utteranceText;
            speaker.semanticConfidence = utterance.semanticConfidence;
            speaker.SetSpeaking(true);
            activeSpeakers[speakerIndex] = true;
        }

        private int FindSpeakerIndex(string speakerId)
        {
            if (speakers == null)
            {
                return -1;
            }

            for (var i = 0; i < speakers.Length; i++)
            {
                if (speakers[i] != null && speakers[i].speakerId == speakerId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void ClearSpeakers()
        {
            if (speakers == null)
            {
                return;
            }

            for (var i = 0; i < speakers.Length; i++)
            {
                if (speakers[i] != null)
                {
                    speakers[i].SetSpeaking(false);
                }
            }
        }

        private float GetElapsedTime()
        {
            return Time.time - scriptStartTime;
        }

        private float GetScriptDuration()
        {
            var duration = 0f;

            if (utterances == null)
            {
                return duration;
            }

            for (var i = 0; i < utterances.Length; i++)
            {
                if (utterances[i] == null)
                {
                    continue;
                }

                duration = Mathf.Max(duration, utterances[i].startTime + Mathf.Max(0f, utterances[i].duration));
            }

            return duration;
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
