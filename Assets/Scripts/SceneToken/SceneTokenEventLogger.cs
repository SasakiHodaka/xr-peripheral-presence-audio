using System.IO;
using System.Text;
using UnityEngine;

namespace SceneTokens
{
    public class SceneTokenEventLogger : MonoBehaviour
    {
        public string fileNamePrefix = "scene_token_events";

        private string filePath;
        private readonly StringBuilder pendingRows = new StringBuilder();

        private void Start()
        {
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            filePath = Path.Combine(Application.persistentDataPath, fileNamePrefix + "_" + timestamp + ".csv");
            File.WriteAllText(filePath, "timestamp,eventType,value\n");
        }

        private void OnDisable()
        {
            Flush();
        }

        public void WriteEvent(string eventType, string value)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            pendingRows.AppendFormat("{0:F3},{1},{2}\n", Time.time, EscapeCsv(eventType), EscapeCsv(value));
            Flush();
        }

        private void Flush()
        {
            if (string.IsNullOrEmpty(filePath) || pendingRows.Length == 0)
            {
                return;
            }

            File.AppendAllText(filePath, pendingRows.ToString());
            pendingRows.Length = 0;
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
}
