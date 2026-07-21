#if UNITY_EDITOR
using System.Collections.Generic;
using SceneTokens;
using UnityEditor;
using UnityEngine;

public static class SceneTokenAnalyzerSelfCheck
{
    [MenuItem("Tools/Semantic Spatial Audio/Run Scene Token Analyzer Self Check")]
    [MenuItem("Tools/Scene Tokens/Run Analyzer Self Check")]
    public static void RunFromMenu()
    {
        var errors = RunChecks();
        if (errors.Count == 0)
        {
            Debug.Log("[SceneTokenAnalyzerSelfCheck] Passed.");
            return;
        }

        Debug.LogError("[SceneTokenAnalyzerSelfCheck] Failed:\n" + string.Join("\n", errors));
    }

    public static void RunForBatch()
    {
        var errors = RunChecks();
        if (errors.Count == 0)
        {
            Debug.Log("[SceneTokenAnalyzerSelfCheck] Passed.");
            return;
        }

        Debug.LogError("[SceneTokenAnalyzerSelfCheck] Failed:\n" + string.Join("\n", errors));
        EditorApplication.Exit(1);
    }

    private static List<string> RunChecks()
    {
        var errors = new List<string>();
        CheckDirectionQuantization(errors);
        CheckDistanceQuantization(errors);
        CheckConversationQuantization(errors);
        CheckTransformAzimuth(errors);
        CheckCsvEscaping(errors);
        CheckMetricByteEstimates(errors);
        CheckPrioritySelectionPolicy(errors);
        CheckAdaptivePresentationPolicy(errors);
        CheckUserAdaptation(errors);
        return errors;
    }

    private static void CheckDirectionQuantization(List<string> errors)
    {
        Expect(errors, DirectionAnalyzer.QuantizeDirection(0f), SceneTokenDirection.FRONT, "0 deg should be FRONT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(45f), SceneTokenDirection.FRONT_RIGHT, "45 deg should be FRONT_RIGHT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(90f), SceneTokenDirection.RIGHT, "90 deg should be RIGHT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(135f), SceneTokenDirection.BACK_RIGHT, "135 deg should be BACK_RIGHT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(180f), SceneTokenDirection.BACK, "180 deg should be BACK.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(-45f), SceneTokenDirection.FRONT_LEFT, "-45 deg should be FRONT_LEFT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(-90f), SceneTokenDirection.LEFT, "-90 deg should be LEFT.");
        Expect(errors, DirectionAnalyzer.QuantizeDirection(-135f), SceneTokenDirection.BACK_LEFT, "-135 deg should be BACK_LEFT.");
    }

    private static void CheckDistanceQuantization(List<string> errors)
    {
        Expect(errors, DistanceAnalyzer.QuantizeDistance(0f), SceneTokenDistance.NEAR, "0m should be NEAR.");
        Expect(errors, DistanceAnalyzer.QuantizeDistance(1.49f), SceneTokenDistance.NEAR, "1.49m should be NEAR.");
        Expect(errors, DistanceAnalyzer.QuantizeDistance(1.5f), SceneTokenDistance.MID, "1.5m should be MID.");
        Expect(errors, DistanceAnalyzer.QuantizeDistance(2.99f), SceneTokenDistance.MID, "2.99m should be MID.");
        Expect(errors, DistanceAnalyzer.QuantizeDistance(3f), SceneTokenDistance.FAR, "3m should be FAR.");
    }

    private static void CheckConversationQuantization(List<string> errors)
    {
        Expect(errors, ConversationAnalyzer.QuantizeSpeakingState(false), SceneSpeakingState.SILENT, "false should be SILENT.");
        Expect(errors, ConversationAnalyzer.QuantizeSpeakingState(true), SceneSpeakingState.SPEAKING, "true should be SPEAKING.");
        Expect(errors, ConversationAnalyzer.QuantizeTurnState(false, 1), SceneTurnState.LISTENER, "silent speaker should be LISTENER.");
        Expect(errors, ConversationAnalyzer.QuantizeTurnState(true, 1), SceneTurnState.TURN_HOLDER, "single active speaker should be TURN_HOLDER.");
        Expect(errors, ConversationAnalyzer.QuantizeTurnState(true, 2), SceneTurnState.OVERLAPPER, "multiple active speakers should be OVERLAPPER.");
    }

