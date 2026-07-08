namespace SceneTokens
{
    public static class TokenSelector
    {
        public static bool ShouldTransmit(SceneToken token, float minimumImportance)
        {
            if (token == null)
            {
                return false;
            }

            if (token.semanticType == SceneSemanticToken.EMERGENCY.ToString() ||
                token.urgency == SceneUrgency.CRITICAL.ToString())
            {
                token.selectionReason = "critical";
                return true;
            }

            if (token.semanticType == SceneSemanticToken.WARNING.ToString() ||
                token.semanticType == SceneSemanticToken.INSTRUCTION.ToString())
            {
                token.selectionReason = "important_intent";
                return true;
            }

            if (token.speechActive && token.importance >= minimumImportance)
            {
                token.selectionReason = "importance";
                return true;
            }

            token.selectionReason = "low_importance";
            return false;
        }
    }

    public enum SceneTokenVisibility
    {
        IN_VIEW,
        OUT_OF_VIEW
    }
}
