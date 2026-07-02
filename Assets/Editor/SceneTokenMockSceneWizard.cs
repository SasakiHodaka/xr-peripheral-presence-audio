#if UNITY_EDITOR
using SceneTokens;
using SemanticSpatialAudio.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTokenMockSceneWizard
{
    [MenuItem("Tools/Semantic Spatial Audio/Create Scene Token Mock Scene")]
    [MenuItem("Tools/Scene Tokens/Create Spatial Conversation Mock Scene")]
    public static void CreateSpatialConversationMockScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(3f, 1f, 3f);

        var light = new GameObject("Directional Light");
        var lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = 1.1f;
        light.transform.rotation = Quaternion.Euler(50f, -25f, 0f);

        var camera = new GameObject("Listener Camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();
        camera.transform.position = new Vector3(0f, 1.6f, -2.4f);
        camera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        var speakerA = CreateSpeaker("AvatarA", "A", new Vector3(-1.4f, 0f, 1.2f), new Color(0.2f, 0.55f, 1f), KeyCode.A, KeyCode.Q, SceneSemanticToken.QUESTION, 220f);
        var speakerB = CreateSpeaker("AvatarB", "B", new Vector3(1.5f, 0f, 1.8f), new Color(0.2f, 0.8f, 0.35f), KeyCode.B, KeyCode.W, SceneSemanticToken.ANSWER, 280f);
        var speakerC = CreateSpeaker("AvatarC", "C", new Vector3(0.2f, 0f, 3.4f), new Color(1f, 0.55f, 0.15f), KeyCode.C, KeyCode.E, SceneSemanticToken.INSTRUCTION, 340f);

        var managerObject = new GameObject("SceneTokenSystem");
        var logger = managerObject.AddComponent<SceneTokenLogger>();
        var eventLogger = managerObject.AddComponent<SceneTokenEventLogger>();
        var metrics = managerObject.AddComponent<SceneTokenMetrics>();
        var renderer = managerObject.AddComponent<SceneTokenDecoderRenderer>();
        var conditionController = managerObject.AddComponent<SceneTokenConditionController>();
        var experimentSession = managerObject.AddComponent<SceneTokenExperimentSession>();
        var scriptedConversation = managerObject.AddComponent<SceneTokenScriptedConversation>();
        var manager = managerObject.AddComponent<SceneTokenManager>();

        var speakers = new[] { speakerA, speakerB, speakerC };
        renderer.listener = camera.transform;
        renderer.speakers = speakers;
        renderer.repositionAudioSources = true;
        renderer.renderCondition = SceneTokenRenderCondition.FULL_SCENE_TOKEN;

        metrics.decoderRenderer = renderer;
        conditionController.decoderRenderer = renderer;
        conditionController.eventLogger = eventLogger;

        experimentSession.conditionController = conditionController;
        experimentSession.decoderRenderer = renderer;
        experimentSession.eventLogger = eventLogger;
        experimentSession.conditionDurationSeconds = 30f;
        experimentSession.autoAdvanceCondition = true;
        experimentSession.conditionOrder = new[]
        {
            SceneTokenRenderCondition.TRADITIONAL,
            SceneTokenRenderCondition.DIRECTION_DISTANCE,
            SceneTokenRenderCondition.FULL_SCENE_TOKEN
        };
        experimentSession.scriptedConversation = scriptedConversation;
        experimentSession.startScriptedConversationWithTrial = true;

        scriptedConversation.speakers = speakers;
        scriptedConversation.experimentSession = experimentSession;
        scriptedConversation.eventLogger = eventLogger;
        scriptedConversation.playOnStart = false;
        scriptedConversation.autoStartWithExperimentSession = false;
        scriptedConversation.loopScript = true;
        AssignConversationClips(scriptedConversation);

        manager.listener = camera.transform;
        manager.speakers = speakers;
        manager.logger = logger;
        manager.decoderRenderer = renderer;
        manager.metrics = metrics;
        manager.experimentSession = experimentSession;
        manager.scriptedConversation = scriptedConversation;
        manager.tokenUpdateInterval = 0.1f;
        manager.logOnlyDuringExperimentSession = true;
        manager.showDebugHud = true;

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/SceneTokenMock.unity");
        Selection.activeGameObject = managerObject;
        EditorGUIUtility.PingObject(managerObject);
    }

    private static SpeakerObject CreateSpeaker(
        string objectName,
        string speakerId,
        Vector3 position,
        Color color,
        KeyCode toggleKey,
        KeyCode semanticKey,
        SceneSemanticToken semanticToken,
        float toneFrequency)
    {
        var root = new GameObject(objectName);
        root.transform.position = position;

        var body = CreateCube("Body", position + new Vector3(0f, 0.8f, 0f), new Vector3(0.35f, 0.8f, 0.25f), color);
        var head = CreateCube("Head", position + new Vector3(0f, 1.35f, 0f), new Vector3(0.28f, 0.28f, 0.28f), color);
        body.transform.SetParent(root.transform);
        head.transform.SetParent(root.transform);

        var voiceOutput = new GameObject("VoiceOutput");
        voiceOutput.transform.position = position + new Vector3(0f, 1.35f, 0f);
        voiceOutput.transform.SetParent(root.transform);
        var audioSource = voiceOutput.AddComponent<AudioSource>();

        var speaker = root.AddComponent<SpeakerObject>();
        speaker.speakerId = speakerId;
        speaker.audioSource = audioSource;
        speaker.toggleKey = toggleKey;
        speaker.cycleSemanticKey = semanticKey;
        speaker.semanticToken = semanticToken;
        speaker.utteranceText = GetDefaultUtterance(semanticToken);
        speaker.generatedToneFrequency = toneFrequency;
        speaker.generatedToneAmplitude = 0.18f;

        var label = root.AddComponent<SpeakerDebugLabel>();
        label.speaker = speaker;
        return speaker;
    }

    private static string GetDefaultUtterance(SceneSemanticToken token)
    {
        switch (token)
        {
            case SceneSemanticToken.QUESTION:
                return "Can you check this object for me?";
            case SceneSemanticToken.ANSWER:
                return "Yes, that part looks aligned.";
            case SceneSemanticToken.INSTRUCTION:
                return "Move this object forward, please.";
            case SceneSemanticToken.AGREEMENT:
                return "I think that direction is good.";
            case SceneSemanticToken.DISAGREEMENT:
                return "I think that is slightly off.";
            case SceneSemanticToken.WARNING:
                return "Be careful, the direction is wrong.";
            case SceneSemanticToken.CHAT:
                return "Let's continue working like this.";
            default:
                return string.Empty;
        }
    }

    private static void AssignConversationClips(SceneTokenScriptedConversation scriptedConversation)
    {
        if (scriptedConversation == null || scriptedConversation.utterances == null)
        {
            return;
        }

        for (var i = 0; i < scriptedConversation.utterances.Length; i++)
        {
            var utterance = scriptedConversation.utterances[i];
            if (utterance == null)
            {
                continue;
            }

            utterance.audioClip = LoadClipForSemanticToken(utterance.semanticToken);
        }
    }

    private static AudioClip LoadClipForSemanticToken(SceneSemanticToken token)
    {
        var fileName = string.Empty;
        switch (token)
        {
            case SceneSemanticToken.QUESTION:
                fileName = "scene_token_question.wav";
                break;
            case SceneSemanticToken.ANSWER:
                fileName = "scene_token_answer.wav";
                break;
            case SceneSemanticToken.INSTRUCTION:
                fileName = "scene_token_instruction.wav";
                break;
            case SceneSemanticToken.WARNING:
                fileName = "scene_token_warning.wav";
                break;
            case SceneSemanticToken.AGREEMENT:
                fileName = "scene_token_agreement.wav";
                break;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + fileName);
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Color color)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard"))
        {
            color = color
        };
        return item;
    }
}
#endif
