using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public sealed class QuestGrabTaskObject : MonoBehaviour
{
    private InteractiveObjectScenario owner;
    private XRGrabInteractable grab;

    public void Configure(InteractiveObjectScenario scenario)
    {
        owner = scenario;
    }

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.throwOnDetach = false;
    }

    private void OnEnable()
    {
        if (grab == null) grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (owner != null && owner.CanInteract) owner.NotifyGrab(transform.position);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (owner != null && owner.CanInteract) owner.NotifyRelease(transform.position);
    }
}
