using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class PeripheralStateLogger : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;

    [Header("Experiment")]
    public string participantId = "P001";
    public string conditionLabel = "demo";
    public string trialId = "T001";

    [Header("CSV")]
    public string fileName = "peripheral_state_log.csv";
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
    }

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, BuildFileName());
        writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("participantId,conditionLabel,trialId,time,targetId,state,outOfView,approaching,speaking,gazing,near,crossing,distance,viewAngle,radialSpeed,lateralSpeed,localX,localY,localZ");
        writer.Flush();

        Debug.Log("Peripheral CSV created: " + filePath);
    }

    private void Update()
    {
        if (writer == null) return;

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
        if (!appendTimestampToFileName)
            return fileName;

        string directory = Path.GetDirectoryName(fileName);
        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string stampedFileName = name + "_" + timestamp + extension;

        if (string.IsNullOrEmpty(directory))
            return stampedFileName;

        return Path.Combine(directory, stampedFileName);
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
