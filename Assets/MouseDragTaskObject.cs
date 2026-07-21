using UnityEngine;

public sealed class MouseDragTaskObject : MonoBehaviour
{
    private InteractiveObjectScenario owner;
    private Camera sceneCamera;
    private Plane dragPlane;
    private Vector3 dragOffset;
    private bool dragging;

    public void Configure(InteractiveObjectScenario scenario, Camera camera)
    {
        owner = scenario;
        sceneCamera = camera;
    }

    private void OnMouseDown()
    {
        if (owner == null || !owner.CanInteract) return;
        if (sceneCamera == null) sceneCamera = Camera.main;
        if (sceneCamera == null) return;

        dragPlane = new Plane(Vector3.up, transform.position);
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter)) return;

        dragOffset = transform.position - ray.GetPoint(enter);
        dragging = true;
        owner.NotifyGrab(transform.position);
    }

    private void OnMouseDrag()
    {
        if (!dragging || sceneCamera == null) return;
        Ray ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
        if (!dragPlane.Raycast(ray, out float enter)) return;
        transform.position = ray.GetPoint(enter) + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;
        owner.NotifyRelease(transform.position);
    }
}