    private static void CheckTransformAzimuth(List<string> errors)
    {
        var listenerObject = new GameObject("SelfCheckListener");
        var speakerObject = new GameObject("SelfCheckSpeaker");

        try
        {
            listenerObject.transform.position = Vector3.zero;
            listenerObject.transform.rotation = Quaternion.identity;

            speakerObject.transform.position = Vector3.forward;
            ExpectClose(errors, DirectionAnalyzer.CalculateSignedAzimuth(listenerObject.transform, speakerObject.transform), 0f, "forward speaker azimuth");

            speakerObject.transform.position = Vector3.right;
            ExpectClose(errors, DirectionAnalyzer.CalculateSignedAzimuth(listenerObject.transform, speakerObject.transform), 90f, "right speaker azimuth");

            speakerObject.transform.position = Vector3.left;
            ExpectClose(errors, DirectionAnalyzer.CalculateSignedAzimuth(listenerObject.transform, speakerObject.transform), -90f, "left speaker azimuth");

            speakerObject.transform.position = Vector3.forward * 3f;
            ExpectClose(errors, DistanceAnalyzer.CalculateHorizontalRange(listenerObject.transform, speakerObject.transform), 3f, "horizontal range");
        }
        finally
        {
            Object.DestroyImmediate(listenerObject);
            Object.DestroyImmediate(speakerObject);
        }
    }

    private static void CheckCsvEscaping(List<string> errors)
    {
        Expect(errors, SceneToken.EscapeCsv("plain"), "plain", "plain CSV value should not be quoted.");
        Expect(errors, SceneToken.EscapeCsv("speaker,A"), "\"speaker,A\"", "comma CSV value should be quoted.");
        Expect(errors, SceneToken.EscapeCsv("say \"hi\""), "\"say \"\"hi\"\"\"", "quote CSV value should be escaped.");
        Expect(errors, SceneToken.EscapeCsv("line\nbreak"), "\"line\nbreak\"", "newline CSV value should be quoted.");

        Expect(errors, SceneTokenEventLogger.EscapeCsv("event;value"), "event;value", "semicolon event payload should not be quoted.");
        Expect(errors, SceneTokenEventLogger.EscapeCsv("response=RIGHT,expected=LEFT"), "\"response=RIGHT,expected=LEFT\"", "comma event payload should be quoted.");
    }

    private static void CheckMetricByteEstimates(List<string> errors)
    {
        Expect(errors, SceneTokenMetrics.EstimateJsonBytes(null), 0, "null token JSON bytes should be zero.");
        Expect(errors, SceneTokenMetrics.EstimateCompactSceneTokenBytes(null), 0, "null token compact bytes should be zero.");
        Expect(errors, SceneTokenMetrics.EstimateObjectMetadataBytes(), 17, "object metadata byte estimate should match documented layout.");

        var token = new SceneToken
        {
            speakerId = "A",
            direction = SceneTokenDirection.FRONT.ToString(),
            distance = SceneTokenDistance.NEAR.ToString(),
            speakingState = SceneSpeakingState.SPEAKING.ToString(),
            turnState = SceneTurnState.TURN_HOLDER.ToString(),
            semanticToken = SceneSemanticToken.WARNING.ToString(),
            urgency = SceneUrgency.HIGH.ToString(),
            targetObjectId = "door",
            priority = 0.8f,
            semanticConfidence = 0.9f
        };

        Expect(errors, SceneTokenMetrics.EstimateCompactSceneTokenBytes(token), 12, "compact scene token byte estimate should match documented layout.");
        if (SceneTokenMetrics.EstimateJsonBytes(token) <= SceneTokenMetrics.EstimateCompactSceneTokenBytes(token))
        {
            errors.Add("JSON byte estimate should be larger than compact scene token estimate for a populated token.");
        }
    }

