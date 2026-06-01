using System;
using UnityEngine;

[Serializable]
public struct EnvironmentAcousticPreset
{
    public string label;
    [Range(0.25f, 3f)] public float roomScale;
    public AcousticMaterialClass materialClass;
    [Range(0f, 1f)] public float reverbAmount;
    [Range(0f, 1f)] public float occlusionStrength;
    [Range(0f, 1f)] public float distanceAttenuation;
    public float rt60;
    public float drr;

    public static EnvironmentAcousticPreset Neutral()
    {
        EnvironmentAcousticPreset preset = new EnvironmentAcousticPreset();
        preset.label = "Neutral";
        preset.roomScale = 1f;
        preset.materialClass = AcousticMaterialClass.Neutral;
        preset.reverbAmount = 0.25f;
        preset.occlusionStrength = 0.35f;
        preset.distanceAttenuation = 0.35f;
        preset.rt60 = 0.45f;
        preset.drr = 6f;
        return preset;
    }

    public static EnvironmentAcousticPreset Reverberant()
    {
        EnvironmentAcousticPreset preset = new EnvironmentAcousticPreset();
        preset.label = "Reverberant";
        preset.roomScale = 2.1f;
        preset.materialClass = AcousticMaterialClass.Concrete;
        preset.reverbAmount = 0.7f;
        preset.occlusionStrength = 0.25f;
        preset.distanceAttenuation = 0.2f;
        preset.rt60 = 1.4f;
        preset.drr = 2.5f;
        return preset;
    }

    public static EnvironmentAcousticPreset Occluded()
    {
        EnvironmentAcousticPreset preset = new EnvironmentAcousticPreset();
        preset.label = "Occluded";
        preset.roomScale = 0.8f;
        preset.materialClass = AcousticMaterialClass.Carpet;
        preset.reverbAmount = 0.15f;
        preset.occlusionStrength = 0.8f;
        preset.distanceAttenuation = 0.55f;
        preset.rt60 = 0.25f;
        preset.drr = 8f;
        return preset;
    }
}

[DefaultExecutionOrder(-100)]
public class PeripheralAuiLogCollectionController : MonoBehaviour
{
    [Header("References")]
    public PeripheralCueModel cueModel;
    public EnvironmentAcousticProfile environmentProfile;
    public PeripheralTrialConditionController conditionController;
    public PeripheralTrialController trialController;
    public PeripheralStateLogger logger;

    [Header("Automation")]
    public bool applyOnStart = true;
    public bool autoAdvanceTrials;
    public bool stopWhenComplete;
    public int startTrialIndex;

    [Header("Experiment Conditions")]
    public PeripheralTrialCondition[] targetScenarios = new PeripheralTrialCondition[]
    {
        PeripheralTrialCondition.Approach,
        PeripheralTrialCondition.BackApproach,
        PeripheralTrialCondition.Crossing,
        PeripheralTrialCondition.Speaking
    };

    public PeripheralCueComparisonCondition[] cueConditions = new PeripheralCueComparisonCondition[]
    {
        PeripheralCueComparisonCondition.NoCue,
        PeripheralCueComparisonCondition.FixedCue,
        PeripheralCueComparisonCondition.StateBasedCue,
        PeripheralCueComparisonCondition.EnvironmentAdaptiveCue
    };

    public EnvironmentAcousticPreset[] environmentPresets = new EnvironmentAcousticPreset[]
    {
        EnvironmentAcousticPreset.Neutral(),
        EnvironmentAcousticPreset.Reverberant(),
        EnvironmentAcousticPreset.Occluded()
    };

    private int currentTrialIndex;

    public int CurrentTrialIndex
    {
        get { return currentTrialIndex; }
    }

    public int TotalTrialCount
    {
        get { return Mathf.Max(0, ScenarioCount * CueConditionCount * EnvironmentPresetCount); }
    }

    private int ScenarioCount
    {
        get { return targetScenarios != null ? targetScenarios.Length : 0; }
    }

    private int CueConditionCount
    {
        get { return cueConditions != null ? cueConditions.Length : 0; }
    }

    private int EnvironmentPresetCount
    {
        get { return environmentPresets != null ? environmentPresets.Length : 0; }
    }

