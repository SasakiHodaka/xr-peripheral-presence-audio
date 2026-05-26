using UnityEngine;

public enum DemoAvatarMoveMode
{
    Idle,
    ApproachUser,
    CrossInFront,
    CircleUser,
    BackApproach
}

public class DemoAvatarMover : MonoBehaviour
{
    [Header("References")]
    public Transform userHead;
    public PeripheralTarget target;

    [Header("Motion")]
    public DemoAvatarMoveMode moveMode = DemoAvatarMoveMode.Idle;
    public float moveSpeed = 1.0f;
    public float crossWidth = 4.0f;
    public float crossDistance = 1.8f;
    public float circleRadius = 2.5f;
    public float approachResetDistance = 6.0f;
    public float approachStopDistance = 0.9f;
    public bool faceUser = true;

    [Header("Speaking Demo")]
    public bool toggleSpeaking;
    public float speakingInterval = 2.0f;

    private Vector3 startPosition;
    private float elapsed;

    private void Awake()
    {
        startPosition = transform.position;
        if (target == null)
            target = GetComponent<PeripheralTarget>();
    }

    private void Update()
    {
        if (userHead == null) return;

        elapsed += Time.deltaTime;

        switch (moveMode)
        {
            case DemoAvatarMoveMode.ApproachUser:
                MoveTowardUser();
                break;
            case DemoAvatarMoveMode.CrossInFront:
                CrossInFrontOfUser();
                break;
            case DemoAvatarMoveMode.CircleUser:
                CircleAroundUser();
                break;
            case DemoAvatarMoveMode.BackApproach:
                ApproachFromBehind();
                break;
            case DemoAvatarMoveMode.Idle:
                break;
        }

        if (toggleSpeaking && target != null)
            target.SetSpeaking(Mathf.FloorToInt(elapsed / Mathf.Max(speakingInterval, 0.1f)) % 2 == 0);

        if (faceUser)
            LookAtUser();
    }

    private void MoveTowardUser()
    {
        Vector3 toUser = userHead.position - transform.position;
        toUser.y = 0f;

        if (toUser.magnitude <= approachStopDistance)
        {
            transform.position = userHead.position + userHead.forward * approachResetDistance;
            return;
        }

        transform.position += toUser.normalized * moveSpeed * Time.deltaTime;
    }

    private void CrossInFrontOfUser()
    {
        Vector3 center = userHead.position + userHead.forward * crossDistance;
        float x = Mathf.Sin(elapsed * moveSpeed) * crossWidth * 0.5f;
        Vector3 position = center + userHead.right * x;
        position.y = startPosition.y;
        transform.position = position;
    }

    private void CircleAroundUser()
    {
        Vector3 offset =
            userHead.right * (Mathf.Cos(elapsed * moveSpeed) * circleRadius) +
            userHead.forward * (Mathf.Sin(elapsed * moveSpeed) * circleRadius);

        Vector3 position = userHead.position + offset;
        position.y = startPosition.y;
        transform.position = position;
    }

    private void ApproachFromBehind()
    {
        Vector3 targetPosition = userHead.position - userHead.forward * approachStopDistance;
        targetPosition.y = startPosition.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= 0.05f)
        {
            Vector3 resetPosition = userHead.position - userHead.forward * approachResetDistance;
            resetPosition.y = startPosition.y;
            transform.position = resetPosition;
        }
    }

    private void LookAtUser()
    {
        Vector3 direction = userHead.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
