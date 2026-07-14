using System.IO;
using UnityEngine;

public sealed class ExperimentLogger : MonoBehaviour
{
    private const string TokenLogFilePrefix = "generated_scene_tokens";
    private const string TokenLogFileSuffix = ".csv";

    [SerializeField]
    private string logDirectoryName = "scene_token_ground_truth";

    public string LogDirectory { get; private set; }
    public string TokenLogPath { get; private set; }

    private void Awake()
    {
        LogDirectory = Path.Combine(
            Application.persistentDataPath,
            logDirectoryName
        );

        Directory.CreateDirectory(LogDirectory);
        TokenLogPath = Path.Combine(
            LogDirectory,
            $"{TokenLogFilePrefix}_{System.DateTime.Now:yyyyMMdd_HHmmss}{TokenLogFileSuffix}"
        );

        File.WriteAllText(
            TokenLogPath,
            "scenarioId,eventId,sequence,expectedTime,speakerId,direction,targetObjectId,utteranceId,taskState,priority,communicationLevel,selected,selectionReason,packetSeq,packetFlags,packetBytes,expireTime,presentationMode,presentationDescription,relativeAngle,sourceX,sourceY,sourceZ\n"
        );

        Debug.Log($"Experiment Logger Ready: {TokenLogPath}");
    }

    public void LogToken(
        GeneratedSceneToken token,
        SelectionResult selection,
        SemanticPacket packet,
        PresentationResult presentation
    )
    {
        if (token == null)
        {
            return;
        }

        string communicationLevel = selection != null ? selection.level.ToString() : "";
        string selected = selection != null && selection.selected ? "true" : "false";
        string selectionReason = selection != null ? selection.reason : "";
        string packetSeq = packet != null ? packet.sequence.ToString() : "";
        string packetFlags = packet != null ? packet.flags.ToString() : "";
        string packetBytes = packet != null ? packet.GetPacketBytes().ToString() : "";
        string expireTime = packet != null ? packet.expireTime.ToString("F3") : "";
        string presentationMode = presentation != null ? presentation.mode : "";
        string presentationDescription = presentation != null ? presentation.description : "";

        string row =
            $"{Escape(token.scenarioId)}," +
            $"{Escape(token.eventId)}," +
            $"{token.sequence}," +
            $"{token.expectedTime:F3}," +
            $"{Escape(token.speakerId)}," +
            $"{Escape(token.direction)}," +
            $"{Escape(token.targetObjectId)}," +
            $"{Escape(token.utteranceId)}," +
            $"{Escape(token.taskState)}," +
            $"{token.priority}," +
            $"{Escape(communicationLevel)}," +
            $"{selected}," +
            $"{Escape(selectionReason)}," +
            $"{packetSeq}," +
            $"{packetFlags}," +
            $"{packetBytes}," +
            $"{expireTime}," +
            $"{Escape(presentationMode)}," +
            $"{Escape(presentationDescription)}," +
            $"{token.relativeAngle:F3}," +
            $"{token.sourceX:F3}," +
            $"{token.sourceY:F3}," +
            $"{token.sourceZ:F3}";

        File.AppendAllText(TokenLogPath, row + "\n");

        Debug.Log(
            "Scene Token Generated," +
            $"{token.scenarioId}," +
            $"{token.eventId}," +
            $"{token.expectedTime:F3}," +
            $"{token.speakerId}," +
            $"{token.direction}," +
            $"{token.targetObjectId}," +
            $"priority={token.priority}," +
            $"level={communicationLevel}," +
            $"packetBytes={packetBytes}," +
            $"flags={packetFlags}," +
            $"presentation={presentationMode}"
        );
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
