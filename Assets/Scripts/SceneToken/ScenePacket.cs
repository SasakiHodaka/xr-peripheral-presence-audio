using System;
using System.Collections.Generic;

namespace SceneTokens
{
    [Serializable]
    public class ScenePacket
    {
        public string packetId;
        public int sequenceNumber;
        public float timestamp;
        public string condition;
        public string senderId;
        public string receiverId;
        public string sessionId;
        public string participantId;
        public int trialIndex;
        public float trialElapsed;
        public string sendReason;
        public int generatedTokenCount;
        public int selectedTokenCount;
        public int droppedTokenCount;
        public int importantTokenCount;
        public int importantTokenKeptCount;
        public float packetImportance;
        public float packetPriority;
        public int headerBytes;
        public int payloadBytes;
        public int estimatedBytes;
        public readonly List<SceneToken> tokens = new List<SceneToken>();

        public float DropRatio
        {
            get { return generatedTokenCount > 0 ? (float)droppedTokenCount / generatedTokenCount : 0f; }
        }

        public float ImportantTokenKeptRatio
        {
            get { return importantTokenCount > 0 ? (float)importantTokenKeptCount / importantTokenCount : 1f; }
        }

        public string ToCsvRow()
        {
            return string.Format(
                "{0:F3},{1},{2},{3},{4},{5},{6:F3},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16:F4},{17:F4},{18},{19},{20},{21:F4},{22:F4}",
                timestamp,
                SceneToken.EscapeCsv(packetId),
                sequenceNumber,
                SceneToken.EscapeCsv(sessionId),
                SceneToken.EscapeCsv(participantId),
                trialIndex,
                trialElapsed,
                condition,
                SceneToken.EscapeCsv(senderId),
                SceneToken.EscapeCsv(receiverId),
                SceneToken.EscapeCsv(sendReason),
                generatedTokenCount,
                selectedTokenCount,
                droppedTokenCount,
                importantTokenCount,
                importantTokenKeptCount,
                packetImportance,
                packetPriority,
                headerBytes,
                payloadBytes,
                estimatedBytes,
                DropRatio,
                ImportantTokenKeptRatio);
        }
    }
}
