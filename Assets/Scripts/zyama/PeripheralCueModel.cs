using System;
using UnityEngine;

public enum PeripheralCueType
{
    None,
    Footstep,
    Voice,
    AmbientPresence
}

[Serializable]
public struct PeripheralCuePrediction
{
    public PeripheralCueType cueType;
    public float presenceScore;
    public float volumeGain;
}

public class PeripheralCueModel : MonoBehaviour
{
    [Header("Distance")]
    public float nearDistance = 1.0f;
    public float farDistance = 5.0f;

    [Header("Weights")]
    [Range(0f, 1f)] public float outOfViewBoost = 0.25f;
    [Range(0f, 1f)] public float approachingBoost = 0.35f;
    [Range(0f, 1f)] public float speakingBoost = 0.35f;
    [Range(0f, 1f)] public float crossingBoost = 0.2f;

    public PeripheralCuePrediction Predict(PeripheralDetectionResult result)
    {
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

        PeripheralCuePrediction prediction = new PeripheralCuePrediction();
        prediction.cueType = SelectCueType(speaking, approaching, crossing, outOfView, near, score);
        prediction.presenceScore = score;
        prediction.volumeGain = prediction.cueType == PeripheralCueType.None ? 0f : Mathf.Clamp01(score);
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
}
