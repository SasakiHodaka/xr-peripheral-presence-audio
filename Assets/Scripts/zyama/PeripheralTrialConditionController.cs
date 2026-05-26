using UnityEngine;

public enum PeripheralTrialCondition
{
    AllDemoTargets,
    Approach,
    BackApproach,
    Crossing,
    Speaking,
    None
}

public class PeripheralTrialConditionController : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralStateLogger logger;

    [Header("Condition")]
    public PeripheralTrialCondition condition = PeripheralTrialCondition.AllDemoTargets;
    public bool applyOnStart = true;
    public bool updateLoggerConditionLabel = true;

    [Header("Targets")]
    public PeripheralTarget approachTarget;
    public PeripheralTarget backApproachTarget;
    public PeripheralTarget crossingTarget;
    public PeripheralTarget speakingTarget;

    private void Awake()
    {
        if (detector == null)
            detector = GetComponent<PeripheralStateDetector>();

        if (logger == null)
            logger = GetComponent<PeripheralStateLogger>();
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyCondition();
    }

    public void ApplyCondition()
    {
        SetTargetActive(approachTarget, ShouldEnable(PeripheralTrialCondition.Approach));
        SetTargetActive(backApproachTarget, ShouldEnable(PeripheralTrialCondition.BackApproach));
        SetTargetActive(crossingTarget, ShouldEnable(PeripheralTrialCondition.Crossing));
        SetTargetActive(speakingTarget, ShouldEnable(PeripheralTrialCondition.Speaking));
        UpdateDetectorTargets();

        if (updateLoggerConditionLabel && logger != null)
            logger.conditionLabel = condition.ToString();
    }

    private bool ShouldEnable(PeripheralTrialCondition targetCondition)
    {
        return condition == PeripheralTrialCondition.AllDemoTargets || condition == targetCondition;
    }

    private void UpdateDetectorTargets()
    {
        if (detector == null) return;

        detector.targets.Clear();
        AddDetectorTarget(approachTarget);
        AddDetectorTarget(backApproachTarget);
        AddDetectorTarget(crossingTarget);
        AddDetectorTarget(speakingTarget);
    }

    private void AddDetectorTarget(PeripheralTarget target)
    {
        if (target == null) return;
        if (!target.gameObject.activeInHierarchy) return;
        if (!detector.targets.Contains(target))
            detector.targets.Add(target);
    }

    private static void SetTargetActive(PeripheralTarget target, bool active)
    {
        if (target != null)
            target.gameObject.SetActive(active);
    }
}
