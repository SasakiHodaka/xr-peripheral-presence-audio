using UnityEngine;

namespace SceneTokens
{
    [RequireComponent(typeof(AudioSource))]
    public class SpeakerObject : MonoBehaviour
    {
        public string speakerId = "Speaker";
        public AudioSource audioSource;
        public AudioClip speakingClip;
        public bool generateToneWhenClipMissing = true;
        public float generatedToneFrequency = 220f;
        [Range(0.01f, 0.5f)]
        public float generatedToneAmplitude = 0.22f;
        [Range(0f, 1f)]
        public float desktopSpatialBlend = 0.75f;
        public bool isSpeaking;
        public SceneSemanticToken semanticToken = SceneSemanticToken.CHAT;
        public SceneUrgency urgency = SceneUrgency.LOW;
        public string targetObjectId;
        [TextArea]
        public string utteranceText;
        [Range(0f, 1f)]
        public float semanticConfidence = 1f;
        public KeyCode toggleKey = KeyCode.None;
        public KeyCode holdToSpeakKey = KeyCode.None;
        public KeyCode cycleSemanticKey = KeyCode.None;

        private bool hasStartedClip;

        public bool IsSpeaking
        {
            get
            {
                return isSpeaking || (audioSource != null && audioSource.isPlaying);
            }
        }

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            ConfigureSpatialAudio();
            EnsureSpeakingClip();
            EnsureUtteranceText();
        }

        private void Update()
        {
            UpdateKeyboardState();
            UpdateAudioPlayback();
        }

        public void SetSpeaking(bool value)
        {
            isSpeaking = value;

            if (!value && audioSource != null)
            {
                audioSource.Stop();
                hasStartedClip = false;
            }
        }

        private void ConfigureSpatialAudio()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = desktopSpatialBlend;
            audioSource.spatialize = true;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 20f;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = Mathf.Max(audioSource.volume, 0.8f);
        }

        private void EnsureSpeakingClip()
        {
            if (!generateToneWhenClipMissing || speakingClip != null)
            {
                return;
            }

            speakingClip = CreateToneClip(speakerId + "_GeneratedVoiceTone", generatedToneFrequency, generatedToneAmplitude);
        }

        private void UpdateKeyboardState()
        {
            if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
            {
                SetSpeaking(!isSpeaking);
            }

            if (holdToSpeakKey != KeyCode.None)
            {
                SetSpeaking(Input.GetKey(holdToSpeakKey));
            }

            if (cycleSemanticKey != KeyCode.None && Input.GetKeyDown(cycleSemanticKey))
            {
                CycleSemanticToken();
            }
        }

        private void CycleSemanticToken()
        {
            var nextValue = (int)semanticToken + 1;
            var maxValue = (int)SceneSemanticToken.EMERGENCY;

            if (nextValue > maxValue)
            {
                nextValue = (int)SceneSemanticToken.QUESTION;
            }

            semanticToken = (SceneSemanticToken)nextValue;
            utteranceText = GetDefaultUtterance(semanticToken);
        }

        private void UpdateAudioPlayback()
        {
            if (audioSource == null)
            {
                return;
            }

            if (!isSpeaking)
            {
                hasStartedClip = false;
                return;
            }

            if (speakingClip == null)
            {
                return;
            }

            if (!hasStartedClip)
            {
                audioSource.clip = speakingClip;
                audioSource.loop = true;
                audioSource.Play();
                hasStartedClip = true;
            }
        }

        private static AudioClip CreateToneClip(string clipName, float frequency, float amplitude)
        {
            const int sampleRate = 24000;
            const float durationSeconds = 0.5f;
            var sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
            var samples = new float[sampleCount];
            var safeFrequency = Mathf.Max(60f, frequency);
            var safeAmplitude = Mathf.Clamp(amplitude, 0.01f, 0.5f);

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Sin(Mathf.PI * i / (sampleCount - 1));
                samples[i] = Mathf.Sin(2f * Mathf.PI * safeFrequency * t) * safeAmplitude * envelope;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void EnsureUtteranceText()
        {
            if (!string.IsNullOrWhiteSpace(utteranceText))
            {
                return;
            }

            utteranceText = GetDefaultUtterance(semanticToken);
        }

        private static string GetDefaultUtterance(SceneSemanticToken token)
        {
            switch (token)
            {
                case SceneSemanticToken.QUESTION:
                    return "Can you check this object for me?";
                case SceneSemanticToken.ANSWER:
                    return "Yes, that part looks aligned.";
                case SceneSemanticToken.INSTRUCTION:
                    return "Move this object forward, please.";
                case SceneSemanticToken.AGREEMENT:
                    return "I think that direction is good.";
                case SceneSemanticToken.DISAGREEMENT:
                    return "I think that is slightly off.";
                case SceneSemanticToken.WARNING:
                    return "Be careful, the direction is wrong.";
                case SceneSemanticToken.EMERGENCY:
                    return "Stop the equipment immediately.";
                case SceneSemanticToken.CHAT:
                    return "Let's continue working like this.";
                default:
                    return string.Empty;
            }
        }
    }
}
