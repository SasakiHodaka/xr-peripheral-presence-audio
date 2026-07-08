using System.Collections.Generic;
using UnityEngine;

namespace SceneTokens
{
    public class ScenePacketizer : MonoBehaviour
    {
        public string senderId = "local";
        public string receiverId = "remote";
        public int packetHeaderBytes = 16;

        public ScenePacket BuildPacket(
            IReadOnlyList<SceneToken> generatedTokens,
            IReadOnlyList<SceneToken> selectedTokens,
            int sequenceNumber,
            string condition,
            string sessionId,
            string participantId,
            int trialIndex,
            float trialElapsed)
        {
            var packet = new ScenePacket
            {
                packetId = string.Format("{0:D6}", sequenceNumber),
                sequenceNumber = sequenceNumber,
                timestamp = Time.time,
                condition = condition,
                senderId = senderId,
                receiverId = receiverId,
                sessionId = sessionId,
                participantId = participantId,
                trialIndex = trialIndex,
                trialElapsed = trialElapsed,
                sendReason = selectedTokens != null && generatedTokens != null && selectedTokens.Count < generatedTokens.Count
                    ? "selected_scene_packet"
                    : "full_scene_packet",
                headerBytes = packetHeaderBytes
            };

            CountGeneratedTokens(packet, generatedTokens);
            AddSelectedTokens(packet, selectedTokens);

            packet.droppedTokenCount = Mathf.Max(0, packet.generatedTokenCount - packet.selectedTokenCount);
            packet.estimatedBytes = packet.headerBytes + packet.payloadBytes;
            return packet;
        }

        private static void CountGeneratedTokens(ScenePacket packet, IReadOnlyList<SceneToken> generatedTokens)
        {
            if (generatedTokens == null)
            {
                return;
            }

            packet.generatedTokenCount = generatedTokens.Count;
            for (var i = 0; i < generatedTokens.Count; i++)
            {
                var token = generatedTokens[i];
                if (SceneTokenMetrics.IsImportantToken(token))
                {
                    packet.importantTokenCount++;
                }
            }
        }

        private static void AddSelectedTokens(ScenePacket packet, IReadOnlyList<SceneToken> selectedTokens)
        {
            if (selectedTokens == null)
            {
                return;
            }

            packet.selectedTokenCount = selectedTokens.Count;
            for (var i = 0; i < selectedTokens.Count; i++)
            {
                var token = selectedTokens[i];
                if (token == null)
                {
                    continue;
                }

                packet.tokens.Add(token);
                packet.payloadBytes += token.estimatedBytes > 0
                    ? token.estimatedBytes
                    : SceneTokenMetrics.EstimateCompactSceneTokenBytes(token);
                packet.packetImportance = Mathf.Max(packet.packetImportance, token.importance);
                packet.packetPriority = Mathf.Max(packet.packetPriority, token.priority);

                if (SceneTokenMetrics.IsImportantToken(token))
                {
                    packet.importantTokenKeptCount++;
                }
            }
        }
    }
}
