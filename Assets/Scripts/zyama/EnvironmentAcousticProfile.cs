using System;
using UnityEngine;

public enum AcousticMaterialClass
{
    Neutral,
    Carpet,
    Concrete,
    Glass,
    Wood
}

[Serializable]
public struct EnvironmentAcousticSample
{
    public float reverbAmount;
    public float occlusionGain;
    public float lowPassHz;
    public float distanceAttenuation;
}

public class EnvironmentAcousticProfile : MonoBehaviour
{
    [Header("Room")]
    [Range(0.25f, 3f)] public float roomScale = 1f;
    public AcousticMaterialClass materialClass = AcousticMaterialClass.Neutral;

    [Header("Acoustics")]
    [Range(0f, 1f)] public float reverbAmount = 0.25f;
    [Range(0f, 1f)] public float occlusionStrength = 0.35f;
    [Range(0f, 1f)] public float distanceAttenuation = 0.35f;
    public float rt60 = 0.45f;
    public float drr = 6f;

    [Header("Filtering")]
    public float clearLowPassHz = 22000f;
    public float occludedLowPassHz = 4500f;
    public float farLowPassHz = 9000f;

    public EnvironmentAcousticSample Evaluate(PeripheralDetectionResult result, float nearDistance, float farDistance)
    {
        bool outOfView = HasState(result.state, PeripheralState.OutOfView);
        bool rear = result.userLocalPosition.z < 0f;
        float distance01 = Mathf.InverseLerp(nearDistance, farDistance, result.distance);
        float occlusion01 = Mathf.Clamp01((outOfView ? 0.55f : 0f) + (rear ? 0.45f : 0f));
        float materialReverb = MaterialReverbBias(materialClass);

        EnvironmentAcousticSample sample = new EnvironmentAcousticSample();
        sample.distanceAttenuation = Mathf.Lerp(1f, 1f - distanceAttenuation, distance01);
        sample.occlusionGain = Mathf.Clamp01(1f - occlusionStrength * occlusion01);
        sample.reverbAmount = Mathf.Clamp01(reverbAmount + materialReverb + Mathf.InverseLerp(0.2f, 1.8f, rt60) * 0.25f);

        float distanceCutoff = Mathf.Lerp(clearLowPassHz, farLowPassHz, distance01);
        float occludedCutoff = Mathf.Lerp(clearLowPassHz, occludedLowPassHz, occlusionStrength * occlusion01);
        sample.lowPassHz = Mathf.Min(distanceCutoff, occludedCutoff);

        return sample;
    }

    private static float MaterialReverbBias(AcousticMaterialClass value)
    {
        switch (value)
        {
            case AcousticMaterialClass.Carpet:
                return -0.12f;
            case AcousticMaterialClass.Concrete:
                return 0.18f;
            case AcousticMaterialClass.Glass:
                return 0.22f;
            case AcousticMaterialClass.Wood:
                return 0.06f;
            default:
                return 0f;
        }
    }

    private static bool HasState(PeripheralState value, PeripheralState state)
    {
        return (value & state) != 0;
    }
}
