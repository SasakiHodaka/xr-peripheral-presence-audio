using UnityEngine;

public enum PresenceRole
{
    Neutral,
    Teammate,
    Enemy,
    Leader,
    Support
}

public class PresenceTarget : MonoBehaviour
{
    [Header("Transforms")]
    public Transform headTransform;
    public Transform bodyTransform;

    [Header("Role")]
    public PresenceRole role = PresenceRole.Neutral;

    [Header("State")]
    public bool isSpeaking = false;
    public bool isMoving = false;

    [HideInInspector] public Vector3 previousPosition;
    [HideInInspector] public Vector3 velocity;

    private void Start()
    {
        if (bodyTransform == null) bodyTransform = transform;
        if (headTransform == null) headTransform = transform;
        previousPosition = bodyTransform.position;
    }

    private void Update()
    {
        Vector3 currentPosition = bodyTransform.position;
        velocity = (currentPosition - previousPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        isMoving = velocity.magnitude > 0.05f;
        previousPosition = currentPosition;
    }

    public float GetRoleWeight()
    {
        switch (role)
        {
            case PresenceRole.Teammate: return 0.8f;
            case PresenceRole.Enemy:    return 1.2f;
            case PresenceRole.Leader:   return 1.1f;
            case PresenceRole.Support:  return 0.9f;
            default:                    return 0.6f;
        }
    }
}
