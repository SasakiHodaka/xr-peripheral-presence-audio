using System.Collections.Generic;
using UnityEngine;

public class PeripheralDebugUI : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralTrialController trialController;
    public PeripheralTrialConditionController conditionController;
    public PeripheralCueModel cueModel;
    public PeripheralCueAudioEmitter audioEmitter;
    public PeripheralAuiLogCollectionController auiLogController;

    [Header("UI")]
    public bool showDebugUI = true;
    public int fontSize = 22;

    private GUIStyle labelStyle;
    private GUIStyle boxStyle;

    private void Awake()
    {
        if (detector == null)
            detector = GetComponent<PeripheralStateDetector>();

        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();

        if (conditionController == null)
            conditionController = GetComponent<PeripheralTrialConditionController>();

        if (cueModel == null)
            cueModel = GetComponent<PeripheralCueModel>();

        if (audioEmitter == null)
            audioEmitter = GetComponent<PeripheralCueAudioEmitter>();

        if (auiLogController == null)
            auiLogController = GetComponent<PeripheralAuiLogCollectionController>();
    }

    private void InitStyles()
    {
        if (labelStyle != null && boxStyle != null) return;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = fontSize;
        labelStyle.normal.textColor = Color.white;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = fontSize;
        boxStyle.normal.textColor = Color.white;
        boxStyle.alignment = TextAnchor.UpperLeft;
    }

    private void OnGUI()
    {
        if (!showDebugUI) return;

        InitStyles();

        GUI.Box(new Rect(10, 10, 760, 430), "Peripheral Debug", boxStyle);

        if (detector == null)
        {
            DrawLine(0, "Detector: missing", Color.red);
            return;
        }

        if (detector.userHead == null)
        {
            DrawLine(0, "User Head: missing", Color.red);
            return;
        }

        IReadOnlyList<PeripheralDetectionResult> results = detector.LatestResults;
        DrawLine(0, "User Head: " + detector.userHead.name, Color.white);
        DrawLine(1, "Condition: " + GetConditionLabel(), Color.white);
        DrawLine(2, "Cue: " + GetCueConditionLabel(), Color.white);
        DrawLine(3, "AUI Trial: " + GetAuiTrialLabel(), Color.white);
        DrawLine(4, "Targets: " + detector.targets.Count + " / Results: " + results.Count, Color.white);
        DrawTrialLine(5);

        int visibleRows = Mathf.Min(results.Count, 5);
        for (int i = 0; i < visibleRows; i++)
        {
            PeripheralDetectionResult result = results[i];
            PeripheralCuePrediction cue = cueModel != null ? cueModel.Predict(result) : new PeripheralCuePrediction();
            PeripheralCuePlaybackState playback = audioEmitter != null ? audioEmitter.GetPlaybackState(result.targetId) : new PeripheralCuePlaybackState();
            string line =
                result.targetId +
                " | " + result.state +
                " | dist " + result.distance.ToString("F2") +
                " | angle " + result.viewAngle.ToString("F1") +
                " | cue " + cue.cueType +
                " " + cue.presenceScore.ToString("F2") +
                " | audio " + playback.outputVolume.ToString("F2") +
                " lp " + playback.lowPassHz.ToString("F0") +
                " rv " + playback.reverbAmount.ToString("F2");

            DrawLine(i + 6, line, GetStateColor(result.state));
        }
    }

    private string GetConditionLabel()
    {
        return conditionController != null ? conditionController.condition.ToString() : "(none)";
    }

    private string GetCueConditionLabel()
    {
        return cueModel != null ? cueModel.comparisonCondition.ToString() : "(none)";
    }

    private string GetAuiTrialLabel()
    {
        if (auiLogController == null)
            return "(manual)";

        return (auiLogController.CurrentTrialIndex + 1) + " / " + auiLogController.TotalTrialCount;
    }

    private void DrawTrialLine(int row)
    {
        if (trialController == null)
        {
            DrawLine(row, "Trial: not configured", Color.white);
            return;
        }

        string line;
        if (trialController.IsComplete)
            line = "Trial complete: " + trialController.trialDurationSeconds.ToString("F1") + "s";
        else if (trialController.IsRunning)
            line = "Trial: " + trialController.ElapsedSeconds.ToString("F1") + " / " + trialController.trialDurationSeconds.ToString("F1") + "s";
        else
            line = "Pre-trial: " + trialController.PreTrialRemainingSeconds.ToString("F1") + "s";

        Color color = trialController.IsComplete ? Color.green : Color.white;
        DrawLine(row, line, color);
    }

    private void DrawLine(int row, string text, Color color)
    {
        GUI.color = color;
        GUI.Label(new Rect(30, 58 + row * 36, 720, 34), text, labelStyle);
        GUI.color = Color.white;
    }

    private static Color GetStateColor(PeripheralState state)
    {
        if ((state & PeripheralState.Near) != 0)
            return Color.red;

        if ((state & PeripheralState.OutOfView) != 0)
            return Color.yellow;

        if (state != PeripheralState.None)
            return Color.cyan;

        return Color.white;
    }
}
