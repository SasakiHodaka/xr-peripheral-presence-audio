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
        public string urgency;
        public string targetObjectId;
        public string utteranceText;
        public float semanticConfidence;
        public float priority;
        public bool selectedForTransmission;
        public string selectionReason;
        public string condition;
        public string participantId;
        public string sessionId;
        public int trialIndex;
        public float trialElapsed;
        public float azimuth;
        public float range;
        public float timestamp;

        public string ToCsvRow()
        {
            return string.Format(
                "{0:F3},{1},{2},{3},{4:F3},{5},{6:F2},{7:F2},{8},{9},{10},{11},{12},{13},{14},{15},{16:F2},{17:F2},{18},{19},{20}",
                timestamp,
                EscapeCsv(sessionId),
                EscapeCsv(participantId),
                trialIndex,
                trialElapsed,
                EscapeCsv(speakerId),
                azimuth,
                range,
                direction,
                distance,
                speakingState,
                turnState,
                semanticToken,
                urgency,
                EscapeCsv(targetObjectId),
                EscapeCsv(utteranceText),
                semanticConfidence,
                priority,
                selectedForTransmission ? "true" : "false",
                EscapeCsv(selectionReason),
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
        WARNING,
        EMERGENCY
    }

    public enum SceneUrgency
    {
        LOW,
        MEDIUM,
        HIGH,
        CRITICAL
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
