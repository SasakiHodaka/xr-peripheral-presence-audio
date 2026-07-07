using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenConditionController : MonoBehaviour
    {
        public SceneTokenDecoderRenderer decoderRenderer;
        public SceneTokenEventLogger eventLogger;

        private void Reset()
        {
            decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            eventLogger = GetComponent<SceneTokenEventLogger>();
        }

        private void Awake()
        {
            if (decoderRenderer == null)
            {
                decoderRenderer = GetComponent<SceneTokenDecoderRenderer>();
            }

            if (eventLogger == null)
            {
                eventLogger = GetComponent<SceneTokenEventLogger>();
            }
        }

        private void Update()
        {
            if (decoderRenderer == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SetCondition(SceneTokenRenderCondition.TRADITIONAL);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetCondition(SceneTokenRenderCondition.DIRECTION_DISTANCE);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetCondition(SceneTokenRenderCondition.FULL_SCENE_TOKEN);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetCondition(SceneTokenRenderCondition.DIRECTION_ONLY);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetCondition(SceneTokenRenderCondition.DIRECTION_DISTANCE_SPEAKING);
        }

        public void SetCondition(SceneTokenRenderCondition condition)
        {
            if (decoderRenderer.renderCondition == condition)
            {
                return;
            }

            decoderRenderer.renderCondition = condition;
            if (eventLogger != null)
            {
                eventLogger.WriteEvent("condition", condition.ToString());
            }
        }
    }
}
