using System;
using UnityEngine;

public enum PeripheralCueType
{
    None,
    Footstep,
    Voice,
    AmbientPresence
}

public enum PeripheralCueComparisonCondition
{
    NoCue,
    FixedCue,
    StateBasedCue,
    LearnedCue,
    EnvironmentAdaptiveCue
}

[Serializable]
public struct PeripheralCuePrediction
{
    public PeripheralCueType cueType;
    public float presenceScore;
    public float volumeGain;
    public float lowPassHz;
    public float reverbAmount;
    public float occlusionGain;
}

public class PeripheralCueModel : MonoBehaviour
{
    [Header("Comparison Condition")]
    public PeripheralCueComparisonCondition comparisonCondition = PeripheralCueComparisonCondition.EnvironmentAdaptiveCue;
    [Range(0f, 1f)] public float fixedPresenceScore = 0.45f;
    [Range(0f, 1f)] public float fixedVolumeGain = 0.45f;

    [Header("Distance")]
    public float nearDistance = 1.0f;
    public float farDistance = 5.0f;

    [Header("Environment")]
    public EnvironmentAcousticProfile environmentProfile;

    [Header("Learned Model")]
    public TextAsset learnedModelJson;
    public string learnedConditionLabel = "demo";
    public string learnedFeatureCueCondition = "StateBasedCue";

    [Header("Weights")]
    [Range(0f, 1f)] public float outOfViewBoost = 0.25f;
    [Range(0f, 1f)] public float approachingBoost = 0.35f;
    [Range(0f, 1f)] public float speakingBoost = 0.35f;
    [Range(0f, 1f)] public float crossingBoost = 0.2f;

    private PeripheralCueLearnedModel learnedModel;
    private TextAsset loadedLearnedModelJson;

    public PeripheralCuePrediction Predict(PeripheralDetectionResult result)
    {
        if (comparisonCondition == PeripheralCueComparisonCondition.NoCue)
            return EmptyPrediction();

        bool outOfView = HasState(result.state, PeripheralState.OutOfView);
        bool approaching = HasState(result.state, PeripheralState.Approaching);
        bool speaking = HasState(result.state, PeripheralState.Speaking);
        bool crossing = HasState(result.state, PeripheralState.Crossing);
        bool near = HasState(result.state, PeripheralState.Near);

        float nearFactor = 1f - Mathf.InverseLerp(nearDistance, farDistance, result.distance);
        float score = Mathf.Clamp01(nearFactor);

        if (outOfView) score += outOfViewBoost;
        if (approaching) score += approachingBoost;
        if (speaking) score += speakingBoost;
        if (crossing) score += crossingBoost;
        if (near) score += 0.15f;

        score = Mathf.Clamp01(score);

        if (comparisonCondition == PeripheralCueComparisonCondition.LearnedCue)
        {
            PeripheralCuePrediction learnedPrediction;
            if (TryPredictLearned(result, outOfView, approaching, speaking, crossing, near, out learnedPrediction))
                return learnedPrediction;
        }

        PeripheralCuePrediction prediction = new PeripheralCuePrediction();
        prediction.cueType = SelectCueType(speaking, approaching, crossing, outOfView, near, score);
        prediction.presenceScore = prediction.cueType == PeripheralCueType.None ? 0f : score;

        if (comparisonCondition == PeripheralCueComparisonCondition.FixedCue)
        {
            prediction.presenceScore = prediction.cueType == PeripheralCueType.None ? 0f : fixedPresenceScore;
            prediction.volumeGain = prediction.cueType == PeripheralCueType.None ? 0f : fixedVolumeGain;
            prediction.lowPassHz = 22000f;
            prediction.reverbAmount = 0f;
            prediction.occlusionGain = 1f;
            return prediction;
        }

        EnvironmentAcousticSample acousticSample = GetAcousticSample(result);
        prediction.volumeGain = prediction.cueType == PeripheralCueType.None ? 0f : Mathf.Clamp01(score * acousticSample.distanceAttenuation * acousticSample.occlusionGain);
        prediction.lowPassHz = acousticSample.lowPassHz;
        prediction.reverbAmount = prediction.cueType == PeripheralCueType.None ? 0f : acousticSample.reverbAmount;
        prediction.occlusionGain = acousticSample.occlusionGain;
        return prediction;
    }

