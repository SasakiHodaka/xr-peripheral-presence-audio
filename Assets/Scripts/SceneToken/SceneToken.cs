using System;
using UnityEngine;

namespace SceneTokens
{
    [Serializable]
    public class SceneToken
    {
        public string speakerId;
        public string direction;
        public string distance;
        public string speakingState;
        public string turnState;
        public string semanticToken;
        public string utteranceText;
        public float semanticConfidence;
        public string condition;
        public float azimuth;
        public float range;
        public float timestamp;

        public string ToCsvRow()
        {
            return string.Format(
                "{0:F3},{1},{2:F2},{3:F2},{4},{5},{6},{7},{8},{9},{10:F2},{11}",
                timestamp,
                EscapeCsv(speakerId),
                azimuth,
                range,
                direction,
                distance,
                speakingState,
                turnState,
                semanticToken,
                EscapeCsv(utteranceText),
                semanticConfidence,
                condition);
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }

    public enum SceneTokenDirection
    {
        FRONT,
        FRONT_RIGHT,
        RIGHT,
        BACK_RIGHT,
        BACK,
        BACK_LEFT,
        LEFT,
        FRONT_LEFT
    }

    public enum SceneTokenDistance
    {
        NEAR,
        MID,
        FAR
    }

    public enum SceneSpeakingState
    {
        SILENT,
        SPEAKING
    }

    public enum SceneTurnState
    {
        LISTENER,
        TURN_HOLDER,
        OVERLAPPER
    }

    public enum SceneSemanticToken
    {
        NONE,
        QUESTION,
        ANSWER,
        INSTRUCTION,
        AGREEMENT,
        DISAGREEMENT,
        CHAT,
        WARNING
    }

    public enum SceneTokenRenderCondition
    {
        TRADITIONAL = 1,
        DIRECTION_ONLY = 2,
        DIRECTION_DISTANCE = 3,
        DIRECTION_DISTANCE_SPEAKING = 4,
        FULL_SCENE_TOKEN = 5
    }
}
