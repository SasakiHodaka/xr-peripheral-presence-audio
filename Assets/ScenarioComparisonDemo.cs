using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public sealed class ScenarioComparisonDemo : MonoBehaviour
{
    private sealed class EventView { public string text; public bool selected; }
    private readonly List<EventView> events = new List<EventView>();
    private readonly List<string> completedRuns = new List<string>();
    private PrioritySelectionPolicy policy;
    private ScenarioPlayer player;
    private InteractiveObjectScenario interactiveScenario;
    private UserAdaptationController adaptation;
    private int selectedCount;
    private int totalCount;
    private int totalBytes;
    private GUIStyle titleStyle;
    private GUIStyle lineStyle;
    private AudioClip alertClip;
    private string runLogPath;
    private float runStartTime;
    private float runInitialNeed;
    private int runInitialHelpCount;
    private int runInitialErrorCount;
    private int runInitialSuccessCount;
    private bool currentRunCaptured;

    public string RunLogPath => runLogPath;

    private void Awake()
    {
        policy = GetComponent<PrioritySelectionPolicy>();
        player = GetComponent<ScenarioPlayer>();
        interactiveScenario = GetComponent<InteractiveObjectScenario>();
        alertClip = CreateAlertClip();
    }

    private void Start()
    {
        EnsureAdaptation();
        string directory = Path.Combine(Application.persistentDataPath, "scene_token_ground_truth");
        Directory.CreateDirectory(directory);
        runLogPath = Path.Combine(directory,
            "pilot_runs_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
        File.WriteAllText(runLogPath,
            "runIndex,completionReason,mode,initialNeed,finalNeed,duration,totalEvents,selectedEvents,suppressedEvents,packetBytes,helpEvents,errorEvents,successEvents\n");
        BeginRun();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6)) SetMode(ComparisonSelectionMode.FullTransmission);
        if (Input.GetKeyDown(KeyCode.F7)) SetMode(ComparisonSelectionMode.PriorityOnly);
        if (Input.GetKeyDown(KeyCode.F8)) SetMode(ComparisonSelectionMode.ContextAndUserState);
        if (Input.GetKeyDown(KeyCode.F4)) SetGuidanceNeed(0.8f);
        if (Input.GetKeyDown(KeyCode.F5)) SetGuidanceNeed(0.2f);
        if (Input.GetKeyDown(KeyCode.LeftBracket)) SetGuidanceNeed(policy != null ? policy.GuidanceNeed - 0.1f : 0f);
        if (Input.GetKeyDown(KeyCode.RightBracket)) SetGuidanceNeed(policy != null ? policy.GuidanceNeed + 0.1f : 0f);
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.F9)) Restart();
    }

    public void Record(GeneratedSceneToken token, SelectionResult selection,
        SemanticPacket packet, PresentationResult presentation)
    {
        totalCount++;
        bool selected = selection != null && selection.selected;
        if (selected) selectedCount++;
        int bytes = packet != null ? packet.GetPacketBytes() : 0;
        totalBytes += bytes;

        events.Add(new EventView
        {
            selected = selected,
            text = string.Format("{0}  {1}  {2}  {3} bytes  score={4:F2} [U{5:F2} R{6:F2} N{7:F2} P{8:F2}]\n    {9}\n    PRESENT: {10}",
                token.eventId, selected ? "SENT" : "SUPPRESSED",
                selection != null ? selection.level.ToString() : "None", bytes,
                selection != null ? selection.totalScore : 0f,
                selection != null ? selection.urgencyScore : 0f,
                selection != null ? selection.relevanceScore : 0f,
                selection != null ? selection.noveltyScore : 0f,
                selection != null ? selection.needFitScore : 0f,
                selection != null ? selection.reason : "No decision",
                presentation != null ? presentation.message : "None")
        });
        if (events.Count > 6) events.RemoveAt(0);
        StartCoroutine(ShowWorldCue(token, selected, presentation));
    }

    private void SetMode(ComparisonSelectionMode mode)
    {
        if (policy == null) return;
        CaptureCurrentSummary("condition_changed");
        policy.SetComparisonMode(mode);
        Restart(false);
    }

    private void SetGuidanceNeed(float value)
    {
        if (policy == null) return;
        CaptureCurrentSummary("need_preset_changed");
        policy.SetGuidanceNeed(value);
        Restart(false);
    }

    private void Restart(bool captureSummary = true)
    {
        if (captureSummary) CaptureCurrentSummary("restarted");
        events.Clear();
        selectedCount = 0;
        totalCount = 0;
        totalBytes = 0;
        BeginRun();
        if (interactiveScenario != null)
            interactiveScenario.RestartAndPlay();
        else if (player != null)
            player.RestartPlayback();
    }

    public void CompleteCurrentRun()
    {
        CaptureCurrentSummary("completed");
    }

    private void BeginRun()
    {
        EnsureAdaptation();
        runStartTime = Time.time;
        runInitialNeed = policy != null ? policy.GuidanceNeed : 0f;
        runInitialHelpCount = adaptation != null ? adaptation.HelpCount : 0;
        runInitialErrorCount = adaptation != null ? adaptation.ErrorCount : 0;
        runInitialSuccessCount = adaptation != null ? adaptation.SuccessCount : 0;
        currentRunCaptured = false;
    }

    private void CaptureCurrentSummary(string completionReason)
    {
        if (policy == null || totalCount == 0 || currentRunCaptured) return;
        EnsureAdaptation();
        float duration = Mathf.Max(0f, Time.time - runStartTime);
        float finalNeed = policy.GuidanceNeed;
        int helpEvents = adaptation != null ? adaptation.HelpCount - runInitialHelpCount : 0;
        int errorEvents = adaptation != null ? adaptation.ErrorCount - runInitialErrorCount : 0;
        int successEvents = adaptation != null ? adaptation.SuccessCount - runInitialSuccessCount : 0;
        completedRuns.Add(string.Format("{0} / need {1:F1}: {2}/{3} sent, {4} bytes ({5:F0}% events)",
            policy.ComparisonMode, finalNeed,
            selectedCount, totalCount, totalBytes, 100f * selectedCount / totalCount));
        if (completedRuns.Count > 4) completedRuns.RemoveAt(0);
        currentRunCaptured = true;

        if (!string.IsNullOrEmpty(runLogPath))
        {
            int runIndex = File.ReadAllLines(runLogPath).Length;
            File.AppendAllText(runLogPath, string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3:F3},{4:F3},{5:F3},{6},{7},{8},{9},{10},{11},{12}\n",
                runIndex, completionReason, policy.ComparisonMode,
                runInitialNeed, finalNeed, duration, totalCount, selectedCount,
                totalCount - selectedCount, totalBytes, helpEvents, errorEvents, successEvents));
        }
    }

    private void EnsureAdaptation()
    {
        if (adaptation == null) adaptation = GetComponent<UserAdaptationController>();
    }

    private IEnumerator ShowWorldCue(GeneratedSceneToken token, bool selected,
        PresentationResult presentation)
    {
        Vector3 position = new Vector3(token.sourceX, Mathf.Max(1f, token.sourceY + 1f), token.sourceZ);
        GameObject cue = GameObject.CreatePrimitive(selected ? PrimitiveType.Sphere : PrimitiveType.Cube);
        cue.name = "ComparisonCue_" + token.eventId;
        cue.transform.position = position;
        float cueScale = presentation != null ? presentation.cueScale : (selected ? 0.45f : 0.25f);
        cue.transform.localScale = Vector3.one * cueScale;
        Renderer renderer = cue.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = selected ? new Color(0.1f, 1f, 0.25f) : new Color(0.45f, 0.45f, 0.45f);

        if (selected && presentation != null && presentation.audioGain > 0f && alertClip != null)
            AudioSource.PlayClipAtPoint(alertClip, position, presentation.audioGain);

        yield return new WaitForSeconds(presentation != null ? presentation.cueDuration : 0.5f);
        if (cue != null) Destroy(cue);
    }

    private void OnGUI()
    {
        EnsureStyles();
        float panelWidth = Mathf.Min(650f, Screen.width - 40f);
        Rect panel = new Rect(Screen.width - panelWidth - 20f, 20f,
            panelWidth, 155f + completedRuns.Count * 24f + events.Count * 67f);
        GUI.Box(panel, GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x + 14f, panel.y + 10f, panel.width - 28f, panel.height - 20f));
        GUILayout.Label("SEMANTIC SELECTION COMPARISON", titleStyle);
        GUILayout.Label("F4 Need 0.8   F5 Need 0.2   [ / ] Adjust need   F6 Full   F7 Priority   F8 Proposed", lineStyle);
        GUILayout.Label(string.Format("Mode: {0} / Current guidance need: {1:F1}    Events: {2}/{3} sent    Packets: {4} bytes",
            policy != null ? policy.ComparisonMode.ToString() : "Unavailable",
            policy != null ? policy.GuidanceNeed : 0f,
            selectedCount, totalCount, totalBytes), titleStyle);
        if (completedRuns.Count > 0)
        {
            GUILayout.Label("COMPLETED RUNS (same scenario)", lineStyle);
            foreach (string summary in completedRuns) GUILayout.Label(summary, lineStyle);
        }
        foreach (EventView item in events)
        {
            Color old = GUI.color;
            GUI.color = item.selected ? new Color(0.65f, 1f, 0.7f) : new Color(0.75f, 0.75f, 0.75f);
            GUILayout.Label(item.text, lineStyle);
            GUI.color = old;
        }
        GUILayout.EndArea();
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = Color.white;
        lineStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
        lineStyle.normal.textColor = Color.white;
    }

    private static AudioClip CreateAlertClip()
    {
        const int sampleRate = 44100;
        int length = Mathf.CeilToInt(sampleRate * 0.18f);
        float[] samples = new float[length];
        for (int i = 0; i < length; i++)
            samples[i] = Mathf.Sin(2f * Mathf.PI * 880f * i / sampleRate) * (1f - (float)i / length) * 0.25f;
        AudioClip clip = AudioClip.Create("SemanticAlert", length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
