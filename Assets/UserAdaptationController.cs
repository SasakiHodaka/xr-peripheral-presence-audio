using System.Globalization;
using System.IO;
using UnityEngine;

public sealed class UserAdaptationController : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float helpIncrease = 0.25f;
    [SerializeField, Range(0f, 1f)] private float errorIncrease = 0.20f;
    [SerializeField, Range(0f, 1f)] private float successDecrease = 0.15f;

    private PrioritySelectionPolicy policy;
    private string logPath;
    private string lastEvent = "No adaptation event yet";
    private int helpCount;
    private int errorCount;
    private int successCount;

    public float CurrentNeed
    {
        get
        {
            EnsurePolicy();
            return policy != null ? policy.GuidanceNeed : 0f;
        }
    }
    public int HelpCount => helpCount;
    public int ErrorCount => errorCount;
    public int SuccessCount => successCount;

    private void Awake()
    {
        policy = GetComponent<PrioritySelectionPolicy>();
    }

    private void Start()
    {
        string directory = Path.Combine(Application.persistentDataPath, "scene_token_ground_truth");
        Directory.CreateDirectory(directory);
        logPath = Path.Combine(directory,
            "adaptation_events_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
        File.WriteAllText(logPath,
            "timestamp,eventType,reason,needBefore,needAfter,helpCount,errorCount,successCount\n");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) RecordHelpRequest("manual_help_key");
        if (Input.GetKeyDown(KeyCode.X)) RecordError("manual_error_key");
    }

    public void RecordHelpRequest(string reason)
    {
        helpCount++;
        Adjust("help", reason, helpIncrease);
    }

    public void RecordError(string reason)
    {
        errorCount++;
        Adjust("error", reason, errorIncrease);
    }

    public void RecordSuccess(string reason)
    {
        successCount++;
        Adjust("success", reason, -successDecrease);
    }

    private void Adjust(string eventType, string reason, float delta)
    {
        EnsurePolicy();
        if (policy == null) return;
        float before = policy.GuidanceNeed;
        float after = Mathf.Clamp01(before + delta);
        policy.UpdateGuidanceNeed(after);
        lastEvent = string.Format("{0}: {1:F2} -> {2:F2} ({3})", eventType, before, after, reason);

        if (!string.IsNullOrEmpty(logPath))
        {
            File.AppendAllText(logPath, string.Format(CultureInfo.InvariantCulture,
                "{0:F3},{1},{2},{3:F3},{4:F3},{5},{6},{7}\n",
                Time.time, Escape(eventType), Escape(reason), before, after,
                helpCount, errorCount, successCount));
        }
    }

    private void EnsurePolicy()
    {
        if (policy == null) policy = GetComponent<PrioritySelectionPolicy>();
    }

    private void OnGUI()
    {
        Rect area = new Rect(20f, Screen.height - 150f, 390f, 125f);
        GUI.Box(area, GUIContent.none);
        GUILayout.BeginArea(new Rect(area.x + 12f, area.y + 8f, area.width - 24f, area.height - 16f));
        GUILayout.Label(string.Format("CURRENT GUIDANCE NEED: {0:F2}", CurrentNeed));
        GUILayout.Label("H: request help   X: simulate error");
        GUILayout.Label(string.Format("help={0}  errors={1}  successes={2}",
            helpCount, errorCount, successCount));
        GUILayout.Label(lastEvent);
        GUILayout.EndArea();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(",") || value.Contains("\"")
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }
}
