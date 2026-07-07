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
