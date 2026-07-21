using System;
using UnityEngine;

public enum CommunicationLevel
{
    None = 0,
    TokenOnly = 1,
    AudioOnly = 2,
    AudioAndToken = 3
}

public enum ComparisonSelectionMode
{
    FullTransmission = 0,
    PriorityOnly = 1,
    ContextAndUserState = 2
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
    public ComparisonSelectionMode comparisonMode;
    public float guidanceNeed;
    public float urgencyScore;
    public float relevanceScore;
    public float noveltyScore;
    public float needFitScore;
    public float totalScore;
    public float decisionThreshold;
}

public sealed class PrioritySelectionPolicy : MonoBehaviour
{
    [SerializeField]
    private ComparisonSelectionMode comparisonMode = ComparisonSelectionMode.PriorityOnly;

    [SerializeField]
    [Range(0f, 1f)]
    private float guidanceNeed = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float lowNeedThreshold = 0.72f;

    [SerializeField, Range(0f, 1f)]
    private float highNeedThreshold = 0.62f;

    private readonly System.Collections.Generic.HashSet<string> presentedTargets =
        new System.Collections.Generic.HashSet<string>();

    public ComparisonSelectionMode ComparisonMode => comparisonMode;
    public float GuidanceNeed => guidanceNeed;

    public void SetComparisonMode(ComparisonSelectionMode mode)
    {
        comparisonMode = mode;
        ResetUserState();
    }

    public void SetGuidanceNeed(float value)
    {
        UpdateGuidanceNeed(value, true);
    }

    public void UpdateGuidanceNeed(float value, bool resetKnownTargets = false)
    {
        guidanceNeed = Mathf.Clamp01(value);
        if (resetKnownTargets) ResetUserState();
    }

    public void ResetUserState()
    {
        presentedTargets.Clear();
    }

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

        if (comparisonMode == ComparisonSelectionMode.FullTransmission)
        {
            return CreateResult(token, CommunicationLevel.AudioAndToken,
                "Full transmission: all events are sent");
        }

        if (comparisonMode == ComparisonSelectionMode.ContextAndUserState)
        {
            bool critical = token.priority >= highPriorityThreshold;
            bool hasTarget = !string.IsNullOrWhiteSpace(token.targetObjectId) && token.targetObjectId != "None";
            bool taskRelevant = IsTaskRelevant(token);
            bool alreadyKnown = hasTarget && presentedTargets.Contains(token.targetObjectId);
            float urgencyScore = Mathf.Clamp01(token.priority / (float)highPriorityThreshold) * 0.40f;
            float relevanceScore = taskRelevant ? 0.25f : 0f;
            float noveltyScore = hasTarget && !alreadyKnown ? 0.20f : 0f;
            float needFitScore = CalculateNeedFit(token, guidanceNeed) * 0.15f;
            float totalScore = urgencyScore + relevanceScore + noveltyScore + needFitScore;
            float threshold = Mathf.Lerp(lowNeedThreshold, highNeedThreshold, guidanceNeed);

            if (critical)
            {
                level = CommunicationLevel.AudioAndToken;
                reason = "Critical override: always send";
            }
            else if (totalScore >= threshold)
            {
                level = CommunicationLevel.TokenOnly;
                reason = string.Format("Score {0:F2} >= {1:F2}: send at need {2:F2}",
                    totalScore, threshold, guidanceNeed);
            }
            else
            {
                level = CommunicationLevel.None;
                reason = string.Format("Score {0:F2} < {1:F2}: suppress at need {2:F2}",
                    totalScore, threshold, guidanceNeed);
            }

            if (level != CommunicationLevel.None && hasTarget)
            {
                presentedTargets.Add(token.targetObjectId);
            }

            SelectionResult scoredResult = CreateResult(token, level, reason);
            scoredResult.guidanceNeed = guidanceNeed;
            scoredResult.urgencyScore = urgencyScore;
            scoredResult.relevanceScore = relevanceScore;
            scoredResult.noveltyScore = noveltyScore;
            scoredResult.needFitScore = needFitScore;
            scoredResult.totalScore = totalScore;
            scoredResult.decisionThreshold = threshold;
            return scoredResult;
        }

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

        return CreateResult(token, level, reason);
    }

    private SelectionResult CreateResult(GeneratedSceneToken token, CommunicationLevel level, string reason)
    {
        return new SelectionResult
        {
            scenarioId = token.scenarioId,
            eventId = token.eventId,
            priority = token.priority,
            level = level,
            selected = level != CommunicationLevel.None,
            reason = reason,
            comparisonMode = comparisonMode,
            guidanceNeed = guidanceNeed
        };
    }

    private static float CalculateNeedFit(GeneratedSceneToken token, float currentGuidanceNeed)
    {
        // Routine procedure is more useful when the receiver currently needs guidance.
        // Critical information is protected separately by the critical override.
        return token.priority >= 1 ? Mathf.Clamp01(currentGuidanceNeed) : 0f;
    }

    private static bool IsTaskRelevant(GeneratedSceneToken token)
    {
        if (string.IsNullOrWhiteSpace(token.targetObjectId) || token.targetObjectId == "None") return false;
        if (string.IsNullOrWhiteSpace(token.taskState)) return true;

        string normalizedTask = token.taskState.Replace("Work", string.Empty);
        return token.targetObjectId.IndexOf(normalizedTask, StringComparison.OrdinalIgnoreCase) >= 0 ||
               token.priority >= 2;
    }
}
