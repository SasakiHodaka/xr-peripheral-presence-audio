using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresenceDebugUI : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;
    public PresenceTarget target;

    [Header("Score Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 10f;
    public float outOfViewBoost = 0.5f;

    [Header("UI Settings")]
    public int fontSize = 36;

    private GUIStyle labelStyle;
    private GUIStyle boxStyle;

    private string currentAudioCue = "";
    private string currentReason = "";

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
        InitStyles();

        if (playerHead == null || target == null) return;
        if (target.bodyTransform == null) return;

        Vector3 toTarget = target.bodyTransform.position - playerHead.position;
        float distance = toTarget.magnitude;

        float distanceScore =
            1f - Mathf.InverseLerp(minDistance, maxDistance, distance);

        float viewDot =
            Vector3.Dot(
                playerHead.forward.normalized,
                toTarget.normalized
            );

        bool isOutOfView = viewDot < 0.2f;

        float presenceScore = distanceScore;

        if (isOutOfView)
        {
            presenceScore += outOfViewBoost;
        }

        if (target.isSpeaking)
        {
            presenceScore += 0.2f;
        }

        presenceScore = Mathf.Clamp01(presenceScore);

        // =========================
        // Audio Cue ”»’è
        // =========================
        currentAudioCue = "";
        currentReason = "";

        if (distance < 4f)
        {
            currentAudioCue += "Footstep ";
        }

        if (isOutOfView)
        {
            currentAudioCue += "+ Breathing ";
        }

        if (target.isSpeaking)
        {
            currentAudioCue += "+ Voice ";
        }

        if (string.IsNullOrWhiteSpace(currentAudioCue))
        {
            currentAudioCue = "None";
        }

        if (isOutOfView && distance < 4f)
        {
            currentReason = "Out-of-view approach detected";
        }
        else if (isOutOfView)
        {
            currentReason = "Out-of-view presence detected";
        }
        else if (distance < 4f)
        {
            currentReason = "Nearby target detected";
        }
        else
        {
            currentReason = "Normal presence";
        }

        // =========================
        // UI•`‰æ
        // =========================
        GUI.Box(
            new Rect(10, 10, 760, 520),
            "Presence Debug",
            boxStyle
        );

        GUI.color = Color.yellow;
        GUI.Label(
            new Rect(40, 80, 700, 50),
            "Presence Score : " + presenceScore.ToString("F2"),
            labelStyle
        );

        GUI.color = Color.white;
        GUI.Label(
            new Rect(40, 150, 700, 50),
            "Distance : " + distance.ToString("F2") + " m",
            labelStyle
        );

        GUI.color = isOutOfView ? Color.red : Color.white;
        GUI.Label(
            new Rect(40, 220, 700, 50),
            "Out Of View : " + isOutOfView,
            labelStyle
        );

        GUI.color = target.isSpeaking ? Color.cyan : Color.white;
        GUI.Label(
            new Rect(40, 290, 700, 50),
            "Speaking : " + target.isSpeaking,
            labelStyle
        );

        GUI.color = Color.green;
        GUI.Label(
            new Rect(40, 360, 700, 50),
            "Audio Cue : " + currentAudioCue,
            labelStyle
        );

        GUI.color = Color.cyan;
        GUI.Label(
            new Rect(40, 430, 700, 50),
            "Reason : " + currentReason,
            labelStyle
        );

        GUI.color = Color.white;
    }
}