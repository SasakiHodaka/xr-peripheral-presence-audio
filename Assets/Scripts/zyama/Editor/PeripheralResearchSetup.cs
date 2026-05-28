using UnityEditor;
using UnityEngine;

public static class PeripheralResearchSetup
{
    [MenuItem("Tools/Peripheral Research/Create Demo Hierarchy")]
    public static void CreateDemoHierarchy()
    {
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
        PeripheralStateLogger logger = GetOrAdd<PeripheralStateLogger>(systemObject);
        PeripheralTrialController trialController = GetOrAdd<PeripheralTrialController>(systemObject);
        PeripheralTrialConditionController conditionController = GetOrAdd<PeripheralTrialConditionController>(systemObject);
        PeripheralDebugUI debugUI = GetOrAdd<PeripheralDebugUI>(systemObject);
        logger.detector = detector;
        logger.cueModel = cueModel;
        logger.trialController = trialController;
        conditionController.detector = detector;
        conditionController.logger = logger;
        debugUI.detector = detector;
        debugUI.cueModel = cueModel;
        debugUI.trialController = trialController;
        debugUI.conditionController = conditionController;
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
        Debug.Log("Peripheral research demo hierarchy created. Assign XR Origin/Main Camera to PeripheralStateDetector.userHead if it is empty.");
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

        component = Undo.AddComponent<T>(gameObject);
        return component;
    }
}
