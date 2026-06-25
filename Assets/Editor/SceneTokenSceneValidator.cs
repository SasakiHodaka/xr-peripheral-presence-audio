#if UNITY_EDITOR
using System.Collections.Generic;
using SceneTokens;
using SemanticSpatialAudio.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneTokenSceneValidator
{
    private const string ScenePath = "Assets/Scenes/SceneTokenMock.unity";

    [MenuItem("Tools/Semantic Spatial Audio/Validate Scene Token Mock Scene")]
    [MenuItem("Tools/Scene Tokens/Validate Spatial Conversation Mock Scene")]
    public static void ValidateCurrentSceneFromMenu()
    {
        var result = ValidateCurrentScene();
        if (result.IsValid)
        {
            Debug.Log("[SceneTokenValidation] Passed.");
        }
        else
        {
            Debug.LogError("[SceneTokenValidation] Failed:\n" + string.Join("\n", result.Errors));
        }
    }

    public static void ValidateSceneForBatch()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var result = ValidateCurrentScene();

        if (result.IsValid)
        {
            Debug.Log("[SceneTokenValidation] Passed.");
            return;
        }

        Debug.LogError("[SceneTokenValidation] Failed:\n" + string.Join("\n", result.Errors));
        EditorApplication.Exit(1);
    }

    private static ValidationResult ValidateCurrentScene()
    {
        var result = new ValidationResult();

        var manager = Object.FindObjectOfType<SceneTokenManager>();
        var renderer = Object.FindObjectOfType<SceneTokenDecoderRenderer>();
        var logger = Object.FindObjectOfType<SceneTokenLogger>();
        var eventLogger = Object.FindObjectOfType<SceneTokenEventLogger>();
        var metrics = Object.FindObjectOfType<SceneTokenMetrics>();
        var conditionController = Object.FindObjectOfType<SceneTokenConditionController>();
        var experimentSession = Object.FindObjectOfType<SceneTokenExperimentSession>();
        var scriptedConversation = Object.FindObjectOfType<SceneTokenScriptedConversation>();
        var responseRecorder = Object.FindObjectOfType<SceneTokenResponseRecorder>();
        var speakers = Object.FindObjectsOfType<SpeakerObject>();

        Require(result, manager != null, "SceneTokenManager is missing.");
        Require(result, renderer != null, "SceneTokenDecoderRenderer is missing.");
        Require(result, logger != null, "SceneTokenLogger is missing.");
        Require(result, eventLogger != null, "SceneTokenEventLogger is missing.");
        Require(result, metrics != null, "SceneTokenMetrics is missing.");
        Require(result, conditionController != null, "SceneTokenConditionController is missing.");
        Require(result, experimentSession != null, "SceneTokenExperimentSession is missing.");
        Require(result, scriptedConversation != null, "SceneTokenScriptedConversation is missing.");
        Require(result, responseRecorder != null, "SceneTokenResponseRecorder is missing.");
        Require(result, speakers != null && speakers.Length == 3, "Expected exactly three SpeakerObject components.");
        Require(result, Camera.main != null, "MainCamera is missing.");
        if (Object.FindObjectOfType<AudioListener>() == null)
        {
            Debug.LogWarning("[SceneTokenValidation] AudioListener is not present in the scene. SceneTokenManager will add one to the listener at runtime.");
        }

        if (manager != null)
        {
            Require(result, manager.listener != null, "SceneTokenManager.listener is not assigned.");
            Require(result, manager.speakers != null && manager.speakers.Length == 3, "SceneTokenManager.speakers must contain three speakers.");
            Require(result, manager.logger != null, "SceneTokenManager.logger is not assigned.");
            Require(result, manager.decoderRenderer != null, "SceneTokenManager.decoderRenderer is not assigned.");
            Require(result, manager.metrics != null, "SceneTokenManager.metrics is not assigned.");
            Require(result, manager.experimentSession != null, "SceneTokenManager.experimentSession is not assigned.");
            Require(result, manager.scriptedConversation != null, "SceneTokenManager.scriptedConversation is not assigned.");
            Require(result, manager.tokenUpdateInterval > 0f, "SceneTokenManager.tokenUpdateInterval must be positive.");
            Require(result, manager.logOnlyDuringExperimentSession, "SceneTokenManager should log only during active experiment sessions.");
        }

        if (renderer != null)
        {
            Require(result, renderer.listener != null, "SceneTokenDecoderRenderer.listener is not assigned.");
            Require(result, renderer.speakers != null && renderer.speakers.Length == 3, "SceneTokenDecoderRenderer.speakers must contain three speakers.");
        }

        if (metrics != null)
        {
            Require(result, metrics.decoderRenderer != null, "SceneTokenMetrics.decoderRenderer is not assigned.");
            Require(result, metrics.experimentSession != null, "SceneTokenMetrics.experimentSession is not assigned.");
        }

        if (conditionController != null)
        {
            Require(result, conditionController.decoderRenderer != null, "SceneTokenConditionController.decoderRenderer is not assigned.");
            Require(result, conditionController.eventLogger != null, "SceneTokenConditionController.eventLogger is not assigned.");
        }

        if (experimentSession != null)
        {
            Require(result, experimentSession.conditionController != null, "SceneTokenExperimentSession.conditionController is not assigned.");
            Require(result, experimentSession.decoderRenderer != null, "SceneTokenExperimentSession.decoderRenderer is not assigned.");
            Require(result, experimentSession.eventLogger != null, "SceneTokenExperimentSession.eventLogger is not assigned.");
            Require(result, experimentSession.conditionOrder != null && experimentSession.conditionOrder.Length == 5, "SceneTokenExperimentSession.conditionOrder must contain five conditions.");
            Require(result, experimentSession.conditionDurationSeconds > 0f, "SceneTokenExperimentSession.conditionDurationSeconds must be positive.");
        }

        if (scriptedConversation != null)
        {
            Require(result, scriptedConversation.speakers != null && scriptedConversation.speakers.Length == 3, "SceneTokenScriptedConversation.speakers must contain three speakers.");
            Require(result, scriptedConversation.experimentSession != null, "SceneTokenScriptedConversation.experimentSession is not assigned.");
            Require(result, scriptedConversation.eventLogger != null, "SceneTokenScriptedConversation.eventLogger is not assigned.");
            Require(result, scriptedConversation.utterances != null && scriptedConversation.utterances.Length >= 4, "SceneTokenScriptedConversation should contain at least four utterances.");
        }

        if (responseRecorder != null)
        {
            Require(result, responseRecorder.tokenManager != null, "SceneTokenResponseRecorder.tokenManager is not assigned.");
            Require(result, responseRecorder.experimentSession != null, "SceneTokenResponseRecorder.experimentSession is not assigned.");
            Require(result, responseRecorder.decoderRenderer != null, "SceneTokenResponseRecorder.decoderRenderer is not assigned.");
            Require(result, responseRecorder.eventLogger != null, "SceneTokenResponseRecorder.eventLogger is not assigned.");
            Require(result, responseRecorder.speakers != null && responseRecorder.speakers.Length == 3, "SceneTokenResponseRecorder.speakers must contain three speakers.");
            Require(result, responseRecorder.recordOnlyDuringExperimentSession, "SceneTokenResponseRecorder should record only during active experiment sessions.");
        }

        ValidateSpeakers(result, speakers);
        return result;
    }

    private static void ValidateSpeakers(ValidationResult result, SpeakerObject[] speakers)
    {
        if (speakers == null)
        {
            return;
        }

        var ids = new HashSet<string>();
        for (var i = 0; i < speakers.Length; i++)
        {
            var speaker = speakers[i];
            if (speaker == null)
            {
                result.Errors.Add("SpeakerObject entry is null.");
                continue;
            }

            Require(result, !string.IsNullOrEmpty(speaker.speakerId), speaker.name + " has an empty speakerId.");
            Require(result, ids.Add(speaker.speakerId), "Duplicate speakerId: " + speaker.speakerId);
            Require(result, speaker.audioSource != null, speaker.name + " has no AudioSource assigned.");
            Require(result, speaker.toggleKey != KeyCode.None, speaker.name + " has no toggle key.");
            Require(result, speaker.cycleSemanticKey != KeyCode.None, speaker.name + " has no semantic-cycle key.");
            Require(result, speaker.GetComponent<SpeakerDebugLabel>() != null, speaker.name + " has no SpeakerDebugLabel.");
        }
    }

    private static void Require(ValidationResult result, bool condition, string message)
    {
        if (!condition)
        {
            result.Errors.Add(message);
        }
    }

    private sealed class ValidationResult
    {
        public readonly List<string> Errors = new List<string>();

        public bool IsValid
        {
            get { return Errors.Count == 0; }
        }
    }
}
#endif
