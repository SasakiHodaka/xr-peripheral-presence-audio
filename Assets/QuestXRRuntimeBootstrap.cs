using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public sealed class QuestXRRuntimeBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindObjectOfType<QuestXRRuntimeBootstrap>() != null) return;
        new GameObject("QuestXRRuntimeBootstrap").AddComponent<QuestXRRuntimeBootstrap>();
    }

    private IEnumerator Start()
    {
        for (int frame = 0; frame < 120 && !XRSettings.isDeviceActive; frame++)
            yield return null;

        if (!XRSettings.isDeviceActive)
        {
            Debug.Log("[QuestXR] No active XR device; keeping desktop mouse controls.");
            yield break;
        }

        CreateRig();
    }

    private static void CreateRig()
    {
        if (FindObjectOfType<XROrigin>() != null) return;
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[QuestXR] Main camera not found.");
            return;
        }

        GameObject managerObject = new GameObject("XR Interaction Manager");
        XRInteractionManager manager = managerObject.AddComponent<XRInteractionManager>();

        GameObject originObject = new GameObject("Quest XR Origin");
        XROrigin origin = originObject.AddComponent<XROrigin>();
        GameObject offsetObject = new GameObject("Camera Offset");
        offsetObject.transform.SetParent(originObject.transform, false);

        camera.transform.SetParent(offsetObject.transform, false);
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        origin.Camera = camera;
        origin.CameraFloorOffsetObject = offsetObject;

        CreateController("Left Hand Controller", XRNode.LeftHand, offsetObject.transform, manager);
        CreateController("Right Hand Controller", XRNode.RightHand, offsetObject.transform, manager);
        Debug.Log("[QuestXR] Device-based Quest direct-grab rig created.");
    }

    private static void CreateController(
        string name, XRNode node, Transform parent, XRInteractionManager manager)
    {
        GameObject controllerObject = new GameObject(name);
        controllerObject.transform.SetParent(parent, false);

        Rigidbody body = controllerObject.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        SphereCollider collider = controllerObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.10f;

        XRController controller = controllerObject.AddComponent<XRController>();
        controller.controllerNode = node;
        controller.selectUsage = InputHelpers.Button.Grip;

        XRDirectInteractor interactor = controllerObject.AddComponent<XRDirectInteractor>();
        interactor.interactionManager = manager;
    }
}
