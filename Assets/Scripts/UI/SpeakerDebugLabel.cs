using SceneTokens;
using UnityEngine;

namespace SemanticSpatialAudio.UI
{
    public class SpeakerDebugLabel : MonoBehaviour
    {
        public SpeakerObject speaker;
        public Vector3 localOffset = new Vector3(0f, 1.85f, 0f);
        public Color speakingColor = new Color(1f, 0.85f, 0.2f);
        public Color silentColor = Color.white;

        private TextMesh label;
        private Transform cameraTransform;

        private void Awake()
        {
            if (speaker == null)
            {
                speaker = GetComponent<SpeakerObject>();
            }

            cameraTransform = Camera.main != null ? Camera.main.transform : null;
            CreateLabel();
        }

        private void LateUpdate()
        {
            if (speaker == null || label == null)
            {
                return;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            label.transform.position = speaker.transform.position + localOffset;
            if (cameraTransform != null)
            {
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - cameraTransform.position);
            }

            label.color = speaker.IsSpeaking ? speakingColor : silentColor;
            label.text = string.Format(
                "{0}\n{1}\n{2}",
                speaker.speakerId,
                speaker.semanticToken,
                speaker.IsSpeaking ? "SPEAKING" : "SILENT");
        }

        private void CreateLabel()
        {
            var labelObject = new GameObject("SpeakerDebugLabel");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = localOffset;

            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 42;
            label.characterSize = 0.035f;
            label.color = silentColor;
        }
    }
}