    private static void CheckPrioritySelectionPolicy(List<string> errors)
    {
        var policyObject = new GameObject("SelfCheckPrioritySelectionPolicy");

        try
        {
            var policy = policyObject.AddComponent<PrioritySelectionPolicy>();
            var lowPriority = CreateGeneratedToken("E_LOW", "Workpiece01", "WorkpieceWork", 0);
            var normalPriority = CreateGeneratedToken("E_NORMAL", "Workpiece01", "WorkpieceWork", 1);
            var criticalPriority = CreateGeneratedToken("E_CRITICAL", "Workpiece01", "WorkpieceWork", 2);
            var unrelated = CreateGeneratedToken("E_UNRELATED", "OtherMachine", "WorkpieceWork", 0);

            policy.SetComparisonMode(ComparisonSelectionMode.FullTransmission);
            ExpectSelection(errors, policy.Select(lowPriority), true, CommunicationLevel.AudioAndToken,
                "full transmission should send low-priority events with audio and token");

            policy.SetComparisonMode(ComparisonSelectionMode.PriorityOnly);
            ExpectSelection(errors, policy.Select(lowPriority), false, CommunicationLevel.None,
                "priority-only mode should suppress priority 0");
            ExpectSelection(errors, policy.Select(normalPriority), true, CommunicationLevel.TokenOnly,
                "priority-only mode should send priority 1 as token only");
            ExpectSelection(errors, policy.Select(criticalPriority), true, CommunicationLevel.AudioAndToken,
                "priority-only mode should send priority 2 with audio and token");

            policy.SetComparisonMode(ComparisonSelectionMode.ContextAndUserState);
            policy.SetGuidanceNeed(0.8f);
            ExpectSelection(errors, policy.Select(normalPriority), true, CommunicationLevel.TokenOnly,
                "high guidance need should send new task-relevant procedural information");
            ExpectSelection(errors, policy.Select(normalPriority), false, CommunicationLevel.None,
                "proposed mode should suppress already presented target information");
            ExpectSelection(errors, policy.Select(unrelated), false, CommunicationLevel.None,
                "proposed mode should suppress task-irrelevant information");
            ExpectSelection(errors, policy.Select(criticalPriority), true, CommunicationLevel.AudioAndToken,
                "proposed mode should always send critical information even for a known target");

            policy.ResetUserState();
            ExpectSelection(errors, policy.Select(normalPriority), true, CommunicationLevel.TokenOnly,
                "resetting user state should make target information new again");

            policy.SetGuidanceNeed(0.2f);
            SelectionResult lowNeedRoutine = policy.Select(normalPriority);
            ExpectSelection(errors, lowNeedRoutine, false, CommunicationLevel.None,
                "low guidance need should suppress routine procedural information");
            if (lowNeedRoutine != null && lowNeedRoutine.totalScore >= lowNeedRoutine.decisionThreshold)
            {
                errors.Add("low-need routine score should remain below its decision threshold.");
            }
            ExpectSelection(errors, policy.Select(criticalPriority), true, CommunicationLevel.AudioAndToken,
                "low guidance need should still receive critical information");

            CheckSelectionCountDifference(errors, policy);
        }
        finally
        {
            Object.DestroyImmediate(policyObject);
        }
    }

    private static void CheckSelectionCountDifference(
        List<string> errors, PrioritySelectionPolicy policy)
    {
        var sequence = new[]
        {
            CreateGeneratedToken("S_ROUTINE_NEW", "Workpiece01", "WorkpieceWork", 1),
            CreateGeneratedToken("S_ROUTINE_REPEAT", "Workpiece01", "WorkpieceWork", 1),
            CreateGeneratedToken("S_UNRELATED", "OtherMachine", "WorkpieceWork", 0),
            CreateGeneratedToken("S_WARNING", "Workpiece01", "WorkpieceWork", 2),
            CreateGeneratedToken("S_RESULT", "Workpiece01", "WorkpieceWork", 2)
        };

        policy.SetGuidanceNeed(0.8f);
        policy.SetComparisonMode(ComparisonSelectionMode.FullTransmission);
        Expect(errors, CountSelected(policy, sequence), 5,
            "full transmission comparison count");

        policy.SetComparisonMode(ComparisonSelectionMode.PriorityOnly);
        Expect(errors, CountSelected(policy, sequence), 4,
            "priority-only comparison count");

        policy.SetGuidanceNeed(0.8f);
        policy.SetComparisonMode(ComparisonSelectionMode.ContextAndUserState);
        Expect(errors, CountSelected(policy, sequence), 3,
            "proposed high-need comparison count");

        policy.SetGuidanceNeed(0.2f);
        policy.SetComparisonMode(ComparisonSelectionMode.ContextAndUserState);
        Expect(errors, CountSelected(policy, sequence), 2,
            "proposed low-need comparison count");
    }

    private static int CountSelected(PrioritySelectionPolicy policy, GeneratedSceneToken[] tokens)
    {
        policy.ResetUserState();
        int selected = 0;
        foreach (GeneratedSceneToken token in tokens)
        {
            SelectionResult result = policy.Select(token);
            if (result != null && result.selected) selected++;
        }
        return selected;
    }

