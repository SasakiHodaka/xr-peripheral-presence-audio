using UnityEngine;

public class PeripheralCueTrialSequencer : MonoBehaviour
{
    [Header("References")]
    public PeripheralTrialController trialController;
    public PeripheralTrialConditionController conditionController;
    public PeripheralCueExperimentController experimentController;

    [Header("Sequence")]
    public PeripheralTrialCondition[] conditions =
    {
        PeripheralTrialCondition.BackApproach,
        PeripheralTrialCondition.Approach,
        PeripheralTrialCondition.Crossing,
        PeripheralTrialCondition.Speaking
    };

    public PeripheralCueCandidate[] cueCandidates =
    {
        PeripheralCueCandidate.NoCue,
        PeripheralCueCandidate.Footstep,
        PeripheralCueCandidate.Breathing,
        PeripheralCueCandidate.ClothRustle,
        PeripheralCueCandidate.Voice,
        PeripheralCueCandidate.AmbientPresence,
        PeripheralCueCandidate.MixedCue
    };

    public int conditionIndex;
    public int cueCandidateIndex;
    public bool applyOnStart = true;
    public bool autoAdvanceOnTrialComplete = false;

    [Header("Keyboard")]
    public KeyCode nextTrialKey = KeyCode.N;
    public KeyCode previousTrialKey = KeyCode.B;
    public KeyCode restartTrialKey = KeyCode.R;

    public string CurrentTrialLabel
    {
        get
        {
            return CurrentCondition.ToString() + " / " + CurrentCueCandidate.ToString();
        }
    }

    public PeripheralTrialCondition CurrentCondition
    {
        get
        {
            if (conditions == null || conditions.Length == 0)
                return PeripheralTrialCondition.None;

            return conditions[Mathf.Clamp(conditionIndex, 0, conditions.Length - 1)];
        }
    }

    public PeripheralCueCandidate CurrentCueCandidate
    {
        get
        {
            if (cueCandidates == null || cueCandidates.Length == 0)
                return PeripheralCueCandidate.NoCue;

            return cueCandidates[Mathf.Clamp(cueCandidateIndex, 0, cueCandidates.Length - 1)];
        }
    }

    private void Awake()
    {
        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();

        if (conditionController == null)
            conditionController = GetComponent<PeripheralTrialConditionController>();

        if (experimentController == null)
            experimentController = GetComponent<PeripheralCueExperimentController>();
    }

    private void Start()
    {
        if (applyOnStart)
            ApplyCurrentTrial();
    }

    private void Update()
    {
        if (Input.GetKeyDown(nextTrialKey))
            NextTrial();
        else if (Input.GetKeyDown(previousTrialKey))
            PreviousTrial();
        else if (Input.GetKeyDown(restartTrialKey))
            RestartTrial();

        if (autoAdvanceOnTrialComplete && trialController != null && trialController.IsComplete)
            NextTrial();
    }

    public void NextTrial()
    {
        Step(1);
    }

    public void PreviousTrial()
    {
        Step(-1);
    }

    public void RestartTrial()
    {
        if (experimentController != null)
            experimentController.ResetResponse();

        if (trialController != null)
            trialController.RestartTrial();
    }

    public void ApplyCurrentTrial()
    {
        conditionIndex = ClampIndex(conditionIndex, conditions);
        cueCandidateIndex = ClampIndex(cueCandidateIndex, cueCandidates);

        if (experimentController != null)
            experimentController.cueCandidate = CurrentCueCandidate;

        if (conditionController != null)
        {
            conditionController.condition = CurrentCondition;
            conditionController.ApplyCondition();
        }

        RestartTrial();
    }

    private void Step(int delta)
    {
        int conditionCount = conditions != null ? conditions.Length : 0;
        int cueCount = cueCandidates != null ? cueCandidates.Length : 0;
        if (conditionCount == 0 || cueCount == 0)
            return;

        int flatIndex = conditionIndex * cueCount + cueCandidateIndex + delta;
        int total = conditionCount * cueCount;
        flatIndex = (flatIndex % total + total) % total;

        conditionIndex = flatIndex / cueCount;
        cueCandidateIndex = flatIndex % cueCount;
        ApplyCurrentTrial();
    }

    private static int ClampIndex<T>(int value, T[] array)
    {
        int count = array != null ? array.Length : 0;
        if (count == 0)
            return 0;

        return Mathf.Clamp(value, 0, count - 1);
    }
}
