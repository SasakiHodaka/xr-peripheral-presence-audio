using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PeripheralResearchSetup
{
    private const string LearnedCueModelPath = "Assets/Models/cue_model_unity.json";

    [MenuItem("Tools/Peripheral Research/Create Demo Hierarchy")]
    public static void CreateDemoHierarchy()
    {
        OpenDefaultSceneIfNeeded();

        Transform userHead = Camera.main != null ? Camera.main.transform : null;
        if (userHead == null)
        {
            GameObject cameraObject = GameObject.Find("Main Camera");
            if (cameraObject != null)
                userHead = cameraObject.transform;
        }

        GameObject systemObject = GetOrCreateRoot("PeripheralSystem");
        PeripheralStateDetector detector = GetOrAdd<PeripheralStateDetector>(systemObject);
        PeripheralCueModel cueModel = GetOrAdd<PeripheralCueModel>(systemObject);
        EnvironmentAcousticProfile environmentProfile = GetOrAdd<EnvironmentAcousticProfile>(systemObject);
        PeripheralCueExperimentController experimentController = GetOrAdd<PeripheralCueExperimentController>(systemObject);
        PeripheralCueAudioEmitter audioEmitter = GetOrAdd<PeripheralCueAudioEmitter>(systemObject);
        PeripheralStateLogger logger = GetOrAdd<PeripheralStateLogger>(systemObject);
        PeripheralTrialController trialController = GetOrAdd<PeripheralTrialController>(systemObject);
        PeripheralTrialConditionController conditionController = GetOrAdd<PeripheralTrialConditionController>(systemObject);
        PeripheralAuiLogCollectionController auiLogController = GetOrAdd<PeripheralAuiLogCollectionController>(systemObject);
        PeripheralCueTrialSequencer trialSequencer = GetOrAdd<PeripheralCueTrialSequencer>(systemObject);
        GetOrAdd<PeripheralSimulationDatasetGenerator>(systemObject);
        PeripheralDebugUI debugUI = GetOrAdd<PeripheralDebugUI>(systemObject);

        cueModel.environmentProfile = environmentProfile;

        logger.detector = detector;
        logger.cueModel = cueModel;
        logger.audioEmitter = audioEmitter;
        logger.experimentController = experimentController;
        logger.trialController = trialController;

        experimentController.trialController = trialController;
        experimentController.conditionController = conditionController;

        audioEmitter.detector = detector;
        audioEmitter.cueModel = cueModel;
        audioEmitter.experimentController = experimentController;
        AssignDefaultClips(audioEmitter);

        conditionController.detector = detector;
        conditionController.logger = logger;
        conditionController.experimentController = experimentController;

        auiLogController.cueModel = cueModel;
        auiLogController.environmentProfile = environmentProfile;
        auiLogController.conditionController = conditionController;
        auiLogController.trialController = trialController;
        auiLogController.logger = logger;

        trialSequencer.trialController = trialController;
        trialSequencer.conditionController = conditionController;
        trialSequencer.experimentController = experimentController;
        trialSequencer.logger = logger;

        debugUI.detector = detector;
        debugUI.cueModel = cueModel;
        debugUI.audioEmitter = audioEmitter;
        debugUI.auiLogController = auiLogController;
        debugUI.trialController = trialController;
        debugUI.conditionController = conditionController;
        debugUI.experimentController = experimentController;
        debugUI.trialSequencer = trialSequencer;

        detector.userHead = userHead;
        detector.autoFindTargets = false;
        detector.targets.Clear();

        GameObject targetsRoot = GetOrCreateRoot("PeripheralTargets");
        conditionController.approachTarget = CreateTarget("Target_Approach", targetsRoot.transform, userHead, DemoAvatarMoveMode.ApproachUser, new Vector3(0f, 1.6f, 5f), detector, trialController);
        conditionController.crossingTarget = CreateTarget("Target_Crossing", targetsRoot.transform, userHead, DemoAvatarMoveMode.CrossInFront, new Vector3(-2f, 1.6f, 2f), detector, trialController);
        conditionController.speakingTarget = CreateTarget("Target_Speaking", targetsRoot.transform, userHead, DemoAvatarMoveMode.Idle, new Vector3(2f, 1.6f, 2.5f), detector, trialController, true);
        conditionController.backApproachTarget = CreateTarget("Target_Back", targetsRoot.transform, userHead, DemoAvatarMoveMode.BackApproach, new Vector3(0f, 1.6f, -5f), detector, trialController);
        conditionController.ApplyCondition();

        Selection.activeGameObject = systemObject;
        EditorUtility.SetDirty(systemObject);
        EditorUtility.SetDirty(targetsRoot);
        EditorSceneManager.MarkSceneDirty(systemObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Peripheral research demo hierarchy created. Assign XR Origin/Main Camera to PeripheralStateDetector.userHead if it is empty.");
    }

    private static void OpenDefaultSceneIfNeeded()
    {
        const string scenePath = "Assets/Scenes/SampleScene.unity";
        if (!Application.isBatchMode)
            return;

        if (EditorSceneManager.GetActiveScene().path == scenePath)
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            EditorSceneManager.OpenScene(scenePath);
    }

    [MenuItem("Tools/Peripheral Research/Use Learned Cue Model")]
    public static void UseLearnedCueModel()
    {
        GameObject systemObject = GameObject.Find("PeripheralSystem");
        if (systemObject == null)
        {
            Debug.LogWarning("PeripheralSystem was not found. Run Tools/Peripheral Research/Create Demo Hierarchy first.");
            return;
        }

        PeripheralCueModel cueModel = systemObject.GetComponent<PeripheralCueModel>();
        if (cueModel == null)
            cueModel = Undo.AddComponent<PeripheralCueModel>(systemObject);

        TextAsset learnedModel = AssetDatabase.LoadAssetAtPath<TextAsset>(LearnedCueModelPath);
        if (learnedModel == null)
        {
            Debug.LogWarning("Learned cue model JSON was not found at " + LearnedCueModelPath + ". Run python Tools/train_cue_model.py first.");
            return;
        }

        Undo.RecordObject(cueModel, "Use Learned Cue Model");
        cueModel.comparisonCondition = PeripheralCueComparisonCondition.LearnedCue;
        cueModel.learnedModelJson = learnedModel;
        EditorUtility.SetDirty(cueModel);
        Selection.activeGameObject = systemObject;
        Debug.Log("PeripheralCueModel is now using LearnedCue with " + LearnedCueModelPath + ".");
    }

    [MenuItem("Tools/Peripheral Research/Use Learned Cue Model", true)]
    public static bool ValidateUseLearnedCueModel()
    {
        return AssetDatabase.LoadAssetAtPath<TextAsset>(LearnedCueModelPath) != null;
    }

    private static void AssignDefaultClips(PeripheralCueAudioEmitter audioEmitter)
    {
        if (audioEmitter == null) return;

        if (audioEmitter.footstepClip == null)
            audioEmitter.footstepClip = FindAudioClip("footsteps");

        if (audioEmitter.breathingClip == null)
            audioEmitter.breathingClip = FindAudioClip("breathing");

        if (audioEmitter.clothRustleClip == null)
            audioEmitter.clothRustleClip = FindAudioClip("clothing");

        if (audioEmitter.ambientPresenceClip == null)
            audioEmitter.ambientPresenceClip = audioEmitter.breathingClip != null ? audioEmitter.breathingClip : audioEmitter.clothRustleClip;
    }

    private static AudioClip FindAudioClip(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets(namePart + " t:AudioClip", new[] { "Assets/Audio" });
        if (guids == null || guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static PeripheralTarget CreateTarget(
        string targetId,
        Transform parent,
        Transform userHead,
        DemoAvatarMoveMode moveMode,
        Vector3 fallbackPosition,
        PeripheralStateDetector detector,
        PeripheralTrialController trialController,
        bool speakingDemo = false)
    {
        GameObject targetObject = GameObject.Find(targetId);
        if (targetObject == null)
        {
            targetObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            targetObject.name = targetId;
            Undo.RegisterCreatedObjectUndo(targetObject, "Create Peripheral Target");
        }

        targetObject.transform.SetParent(parent);
        targetObject.transform.position = userHead != null ? userHead.position + fallbackPosition : fallbackPosition;

        PeripheralTarget target = GetOrAdd<PeripheralTarget>(targetObject);
        target.targetId = targetId;
        target.bodyTransform = targetObject.transform;
        target.headTransform = targetObject.transform;
        target.gazeTransform = targetObject.transform;
        target.SetSpeaking(speakingDemo);

        DemoAvatarMover mover = GetOrAdd<DemoAvatarMover>(targetObject);
        mover.userHead = userHead;
        mover.target = target;
        mover.trialController = trialController;
        mover.moveMode = moveMode;
        mover.toggleSpeaking = speakingDemo;

        if (!detector.targets.Contains(target))
            detector.targets.Add(target);

        EditorUtility.SetDirty(targetObject);
        return target;
    }

    private static GameObject GetOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root != null)
            return root;

        root = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(root, "Create " + name);
        return root;
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
            return component;

        component = Application.isBatchMode ? gameObject.AddComponent<T>() : Undo.AddComponent<T>(gameObject);
        return component;
    }
}