    private EnvironmentAcousticSample GetAcousticSample(PeripheralDetectionResult result)
    {
        if (comparisonCondition == PeripheralCueComparisonCondition.EnvironmentAdaptiveCue && environmentProfile != null)
            return environmentProfile.Evaluate(result, nearDistance, farDistance);

        EnvironmentAcousticSample sample = new EnvironmentAcousticSample();
        sample.reverbAmount = 0f;
        sample.occlusionGain = 1f;
        sample.lowPassHz = 22000f;
        sample.distanceAttenuation = 1f;
        return sample;
    }

    private static PeripheralCuePrediction EmptyPrediction()
    {
        PeripheralCuePrediction prediction = new PeripheralCuePrediction();
        prediction.cueType = PeripheralCueType.None;
        prediction.lowPassHz = 22000f;
        prediction.occlusionGain = 1f;
        return prediction;
    }

    private static PeripheralCueType SelectCueType(
        bool speaking,
        bool approaching,
        bool crossing,
        bool outOfView,
        bool near,
        float score)
    {
        if (score <= 0.05f)
            return PeripheralCueType.None;

        if (speaking)
            return PeripheralCueType.Voice;

        if (approaching || crossing)
            return PeripheralCueType.Footstep;

        if (outOfView || near)
            return PeripheralCueType.AmbientPresence;

        return PeripheralCueType.None;
    }

    private static bool HasState(PeripheralState value, PeripheralState state)
    {
        return (value & state) != 0;
    }

    private bool TryPredictLearned(
        PeripheralDetectionResult result,
        bool outOfView,
        bool approaching,
        bool speaking,
        bool crossing,
        bool near,
        out PeripheralCuePrediction prediction)
    {
        prediction = new PeripheralCuePrediction();
        if (!EnsureLearnedModelLoaded())
            return false;

        PeripheralCueFeatureContext context = new PeripheralCueFeatureContext();
        context.conditionLabel = learnedConditionLabel;
        context.cueCondition = learnedFeatureCueCondition;
        context.materialClass = environmentProfile != null ? environmentProfile.materialClass.ToString() : AcousticMaterialClass.Neutral.ToString();
        context.targetId = result.targetId;
        context.roomScale = environmentProfile != null ? environmentProfile.roomScale : 1f;
        context.environmentReverbAmount = environmentProfile != null ? environmentProfile.reverbAmount : 0f;
        context.environmentOcclusionStrength = environmentProfile != null ? environmentProfile.occlusionStrength : 0f;
        context.environmentDistanceAttenuation = environmentProfile != null ? environmentProfile.distanceAttenuation : 0f;
        context.environmentRt60 = environmentProfile != null ? environmentProfile.rt60 : 0f;
        context.environmentDrr = environmentProfile != null ? environmentProfile.drr : 0f;
        context.outOfView = outOfView;
        context.approaching = approaching;
        context.speaking = speaking;
        context.gazing = HasState(result.state, PeripheralState.Gazing);
        context.near = near;
        context.crossing = crossing;
        context.distance = result.distance;
        context.viewAngle = result.viewAngle;
        context.radialSpeed = result.radialSpeed;
        context.lateralSpeed = result.lateralSpeed;
        context.localX = result.userLocalPosition.x;
        context.localY = result.userLocalPosition.y;
        context.localZ = result.userLocalPosition.z;

        prediction = learnedModel.Predict(context);
        return true;
    }

    private bool EnsureLearnedModelLoaded()
    {
        if (learnedModelJson == null)
            return false;

        if (learnedModel != null && loadedLearnedModelJson == learnedModelJson && learnedModel.IsLoaded)
            return true;

        loadedLearnedModelJson = learnedModelJson;
        try
        {
            learnedModel = PeripheralCueLearnedModel.FromJson(learnedModelJson.text);
            return learnedModel != null && learnedModel.IsLoaded;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to load learned peripheral cue model: " + exception.Message, this);
            learnedModel = null;
            return false;
        }
    }
}
