using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class PeripheralStateLogger : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralTrialController trialController;

    [Header("Experiment")]
    public string participantId = "P001";
    public string conditionLabel = "demo";
    public string trialId = "T001";

    [Header("CSV")]
    public string fileName = "peripheral_state_log.csv";
    public bool includeExperimentMetadataInFileName = true;
    public bool appendTimestampToFileName = true;
    public float logInterval = 0.1f;
    public bool logNoneState = true;
    public bool flushEachWrite = true;

    private StreamWriter writer;
    private float timer;
    private string filePath;
    private bool warnedMissingDetector;
    private bool warnedMissingUserHead;

    public string FilePath
    {
        get { return filePath; }
    }

    private void Awake()
    {
        if (detector == null)
            detector = GetComponent<PeripheralStateDetector>();

        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();
    }

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, BuildFileName());
        writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("participantId,conditionLabel,trialId,time,trialElapsed,trialDuration,targetId,state,outOfView,approaching,speaking,gazing,near,crossing,distance,viewAngle,radialSpeed,lateralSpeed,localX,localY,localZ");
        writer.Flush();

        Debug.Log("Peripheral CSV created: " + filePath);
    }

    private void Update()
    {
        if (writer == null) return;
        if (trialController != null && !trialController.IsRunning) return;

        if (detector == null)
        {
            if (!warnedMissingDetector)
            {
                Debug.LogWarning("PeripheralStateLogger has no detector assigned.", this);
                warnedMissingDetector = true;
            }

            return;
        }

        if (detector.userHead == null)
        {
            if (!warnedMissingUserHead)
            {
                Debug.LogWarning("PeripheralStateDetector.userHead is empty. Assign Main Camera or XR Origin/Main Camera.", detector);
                warnedMissingUserHead = true;
            }

            return;
        }

        timer += Time.deltaTime;
        if (timer < logInterval) return;

        timer = 0f;

        IReadOnlyList<PeripheralDetectionResult> results = detector.LatestResults;
        for (int i = 0; i < results.Count; i++)
        {
            WriteResult(results[i]);
        }

        if (flushEachWrite)
            writer.Flush();
    }

    private string BuildFileName()
    {
        if (!includeExperimentMetadataInFileName && !appendTimestampToFileName)
            return fileName;

        string directory = Path.GetDirectoryName(fileName);
        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        string suffix = BuildFileNameSuffix();
        string stampedFileName = string.IsNullOrEmpty(suffix) ? name + extension : name + "_" + suffix + extension;

        if (string.IsNullOrEmpty(directory))
            return stampedFileName;

        return Path.Combine(directory, stampedFileName);
    }

    private string BuildFileNameSuffix()
    {
        List<string> parts = new List<string>();

        if (includeExperimentMetadataInFileName)
        {
            AddFileNamePart(parts, participantId);
            AddFileNamePart(parts, conditionLabel);
            AddFileNamePart(parts, trialId);
        }

        if (appendTimestampToFileName)
            parts.Add(System.DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));

        return string.Join("_", parts);
    }

    private static void AddFileNamePart(List<string> parts, string value)
    {
        string sanitized = SanitizeFileNamePart(value);
        if (!string.IsNullOrEmpty(sanitized))
            parts.Add(sanitized);
    }

    private static string SanitizeFileNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        char[] invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            bool invalid = false;
            for (int j = 0; j < invalidChars.Length; j++)
            {
                if (c == invalidChars[j])
                {
                    invalid = true;
                    break;
                }
            }

            builder.Append(invalid || char.IsWhiteSpace(c) ? '_' : c);
        }

        return builder.ToString().Trim('_');
    }

    private void WriteResult(PeripheralDetectionResult result)
    {
        if (!logNoneState && result.state == PeripheralState.None)
            return;

        string line = string.Join(",",
            Escape(participantId),
            Escape(conditionLabel),
            Escape(trialId),
            Time.time.ToString("F3", CultureInfo.InvariantCulture),
            GetTrialElapsed().ToString("F3", CultureInfo.InvariantCulture),
            GetTrialDuration().ToString("F3", CultureInfo.InvariantCulture),
            Escape(result.targetId),
            Escape(result.state.ToString()),
            HasState(result.state, PeripheralState.OutOfView),
            HasState(result.state, PeripheralState.Approaching),
            HasState(result.state, PeripheralState.Speaking),
            HasState(result.state, PeripheralState.Gazing),
            HasState(result.state, PeripheralState.Near),
            HasState(result.state, PeripheralState.Crossing),
            result.distance.ToString("F3", CultureInfo.InvariantCulture),
            result.viewAngle.ToString("F2", CultureInfo.InvariantCulture),
            result.radialSpeed.ToString("F3", CultureInfo.InvariantCulture),
            result.lateralSpeed.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.x.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.y.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.z.ToString("F3", CultureInfo.InvariantCulture)
        );

        writer.WriteLine(line);
    }

    private float GetTrialElapsed()
    {
        return trialController != null ? trialController.ElapsedSeconds : Time.time;
    }

    private float GetTrialDuration()
    {
        return trialController != null ? trialController.trialDurationSeconds : 0f;
    }

    private static bool HasState(PeripheralState value, PeripheralState state)
    {
        return (value & state) != 0;
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    private void OnDestroy()
    {
        CloseWriter();
    }

    private void OnApplicationQuit()
    {
        CloseWriter();
    }

    private void CloseWriter()
    {
        if (writer == null) return;

        writer.Flush();
        writer.Close();
        writer = null;
    }
}
