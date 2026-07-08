#if UNITY_EDITOR
using System.Collections.Generic;
using SceneTokens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneTokenSceneValidator
{
    private const string ScenePath = "Assets/Scenes/SceneTokenMock.unity";

    [MenuItem("Tools/Semantic Spatial Audio/Validate Scene Token Mock Scene")]
    [MenuItem("Tools/Scene Tokens/Validate Spatial Conversation Mock Scene")]
    public static void ValidateSceneFromMenu()
    {
        var errorCount = ValidateScene();
        if (errorCount == 0)
        {
            Debug.Log("Scene token validation passed.");
        }
        else
        {
            Debug.LogError("Scene token validation failed with " + errorCount + " issue(s).");
        }
    }

    public static void ValidateSceneForBatch()
    {
        var errorCount = ValidateScene();
        EditorApplication.Exit(errorCount == 0 ? 0 : 1);
    }

    private static int ValidateScene()
    {
        var errors = new List<string>();

        var scene = EditorSceneManager.OpenScene(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            errors.Add("Scene could not be opened: " + ScenePath);
            return Report(errors);
        }

        var manager = Object.FindObjectOfType<SceneTokenManager>();
        Require(manager != null, errors, "SceneTokenManager is missing.");

        var logger = Object.FindObjectOfType<SceneTokenLogger>();
        var packetizer = Object.FindObjectOfType<ScenePacketizer>();
        var packetLogger = Object.FindObjectOfType<ScenePacketLogger>();
        var eventLogger = Object.FindObjectOfType<SceneTokenEventLogger>();
        var metrics = Object.FindObjectOfType<SceneTokenMetrics>();
        var renderer = Object.FindObjectOfType<SceneTokenDecoderRenderer>();
        var conditionController = Object.FindObjectOfType<SceneTokenConditionController>();
        var experimentSession = Object.FindObjectOfType<SceneTokenExperimentSession>();
        var scriptedConversation = Object.FindObjectOfType<SceneTokenScriptedConversation>();
        var speakers = Object.FindObjectsOfType<SpeakerObject>();
        var labels = Object.FindObjectsOfType<SemanticSpatialAudio.UI.SpeakerDebugLabel>();
        var listener = Camera.main != null ? Camera.main.transform : null;

        Require(logger != null, errors, "SceneTokenLogger is missing.");
        if (packetizer == null)
        {
            Debug.LogWarning("[SceneTokenValidation] ScenePacketizer is missing in the scene asset; SceneTokenManager will add one at runtime.");
        }

        if (packetLogger == null)
        {
            Debug.LogWarning("[SceneTokenValidation] ScenePacketLogger is missing in the scene asset; SceneTokenManager will add one at runtime.");
        }

        Require(eventLogger != null, errors, "SceneTokenEventLogger is missing.");
        Require(metrics != null, errors, "SceneTokenMetrics is missing.");
        Require(renderer != null, errors, "SceneTokenDecoderRenderer is missing.");
        Require(conditionController != null, errors, "SceneTokenConditionController is missing.");
        Require(experimentSession != null, errors, "SceneTokenExperimentSession is missing.");
        Require(scriptedConversation != null, errors, "SceneTokenScriptedConversation is missing.");
        Require(listener != null, errors, "Main camera listener transform is missing.");
        Require(Object.FindObjectOfType<AudioListener>() != null, errors, "AudioListener is missing.");
        Require(speakers.Length == 3, errors, "Expected exactly three SpeakerObject components, found " + speakers.Length + ".");
        Require(labels.Length == 3, errors, "Expected exactly three SpeakerDebugLabel components, found " + labels.Length + ".");

        if (manager != null)
        {
            Require(manager.listener != null, errors, "SceneTokenManager.listener is not assigned.");
            Require(manager.speakers != null && manager.speakers.Length == 3, errors, "SceneTokenManager.speakers must contain three speakers.");
            Require(manager.logger != null, errors, "SceneTokenManager.logger is not assigned.");
            Require(manager.eventLogger != null || manager.GetComponent<SceneTokenEventLogger>() != null, errors, "SceneTokenManager.eventLogger is not assigned.");
            Require(manager.decoderRenderer != null, errors, "SceneTokenManager.decoderRenderer is not assigned.");
            Require(manager.metrics != null, errors, "SceneTokenManager.metrics is not assigned.");
            Require(manager.experimentSession != null, errors, "SceneTokenManager.experimentSession is not assigned.");
            Require(manager.scriptedConversation != null, errors, "SceneTokenManager.scriptedConversation is not assigned.");
            Require(manager.logOnlyDuringExperimentSession, errors, "SceneTokenManager.logOnlyDuringExperimentSession should be enabled.");
            Require(manager.showDebugHud, errors, "SceneTokenManager.showDebugHud should be enabled.");
            Require(manager.tokenUpdateInterval > 0f, errors, "SceneTokenManager.tokenUpdateInterval must be positive.");
        }

        if (renderer != null)
        {
            Require(renderer.listener != null, errors, "SceneTokenDecoderRenderer.listener is not assigned.");
            Require(renderer.speakers != null && renderer.speakers.Length == 3, errors, "SceneTokenDecoderRenderer.speakers must contain three speakers.");
            Require(renderer.repositionAudioSources, errors, "SceneTokenDecoderRenderer.repositionAudioSources should be enabled.");
        }

        if (conditionController != null)
        {
            Require(conditionController.decoderRenderer != null, errors, "SceneTokenConditionController.decoderRenderer is not assigned.");
            Require(conditionController.eventLogger != null, errors, "SceneTokenConditionController.eventLogger is not assigned.");
        }

        if (experimentSession != null)
        {
            Require(experimentSession.conditionController != null, errors, "SceneTokenExperimentSession.conditionController is not assigned.");
            Require(experimentSession.decoderRenderer != null, errors, "SceneTokenExperimentSession.decoderRenderer is not assigned.");
            Require(experimentSession.eventLogger != null, errors, "SceneTokenExperimentSession.eventLogger is not assigned.");
            Require(experimentSession.scriptedConversation != null, errors, "SceneTokenExperimentSession.scriptedConversation is not assigned.");
            Require(
                HasMainConditions(experimentSession.conditionOrder),
                errors,
                "SceneTokenExperimentSession.conditionOrder should contain C1_TRADITIONAL, C2_DIRECTION_DISTANCE, C3_FULL_SCENE_TOKEN, and C4_SELECTED_SCENE_TOKEN.");
            Require(experimentSession.conditionDurationSeconds > 0f, errors, "SceneTokenExperimentSession.conditionDurationSeconds must be positive.");
            Require(!string.IsNullOrWhiteSpace(experimentSession.participantId), errors, "SceneTokenExperimentSession.participantId should have a default value.");
        }

        if (scriptedConversation != null)
        {
            Require(scriptedConversation.speakers != null && scriptedConversation.speakers.Length == 3, errors, "SceneTokenScriptedConversation.speakers must contain three speakers.");
            Require(scriptedConversation.eventLogger != null, errors, "SceneTokenScriptedConversation.eventLogger is not assigned.");
            Require(scriptedConversation.utterances != null && scriptedConversation.utterances.Length >= 5, errors, "SceneTokenScriptedConversation should have at least five utterances.");
        }

        ValidateSpeakers(speakers, errors);
        return Report(errors);
    }

    private static bool HasMainConditions(SceneTokenRenderCondition[] conditionOrder)
    {
        if (conditionOrder == null || conditionOrder.Length != 4)
        {
            return false;
        }

        var conditions = new HashSet<SceneTokenRenderCondition>(conditionOrder);
        return conditions.Contains(SceneTokenRenderCondition.C1_TRADITIONAL)
            && conditions.Contains(SceneTokenRenderCondition.C2_DIRECTION_DISTANCE)
            && conditions.Contains(SceneTokenRenderCondition.C3_FULL_SCENE_TOKEN)
            && conditions.Contains(SceneTokenRenderCondition.C4_SELECTED_SCENE_TOKEN);
    }

    private static void ValidateSpeakers(SpeakerObject[] speakers, ICollection<string> errors)
    {
        var ids = new HashSet<string>();
        for (var i = 0; i < speakers.Length; i++)
        {
            var speaker = speakers[i];
            if (speaker == null)
            {
                errors.Add("Speaker " + i + " is null.");
                continue;
            }

            Require(!string.IsNullOrWhiteSpace(speaker.speakerId), errors, speaker.name + " has an empty speakerId.");
            Require(ids.Add(speaker.speakerId), errors, "Duplicate speakerId: " + speaker.speakerId + ".");
            Require(speaker.audioSource != null, errors, speaker.name + ".audioSource is not assigned.");
            Require(speaker.toggleKey != KeyCode.None, errors, speaker.name + ".toggleKey is not assigned.");
            Require(speaker.cycleSemanticKey != KeyCode.None, errors, speaker.name + ".cycleSemanticKey is not assigned.");
            Require(speaker.generatedToneFrequency > 0f, errors, speaker.name + ".generatedToneFrequency must be positive.");
        }
    }

    private static void Require(bool condition, ICollection<string> errors, string message)
    {
        if (!condition)
        {
            errors.Add(message);
        }
    }

    private static int Report(IReadOnlyList<string> errors)
    {
        for (var i = 0; i < errors.Count; i++)
        {
            Debug.LogError("[SceneTokenValidation] " + errors[i]);
        }

        if (errors.Count == 0)
        {
            Debug.Log("[SceneTokenValidation] Passed.");
        }

        return errors.Count;
    }
}
#endif