    private void Awake()
    {
        if (cueModel == null)
            cueModel = GetComponent<PeripheralCueModel>();

        if (environmentProfile == null)
            environmentProfile = GetComponent<EnvironmentAcousticProfile>();

        if (conditionController == null)
            conditionController = GetComponent<PeripheralTrialConditionController>();

        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();

        if (logger == null)
            logger = GetComponent<PeripheralStateLogger>();
    }

    private void Start()
    {
        currentTrialIndex = Mathf.Clamp(startTrialIndex, 0, Mathf.Max(0, TotalTrialCount - 1));

        if (applyOnStart)
            ApplyCurrentTrial();
    }

    private void Update()
    {
        if (!autoAdvanceTrials || trialController == null || !trialController.IsComplete)
            return;

        AdvanceTrial();
    }

    public void ApplyCurrentTrial()
    {
        if (TotalTrialCount <= 0)
            return;

        DecodeTrialIndex(currentTrialIndex, out int scenarioIndex, out int cueIndex, out int presetIndex);

        PeripheralTrialCondition scenario = targetScenarios[scenarioIndex];
        PeripheralCueComparisonCondition cueCondition = cueConditions[cueIndex];
        EnvironmentAcousticPreset preset = environmentPresets[presetIndex];

        if (conditionController != null)
        {
            conditionController.condition = scenario;
            conditionController.ApplyCondition();
        }

        if (cueModel != null)
            cueModel.comparisonCondition = cueCondition;

        ApplyEnvironmentPreset(preset);
        UpdateLoggerMetadata(scenario, cueCondition, preset);
    }

    public void AdvanceTrial()
    {
        if (TotalTrialCount <= 0)
            return;

        currentTrialIndex++;
        if (currentTrialIndex >= TotalTrialCount)
        {
            currentTrialIndex = TotalTrialCount - 1;
            if (stopWhenComplete)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            return;
        }

        ApplyCurrentTrial();

        if (trialController != null)
            trialController.RestartTrial();
    }

    public string GetCurrentConditionLabel()
    {
        if (TotalTrialCount <= 0)
            return "NoAuiTrials";

        DecodeTrialIndex(currentTrialIndex, out int scenarioIndex, out int cueIndex, out int presetIndex);
        return BuildConditionLabel(targetScenarios[scenarioIndex], cueConditions[cueIndex], environmentPresets[presetIndex]);
    }

    private void ApplyEnvironmentPreset(EnvironmentAcousticPreset preset)
    {
        if (environmentProfile == null)
            return;

        environmentProfile.roomScale = preset.roomScale;
        environmentProfile.materialClass = preset.materialClass;
        environmentProfile.reverbAmount = preset.reverbAmount;
        environmentProfile.occlusionStrength = preset.occlusionStrength;
        environmentProfile.distanceAttenuation = preset.distanceAttenuation;
        environmentProfile.rt60 = preset.rt60;
        environmentProfile.drr = preset.drr;
    }

    private void UpdateLoggerMetadata(
        PeripheralTrialCondition scenario,
        PeripheralCueComparisonCondition cueCondition,
        EnvironmentAcousticPreset preset)
    {
        if (logger == null)
            return;

        logger.conditionLabel = BuildConditionLabel(scenario, cueCondition, preset);
        logger.trialId = "T" + (currentTrialIndex + 1).ToString("000");
    }

    private static string BuildConditionLabel(
        PeripheralTrialCondition scenario,
        PeripheralCueComparisonCondition cueCondition,
        EnvironmentAcousticPreset preset)
    {
        string presetLabel = string.IsNullOrWhiteSpace(preset.label) ? "Env" : preset.label;
        return scenario + "_" + cueCondition + "_" + presetLabel;
    }

    private void DecodeTrialIndex(int trialIndex, out int scenarioIndex, out int cueIndex, out int presetIndex)
    {
        int presetCount = Mathf.Max(1, EnvironmentPresetCount);
        int cueCount = Mathf.Max(1, CueConditionCount);
        int scenarioCount = Mathf.Max(1, ScenarioCount);

        presetIndex = trialIndex % presetCount;
        int remaining = trialIndex / presetCount;
        cueIndex = remaining % cueCount;
        scenarioIndex = (remaining / cueCount) % scenarioCount;
    }
}
