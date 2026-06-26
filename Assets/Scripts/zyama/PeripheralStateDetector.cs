using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PeripheralState
{
    None = 0,
    OutOfView = 1 << 0,
    Approaching = 1 << 1,
    Speaking = 1 << 2,
    Gazing = 1 << 3,
    Near = 1 << 4,
    Crossing = 1 << 5
}

[Serializable]
public struct PeripheralDetectionResult
{
    public string targetId;
    public PeripheralTarget target;
    public PeripheralState state;
    public float distance;
    public float viewAngle;
    public float radialSpeed;
    public float lateralSpeed;
    public Vector3 userLocalPosition;
}

public class PeripheralStateDetector : MonoBehaviour
{
    [Header("User")]
    public Transform userHead;

    [Header("Targets")]
    public List<PeripheralTarget> targets = new List<PeripheralTarget>();
    public bool autoFindTargets = true;

    [Header("Thresholds")]
    [Range(1f, 180f)] public float fieldOfViewAngle = 100f;
    public float nearDistance = 1.5f;
    public float approachingSpeedThreshold = 0.25f;
    [Range(1f, 90f)] public float gazeAngleThreshold = 20f;
    public float crossingForwardDistance = 2.5f;
    public float crossingSideLimit = 2.0f;
    public float crossingLateralSpeedThreshold = 0.45f;

    [Header("Smoothing")]
    public float crossingHoldSeconds = 0.25f;

    private readonly Dictionary<PeripheralTarget, float> previousDistances = new Dictionary<PeripheralTarget, float>();
    private readonly Dictionary<PeripheralTarget, Vector3> previousLocalPositions = new Dictionary<PeripheralTarget, Vector3>();
    private readonly Dictionary<PeripheralTarget, float> crossingTimers = new Dictionary<PeripheralTarget, float>();
    private readonly List<PeripheralDetectionResult> latestResults = new List<PeripheralDetectionResult>();

    public event Action<PeripheralDetectionResult> StateDetected;

    public IReadOnlyList<PeripheralDetectionResult> LatestResults
    {
        get { return latestResults; }
    }

    private void Awake()
    {
        if (autoFindTargets && targets.Count == 0)
        {
            targets.AddRange(FindObjectsOfType<PeripheralTarget>());
        }
    }

    private void Update()
    {
        if (userHead == null) return;

        latestResults.Clear();

        for (int i = 0; i < targets.Count; i++)
        {
            PeripheralTarget target = targets[i];
            if (target == null) continue;

            PeripheralDetectionResult result = Detect(target);
            latestResults.Add(result);
            StateDetected?.Invoke(result);
        }
    }

    private PeripheralDetectionResult Detect(PeripheralTarget target)
    {
        Vector3 toTarget = target.Position - userHead.position;
        float distance = toTarget.magnitude;
        Vector3 direction = distance > 0.0001f ? toTarget / distance : userHead.forward;
        float viewAngle = Vector3.Angle(userHead.forward, direction);
        Vector3 localPosition = userHead.InverseTransformPoint(target.Position);

        float radialSpeed = CalculateRadialSpeed(target, distance);
        float lateralSpeed = CalculateLateralSpeed(target, localPosition);

        PeripheralState state = PeripheralState.None;

        if (viewAngle > fieldOfViewAngle * 0.5f)
            state |= PeripheralState.OutOfView;

        if (radialSpeed >= approachingSpeedThreshold)
            state |= PeripheralState.Approaching;

        if (target.isSpeaking)
            state |= PeripheralState.Speaking;

        if (IsGazingAtUser(target))
            state |= PeripheralState.Gazing;

        if (distance <= nearDistance)
            state |= PeripheralState.Near;

        if (IsCrossing(target, localPosition, lateralSpeed))
            state |= PeripheralState.Crossing;

        previousDistances[target] = distance;
        previousLocalPositions[target] = localPosition;

        PeripheralDetectionResult result = new PeripheralDetectionResult();
        result.targetId = target.targetId;
        result.target = target;
        result.state = state;
        result.distance = distance;
        result.viewAngle = viewAngle;
        result.radialSpeed = radialSpeed;
        result.lateralSpeed = lateralSpeed;
        result.userLocalPosition = localPosition;
        return result;
    }

    private float CalculateRadialSpeed(PeripheralTarget target, float currentDistance)
    {
        if (!previousDistances.TryGetValue(target, out float previousDistance))
            return 0f;

        return (previousDistance - currentDistance) / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private float CalculateLateralSpeed(PeripheralTarget target, Vector3 currentLocalPosition)
    {
        if (!previousLocalPositions.TryGetValue(target, out Vector3 previousLocalPosition))
            return 0f;

        return (currentLocalPosition.x - previousLocalPosition.x) / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private bool IsGazingAtUser(PeripheralTarget target)
    {
        Vector3 toUser = userHead.position - target.Position;
        if (toUser.sqrMagnitude < 0.0001f) return false;

        float gazeAngle = Vector3.Angle(target.Forward, toUser.normalized);
        return gazeAngle <= gazeAngleThreshold;
    }

    private bool IsCrossing(PeripheralTarget target, Vector3 localPosition, float lateralSpeed)
    {
        bool inFrontCorridor =
            localPosition.z > 0f &&
            localPosition.z <= crossingForwardDistance &&
            Mathf.Abs(localPosition.x) <= crossingSideLimit;

        bool crossingNow = false;

        if (inFrontCorridor && Mathf.Abs(lateralSpeed) >= crossingLateralSpeedThreshold)
        {
            if (previousLocalPositions.TryGetValue(target, out Vector3 previousLocalPosition))
            {
                bool crossedCenter = Mathf.Sign(localPosition.x) != Mathf.Sign(previousLocalPosition.x);
                bool lateralDominant = Mathf.Abs(localPosition.x - previousLocalPosition.x) >
                                       Mathf.Abs(localPosition.z - previousLocalPosition.z);
                crossingNow = crossedCenter || lateralDominant;
            }
        }

        float timer = 0f;
        crossingTimers.TryGetValue(target, out timer);

        if (crossingNow)
            timer = crossingHoldSeconds;
        else
            timer = Mathf.Max(0f, timer - Time.deltaTime);

        crossingTimers[target] = timer;
        return timer > 0f;
    }
}
