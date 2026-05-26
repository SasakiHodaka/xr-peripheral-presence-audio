using System.Collections.Generic;
using UnityEngine;

public class PeripheralDebugUI : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralTrialController trialController;

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

        GUI.Box(new Rect(10, 10, 760, 260), "Peripheral Debug", boxStyle);

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
        DrawLine(1, "Targets: " + detector.targets.Count + " / Results: " + results.Count, Color.white);
        DrawTrialLine(2);

        int visibleRows = Mathf.Min(results.Count, 5);
        for (int i = 0; i < visibleRows; i++)
        {
            PeripheralDetectionResult result = results[i];
            string line =
                result.targetId +
                " | " + result.state +
                " | dist " + result.distance.ToString("F2") +
                " | angle " + result.viewAngle.ToString("F1");

            DrawLine(i + 3, line, GetStateColor(result.state));
        }
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
