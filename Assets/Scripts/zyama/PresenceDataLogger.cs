using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresenceDataLogger : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;
    public PresenceTarget target;

    [Header("Score Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 6f;
    public float outOfViewBoost = 0.15f;

    [Header("Log Settings")]
    public float logInterval = 0.5f;

    private float timer = 0f;
    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "presence_training_data.csv");

        string header =
            "time,distance,outOfView,speaking,presenceScore,audioCue,reason";

        File.WriteAllText(filePath, header + "\n");

        Debug.Log("CSV created: " + filePath);
    }

    void Update()
    {
        if (playerHead == null)
        {
            Debug.LogWarning("PresenceDataLogger: Player Head is not set");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("PresenceDataLogger: Target is not set");
            return;
        }

        if (target.bodyTransform == null)
        {
            Debug.LogWarning("PresenceDataLogger: Target Body Transform is not set");
            return;
        }

        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            timer = 0f;
            LogData();
        }
    }

    void LogData()
    {
        Vector3 toTarget =
            target.bodyTransform.position - playerHead.position;

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

        string audioCue =
            GetAudioCue(distance, isOutOfView, target.isSpeaking);

        string reason =
            GetReason(distance, isOutOfView);

        string line =
            Time.time.ToString("F2") + "," +
            distance.ToString("F2") + "," +
            isOutOfView + "," +
            target.isSpeaking + "," +
            presenceScore.ToString("F2") + "," +
            audioCue + "," +
            reason;

        File.AppendAllText(filePath, line + "\n");

        Debug.Log("Logged: " + line);
    }

    string GetAudioCue(float distance, bool isOutOfView, bool isSpeaking)
    {
        string cue = "";

        if (distance < 4f)
        {
            cue += "Footstep";
        }

        if (isOutOfView)
        {
            if (cue != "") cue += "+";
            cue += "Breathing";
        }

        if (isSpeaking)
        {
            if (cue != "") cue += "+";
            cue += "Voice";
        }

        if (cue == "")
        {
            cue = "None";
        }

        return cue;
    }

    string GetReason(float distance, bool isOutOfView)
    {
        if (isOutOfView && distance < 4f)
        {
            return "OutOfViewApproach";
        }
        else if (isOutOfView)
        {
            return "OutOfViewPresence";
        }
        else if (distance < 4f)
        {
            return "NearbyTarget";
        }
        else
        {
            return "NormalPresence";
        }
    }
}