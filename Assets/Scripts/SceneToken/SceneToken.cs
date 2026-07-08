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
        public string visibility;
        public string speakingState;
        public bool speechActive;
        public float rms;
        public string turnState;
        public string semanticToken;
        public string semanticType;
        public string urgency;
        public string targetObjectId;
        public string utteranceText;
        public float semanticConfidence;
        public float importance;
        public float priority;
        public bool selected;
        public bool selectedForTransmission;
        public string selectionReason;
        public int estimatedBytes;
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
                "{0:F3},{1},{2},{3},{4:F3},{5},{6:F2},{7:F2},{8},{9},{10},{11},{12},{13:F3},{14},{15},{16},{17},{18},{19},{20:F2},{21:F3},{22:F2},{23},{24},{25},{26},{27}",
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
                visibility,
                speakingState,
                speechActive ? "true" : "false",
                rms,
                turnState,
                semanticToken,
                semanticType,
                urgency,
                EscapeCsv(targetObjectId),
                EscapeCsv(utteranceText),
                semanticConfidence,
                importance,
                priority,
                selected ? "true" : "false",
                selectedForTransmission ? "true" : "false",
                EscapeCsv(selectionReason),
                estimatedBytes,
                condition);
        }

        public static string EscapeCsv(string value)
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
        C1_TRADITIONAL = 1,
        C2_DIRECTION_DISTANCE = 2,
        C3_FULL_SCENE_TOKEN = 3,
        C4_SELECTED_SCENE_TOKEN = 4,

        TRADITIONAL = C1_TRADITIONAL,
        DIRECTION_DISTANCE = C2_DIRECTION_DISTANCE,
        FULL_SCENE_TOKEN = C3_FULL_SCENE_TOKEN,
        SELECTED_SCENE_TOKEN = C4_SELECTED_SCENE_TOKEN,
        DIRECTION_ONLY = 5,
        DIRECTION_DISTANCE_SPEAKING = 6
    }
}
