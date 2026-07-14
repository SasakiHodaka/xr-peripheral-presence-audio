using System;
using UnityEngine;

public enum CommunicationLevel
{
    None = 0,
    TokenOnly = 1,
    AudioOnly = 2,
    AudioAndToken = 3
}

[Serializable]
public sealed class SelectionResult
{
    public string scenarioId;
    public string eventId;
    public int priority;
    public CommunicationLevel level;
    public bool selected;
    public string reason;
}

public sealed class PrioritySelectionPolicy : MonoBehaviour
{
    [SerializeField]
    private int transmitThreshold = 1;

    [SerializeField]
    private int highPriorityThreshold = 2;

    public SelectionResult Select(GeneratedSceneToken token)
    {
        if (token == null)
        {
            return null;
        }

        CommunicationLevel level;
        string reason;

        if (token.priority >= highPriorityThreshold)
        {
            level = CommunicationLevel.AudioAndToken;
            reason = "High priority";
        }
        else if (token.priority >= transmitThreshold)
        {
            level = CommunicationLevel.TokenOnly;
            reason = "Priority threshold passed";
        }
        else
        {
            level = CommunicationLevel.None;
            reason = "Priority below threshold";
        }

        return new SelectionResult
        {
            scenarioId = token.scenarioId,
            eventId = token.eventId,
            priority = token.priority,
            level = level,
            selected = level != CommunicationLevel.None,
            reason = reason
        };
    }
}
