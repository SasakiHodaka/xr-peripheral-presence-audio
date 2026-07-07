using System.IO;
using System.Text;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenLogger : MonoBehaviour
    {
        public string fileNamePrefix = "scene_tokens";
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
                File.WriteAllText(filePath, "timestamp,sessionId,participantId,trialIndex,trialElapsed,speakerId,azimuth,range,direction,distance,speakingState,turnState,semanticToken,urgency,targetObjectId,utteranceText,semanticConfidence,priority,selectedForTransmission,selectionReason,condition\n");
            }
        }

        private void OnDisable()
        {
            Flush();
        }

        public void Write(SceneToken token)
        {
            if (string.IsNullOrEmpty(filePath) || token == null)
            {
                return;
            }

            pendingRows.AppendLine(token.ToCsvRow());

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