    private static void CheckAdaptivePresentationPolicy(List<string> errors)
    {
        var policyObject = new GameObject("SelfCheckAdaptivePresentationPolicy");

        try
        {
            var selectionPolicy = policyObject.AddComponent<PrioritySelectionPolicy>();
            var presentationPolicy = policyObject.AddComponent<PresentationPolicy>();
            var critical = CreateGeneratedToken(
                "P_WARNING", "Workpiece01", "WorkpieceWork", 2);
            critical.utteranceId = "WRONG_PLACEMENT";
            critical.direction = "Front Right";

            selectionPolicy.SetComparisonMode(ComparisonSelectionMode.ContextAndUserState);
            selectionPolicy.SetGuidanceNeed(0.8f);
            SelectionResult highNeedSelection = selectionPolicy.Select(critical);
            PresentationResult highNeed = presentationPolicy.Present(critical, highNeedSelection, null);

            selectionPolicy.SetGuidanceNeed(0.2f);
            SelectionResult lowNeedSelection = selectionPolicy.Select(critical);
            PresentationResult lowNeed = presentationPolicy.Present(critical, lowNeedSelection, null);

            Expect(errors, highNeed.mode, "AdaptiveDetailedCritical",
                "high-need critical presentation mode");
            Expect(errors, lowNeed.mode, "AdaptiveCompactAlert",
                "low-need critical presentation mode");
            if (string.IsNullOrEmpty(highNeed.message) || !highNeed.message.Contains("Front Right"))
            {
                errors.Add("high-need guidance should explicitly include direction.");
            }
            if (!string.IsNullOrEmpty(lowNeed.message) && lowNeed.message.Contains("Front Right"))
            {
                errors.Add("low-need compact alert should omit redundant direction text.");
            }
            if (highNeed.cueScale <= lowNeed.cueScale || highNeed.cueDuration <= lowNeed.cueDuration)
            {
                errors.Add("high-need guidance should be larger and longer than low-need guidance.");
            }
        }
        finally
        {
            Object.DestroyImmediate(policyObject);
        }
    }

    private static void CheckUserAdaptation(List<string> errors)
    {
        var adaptationObject = new GameObject("SelfCheckUserAdaptation");

        try
        {
            var selectionPolicy = adaptationObject.AddComponent<PrioritySelectionPolicy>();
            selectionPolicy.SetGuidanceNeed(0.50f);
            var adaptation = adaptationObject.AddComponent<UserAdaptationController>();

            adaptation.RecordHelpRequest("self_check_help");
            ExpectClose(errors, selectionPolicy.GuidanceNeed, 0.75f,
                "help request guidance need");
            adaptation.RecordError("self_check_error");
            ExpectClose(errors, selectionPolicy.GuidanceNeed, 0.95f,
                "error guidance need");
            adaptation.RecordSuccess("self_check_success");
            ExpectClose(errors, selectionPolicy.GuidanceNeed, 0.80f,
                "success guidance need");

            Expect(errors, adaptation.HelpCount, 1, "help event count");
            Expect(errors, adaptation.ErrorCount, 1, "error event count");
            Expect(errors, adaptation.SuccessCount, 1, "success event count");
        }
        finally
        {
            Object.DestroyImmediate(adaptationObject);
        }
    }

    private static GeneratedSceneToken CreateGeneratedToken(
        string eventId, string targetObjectId, string taskState, int priority)
    {
        return new GeneratedSceneToken
        {
            scenarioId = "SELF_CHECK",
            eventId = eventId,
            targetObjectId = targetObjectId,
            taskState = taskState,
            priority = priority
        };
    }

    private static void ExpectSelection(
        List<string> errors,
        SelectionResult actual,
        bool expectedSelected,
        CommunicationLevel expectedLevel,
        string message)
    {
        if (actual == null)
        {
            errors.Add(message + ": selection result was null.");
            return;
        }

        if (actual.selected != expectedSelected || actual.level != expectedLevel)
        {
            errors.Add(message + " actualSelected=" + actual.selected +
                " expectedSelected=" + expectedSelected +
                " actualLevel=" + actual.level +
                " expectedLevel=" + expectedLevel);
        }
    }

    private static void Expect<T>(List<string> errors, T actual, T expected, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
        {
            errors.Add(message + " actual=" + actual + " expected=" + expected);
        }
    }

    private static void ExpectClose(List<string> errors, float actual, float expected, string message)
    {
        if (Mathf.Abs(actual - expected) > 0.01f)
        {
            errors.Add(message + " actual=" + actual.ToString("F3") + " expected=" + expected.ToString("F3"));
        }
    }
}
#endif
