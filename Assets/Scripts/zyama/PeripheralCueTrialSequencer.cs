using System.Collections.Generic;
using UnityEngine;

public class PeripheralCueTrialSequencer : MonoBehaviour
{
    [Header("References")]
    public PeripheralTrialController trialController;
    public PeripheralTrialConditionController conditionController;
    public PeripheralCueExperimentController experimentController;
    public PeripheralStateLogger logger;

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
    public int repeatIndex;
    [Min(1)] public int repeatsPerCombination = 1;
    public bool randomizeOrder = false;
    public int randomSeed = 7;
    public bool applyOnStart = true;
    public bool autoAdvanceOnTrialComplete = false;
    public bool updateLoggerMetadata = true;

    [Header("Keyboard")]
    public KeyCode nextTrialKey = KeyCode.N;
    public KeyCode previousTrialKey = KeyCode.B;
    public KeyCode restartTrialKey = KeyCode.R;

    private readonly List<int> trialOrder = new List<int>();
    private int sequenceIndex;
    private bool orderDirty = true;

    public int CurrentSequenceIndex
    {
        get { return sequenceIndex; }
    }

    public int TotalTrialCount
    {
        get { return GetCombinationCount() * Mathf.Max(1, repeatsPerCombination); }
    }

    public string CurrentTrialLabel
    {
        get
        {
            return CurrentCondition + " / " + CurrentCueCandidate + " / R" + (repeatIndex + 1).ToString("00");
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

        if (logger == null)
            logger = GetComponent<PeripheralStateLogger>();
    }

    private void Start()
    {
        RebuildOrder();

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
        if (orderDirty || trialOrder.Count != TotalTrialCount)
            RebuildOrder();

        ApplySequenceIndex(sequenceIndex);

        if (experimentController != null)
            experimentController.cueCandidate = CurrentCueCandidate;

        if (conditionController != null)
        {
            conditionController.condition = CurrentCondition;
            conditionController.ApplyCondition();
        }

        if (updateLoggerMetadata && logger != null)
        {
            logger.conditionLabel = CurrentCondition + "_" + CurrentCueCandidate;
            logger.trialId = "T" + (sequenceIndex + 1).ToString("000") + "_R" + (repeatIndex + 1).ToString("00");
        }

        RestartTrial();
    }

    private void Step(int delta)
    {
        int total = TotalTrialCount;
        if (total <= 0)
            return;

        sequenceIndex = (sequenceIndex + delta) % total;
        if (sequenceIndex < 0)
            sequenceIndex += total;

        ApplyCurrentTrial();
    }

    public void RebuildOrder()
    {
        trialOrder.Clear();
        int total = TotalTrialCount;
        for (int i = 0; i < total; i++)
            trialOrder.Add(i);

        if (randomizeOrder)
            ShuffleOrder(trialOrder, randomSeed);

        sequenceIndex = Mathf.Clamp(sequenceIndex, 0, Mathf.Max(0, total - 1));
        orderDirty = false;
    }

    private void ApplySequenceIndex(int index)
    {
        int conditionCount = conditions != null ? conditions.Length : 0;
        int cueCount = cueCandidates != null ? cueCandidates.Length : 0;
        int repeatCount = Mathf.Max(1, repeatsPerCombination);
        if (conditionCount == 0 || cueCount == 0)
            return;

        int orderedIndex = trialOrder.Count > 0 ? trialOrder[Mathf.Clamp(index, 0, trialOrder.Count - 1)] : index;
        int combinationCount = conditionCount * cueCount;
        repeatIndex = orderedIndex / combinationCount;
        int combinationIndex = orderedIndex % combinationCount;
        conditionIndex = combinationIndex / cueCount;
        cueCandidateIndex = combinationIndex % cueCount;
        repeatIndex = Mathf.Clamp(repeatIndex, 0, repeatCount - 1);
    }

    private int GetCombinationCount()
    {
        int conditionCount = conditions != null ? conditions.Length : 0;
        int cueCount = cueCandidates != null ? cueCandidates.Length : 0;
        return conditionCount * cueCount;
    }

    private static void ShuffleOrder(List<int> values, int seed)
    {
        System.Random random = new System.Random(seed);
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }

    private static int ClampIndex<T>(int value, T[] array)
    {
        int count = array != null ? array.Length : 0;
        if (count == 0)
            return 0;

        return Mathf.Clamp(value, 0, count - 1);
    }
}
