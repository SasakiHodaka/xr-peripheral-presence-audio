using UnityEngine;

namespace SceneTokens
{
    public static class ImportanceCalculator
    {
        public static float Calculate(SceneToken token)
        {
            if (token == null)
            {
                return 0f;
            }

            var score = 0f;

            if (token.range <= 1.5f)
            {
                score += 0.2f;
            }

            if (token.visibility == SceneTokenVisibility.OUT_OF_VIEW.ToString())
            {
                score += 0.2f;
            }

            if (token.speechActive)
            {
                score += 0.2f;
            }

            if (token.semanticType == SceneSemanticToken.INSTRUCTION.ToString() ||
                token.semanticType == SceneSemanticToken.WARNING.ToString())
            {
                score += 0.2f;
            }

            if (token.semanticType == SceneSemanticToken.EMERGENCY.ToString())
            {
                score += 0.3f;
            }

            if (!string.IsNullOrEmpty(token.targetObjectId))
            {
                score += 0.1f;
            }

            if (token.urgency == SceneUrgency.HIGH.ToString())
            {
                score += 0.15f;
            }
            else if (token.urgency == SceneUrgency.CRITICAL.ToString())
            {
                score += 0.25f;
            }

            return Mathf.Clamp01(score);
        }
    }
}
