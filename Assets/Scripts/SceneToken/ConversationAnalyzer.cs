namespace SceneTokens
{
    public static class ConversationAnalyzer
    {
        public static int CountSpeakingSpeakers(SpeakerObject[] speakers)
        {
            var count = 0;

            if (speakers == null)
            {
                return count;
            }

            for (var i = 0; i < speakers.Length; i++)
            {
                if (speakers[i] != null && speakers[i].IsSpeaking)
                {
                    count++;
                }
            }

            return count;
        }

        public static SceneSpeakingState QuantizeSpeakingState(bool isSpeaking)
        {
            return isSpeaking ? SceneSpeakingState.SPEAKING : SceneSpeakingState.SILENT;
        }

        public static SceneTurnState QuantizeTurnState(bool isSpeaking, int speakingCount)
        {
            if (!isSpeaking) return SceneTurnState.LISTENER;
            if (speakingCount == 1) return SceneTurnState.TURN_HOLDER;
            return SceneTurnState.OVERLAPPER;
        }
    }
}
