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
        PeripheralStateLogger logger = GetOrAdd<PeripheralStateLogger>(systemObject);
        PeripheralDebugUI debugUI = GetOrAdd<PeripheralDebugUI>(systemObject);
        logger.detector = detector;
        debugUI.detector = detector;
        detector.userHead = userHead;
        detector.autoFindTargets = false;
        detector.targets.Clear();

        GameObject targetsRoot = GetOrCreateRoot("PeripheralTargets");
        CreateTarget("Target_Approach", targetsRoot.transform, userHead, DemoAvatarMoveMode.ApproachUser, new Vector3(0f, 1.6f, 5f), detector);
        CreateTarget("Target_Crossing", targetsRoot.transform, userHead, DemoAvatarMoveMode.CrossInFront, new Vector3(-2f, 1.6f, 2f), detector);
        CreateTarget("Target_Speaking", targetsRoot.transform, userHead, DemoAvatarMoveMode.Idle, new Vector3(2f, 1.6f, 2.5f), detector, true);
        CreateTarget("Target_Back", targetsRoot.transform, userHead, DemoAvatarMoveMode.BackApproach, new Vector3(0f, 1.6f, -5f), detector);

        Selection.activeGameObject = systemObject;
        EditorUtility.SetDirty(systemObject);
        EditorUtility.SetDirty(targetsRoot);
        Debug.Log("Peripheral research demo hierarchy created. Assign XR Origin/Main Camera to PeripheralStateDetector.userHead if it is empty.");
    }

    private static void CreateTarget(
        string targetId,
        Transform parent,
        Transform userHead,
        DemoAvatarMoveMode moveMode,
        Vector3 fallbackPosition,
        PeripheralStateDetector detector,
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
        mover.moveMode = moveMode;
        mover.toggleSpeaking = speakingDemo;

        if (!detector.targets.Contains(target))
            detector.targets.Add(target);

        EditorUtility.SetDirty(targetObject);
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
