using System.IO;
using System.Text;
using UnityEngine;

namespace SceneTokens
{
    public class ScenePacketLogger : MonoBehaviour
    {
        public string fileNamePrefix = "scene_packets";
        public bool writeHeaderOnStart = true;

        private string filePath;
        private readonly StringBuilder pendingRows = new StringBuilder();
        private float nextFlushTime;

        public string FilePath
        {
            get { return filePath; }
        }

        private void Start()
        {
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(Application.persistentDataPath, fileNamePrefix + "_" + timestamp + ".csv");

            if (writeHeaderOnStart)
            {
                File.WriteAllText(filePath, "timestamp,packetId,sequenceNumber,sessionId,participantId,trialIndex,trialElapsed,condition,senderId,receiverId,sendReason,generatedTokenCount,selectedTokenCount,droppedTokenCount,importantTokenCount,importantTokenKeptCount,packetImportance,packetPriority,headerBytes,payloadBytes,estimatedBytes,dropRatio,importantTokenKeptRatio\n");
            }
        }

        private void OnDisable()
        {
            Flush();
        }

        public void Write(ScenePacket packet)
        {
            if (string.IsNullOrEmpty(filePath) || packet == null)
            {
                return;
            }

            pendingRows.AppendLine(packet.ToCsvRow());

            if (Time.unscaledTime >= nextFlushTime)
            {
                Flush();
                nextFlushTime = Time.unscaledTime + 1f;
            }
        }

        public void Flush()
        {
            if (string.IsNullOrEmpty(filePath) || pendingRows.Length == 0)
            {
                return;
            }

            File.AppendAllText(filePath, pendingRows.ToString());
            pendingRows.Length = 0;
        }
    }
}
