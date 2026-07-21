using System;
using UnityEngine;

[Serializable]
public sealed class PresentationResult
{
    public string scenarioId;
    public string eventId;
    public string mode;
    public string description;
    public string message;
    public float cueScale;
    public float cueDuration;
    public float audioGain;
}

public sealed class PresentationPolicy : MonoBehaviour
{
    public PresentationResult Present(
        GeneratedSceneToken token,
        SelectionResult selection,
        SemanticPacket packet
    )
    {
        if (token == null)
        {
            return null;
        }

        string mode;
        string description;
        string message = "";
        float cueScale = 0.25f;
        float cueDuration = 0.5f;
        float audioGain = 0f;

        if (selection == null || !selection.selected)
        {
            mode = "Muted";
            description = "No presentation";
        }
        else if (selection.comparisonMode == ComparisonSelectionMode.ContextAndUserState)
        {
            bool critical = token.priority >= 2;
            float need = Mathf.Clamp01(selection.guidanceNeed);
            cueScale = Mathf.Lerp(0.35f, critical ? 0.85f : 0.65f, need);
            cueDuration = Mathf.Lerp(0.8f, critical ? 2.5f : 2.0f, need);
            audioGain = critical ? Mathf.Lerp(0.45f, 0.65f, need) : Mathf.Lerp(0.15f, 0.35f, need);

            if (need >= 0.5f)
            {
                mode = critical ? "AdaptiveDetailedCritical" : "AdaptiveDetailedGuidance";
                message = string.Format("{0}: {1} at {2}. Follow the highlighted target.",
                    Humanize(token.utteranceId), Humanize(token.targetObjectId), Humanize(token.direction));
                description = string.Format("Detailed guidance for current need {0:F2}", need);
            }
            else
            {
                mode = critical ? "AdaptiveCompactAlert" : "AdaptiveCompactStatus";
                message = string.Format("{0}: {1}",
                    Humanize(token.utteranceId), Humanize(token.targetObjectId));
                description = string.Format("Compact cue for current need {0:F2}", need);
            }
        }
        else
        {
            switch (selection.level)
            {
                case CommunicationLevel.AudioAndToken:
                    mode = "SpatialAudioAndVisualCue";
                    description = "Present spatial audio with semantic cue";
                    message = Humanize(token.utteranceId);
                    cueScale = 0.45f;
                    cueDuration = 1.5f;
                    audioGain = 0.35f;
                    break;
                case CommunicationLevel.AudioOnly:
                    mode = "SpatialAudio";
                    description = "Present spatial audio only";
                    message = Humanize(token.utteranceId);
                    audioGain = 0.35f;
                    break;
                case CommunicationLevel.TokenOnly:
                    mode = "VisualCue";
                    description = "Present semantic visual cue only";
                    message = Humanize(token.utteranceId);
                    cueScale = 0.45f;
                    cueDuration = 1.5f;
                    break;
                case CommunicationLevel.None:
                default:
                    mode = "Muted";
                    description = "No presentation";
                    break;
            }
        }

        Debug.Log(
            "Presentation Result," +
            $"{token.scenarioId}," +
            $"{token.eventId}," +
            $"{mode}," +
            $"{description}"
        );

        return new PresentationResult
        {
            scenarioId = token.scenarioId,
            eventId = token.eventId,
            mode = mode,
            description = description,
            message = message,
            cueScale = cueScale,
            cueDuration = cueDuration,
            audioGain = audioGain
        };
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "unspecified";
        return value.Replace("_", " ");
    }
}
