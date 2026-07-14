using System;
using UnityEngine;

[Serializable]
public sealed class PresentationResult
{
    public string scenarioId;
    public string eventId;
    public string mode;
    public string description;
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

        if (selection == null || !selection.selected)
        {
            mode = "Muted";
            description = "No presentation";
        }
        else
        {
            switch (selection.level)
            {
                case CommunicationLevel.AudioAndToken:
                    mode = "SpatialAudioAndVisualCue";
                    description = "Present spatial audio with semantic cue";
                    break;
                case CommunicationLevel.AudioOnly:
                    mode = "SpatialAudio";
                    description = "Present spatial audio only";
                    break;
                case CommunicationLevel.TokenOnly:
                    mode = "VisualCue";
                    description = "Present semantic visual cue only";
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
            description = description
        };
    }
}
