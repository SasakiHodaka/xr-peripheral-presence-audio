using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public enum PeripheralSimulationViewState
{
    InView,
    Peripheral,
    OutOfView
}

public enum PeripheralSimulationMotionState
{
    Static,
    ApproachingSlow,
    ApproachingFast,
    Leaving,
    Crossing
}

public class PeripheralSimulationDatasetGenerator : MonoBehaviour
{
    [Header("Output")]
    public string fileName = "cue_simulation_dataset.csv";
    public bool writeToPersistentDataPath = true;

    [Header("Simulation Grid")]
    public float[] distances = { 0.75f, 1.5f, 3f, 5f, 7f };
    public float[] angles = { -150f, -90f, -45f, 0f, 45f, 90f, 150f, 180f };
    public PeripheralSimulationMotionState[] motionStates =
    {
        PeripheralSimulationMotionState.Static,
        PeripheralSimulationMotionState.ApproachingSlow,
        PeripheralSimulationMotionState.ApproachingFast,
        PeripheralSimulationMotionState.Leaving,
        PeripheralSimulationMotionState.Crossing
    };
    public bool[] speakingStates = { false, true };
    public bool[] gazingStates = { false, true };

    [Header("Objective Score Weights")]
    [Range(0f, 1f)] public float distanceWeight = 0.35f;
    [Range(0f, 1f)] public float outOfViewWeight = 0.2f;
    [Range(0f, 1f)] public float approachWeight = 0.25f;
    [Range(0f, 1f)] public float speakingWeight = 0.15f;
    [Range(0f, 1f)] public float crossingWeight = 0.15f;
    [Range(0f, 1f)] public float gazingWeight = 0.1f;

    [Header("Acoustic Defaults")]
    public float roomScale = 1f;
    public AcousticMaterialClass materialClass = AcousticMaterialClass.Neutral;
    [Range(0f, 1f)] public float environmentReverbAmount = 0.25f;
    [Range(0f, 1f)] public float environmentOcclusionStrength = 0.35f;
    [Range(0f, 1f)] public float environmentDistanceAttenuation = 0.35f;
    public float environmentRt60 = 0.45f;
    public float environmentDrr = 6f;
    public float clearLowPassHz = 22000f;
    public float occludedLowPassHz = 4500f;
    public float farLowPassHz = 9000f;

    public string LastGeneratedPath { get; private set; }

    [ContextMenu("Generate Simulation Dataset")]
    public void GenerateSimulationDataset()
    {
        string path = ResolveOutputPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            writer.WriteLine(BuildHeader());

            int sampleIndex = 0;
            foreach (float distance in distances)
            {
                foreach (float angle in angles)
                {
                    foreach (PeripheralSimulationMotionState motionState in motionStates)
                    {
                        foreach (bool speaking in speakingStates)
                        {
                            foreach (bool gazing in gazingStates)
                            {
                                SimulationSample sample = BuildSample(sampleIndex, distance, angle, motionState, speaking, gazing);
                                writer.WriteLine(BuildLine(sample));
                                sampleIndex++;
                            }
                        }
                    }
                }
            }
        }

        LastGeneratedPath = path;
        Debug.Log("Peripheral simulation dataset generated: " + path, this);
    }

    private SimulationSample BuildSample(
        int sampleIndex,
        float distance,
        float angle,
        PeripheralSimulationMotionState motionState,
        bool speaking,
        bool gazing)
    {
        Vector3 localPosition = DirectionToLocalPosition(distance, angle);
        PeripheralSimulationViewState viewState = GetViewState(angle);
        bool outOfView = viewState == PeripheralSimulationViewState.OutOfView;
        bool near = distance <= 1.5f;
        bool crossing = motionState == PeripheralSimulationMotionState.Crossing;
        float radialSpeed = GetRadialSpeed(motionState);
        float lateralSpeed = crossing ? Mathf.Sign(localPosition.x == 0f ? 1f : localPosition.x) * 1.2f : 0f;
        bool approaching = radialSpeed > 0f;

        float distanceUrgency = 1f - Mathf.InverseLerp(0.5f, 7.5f, distance);
        float approachUrgency = Mathf.InverseLerp(0f, 1.6f, radialSpeed);
        float objectiveScore =
            distanceWeight * distanceUrgency +
            outOfViewWeight * (outOfView ? 1f : 0f) +
            approachWeight * approachUrgency +
            speakingWeight * (speaking ? 1f : 0f) +
            crossingWeight * (crossing ? 1f : 0f) +
            gazingWeight * (gazing ? 1f : 0f);

        objectiveScore = Mathf.Clamp01(objectiveScore);
        float distance01 = Mathf.InverseLerp(0.5f, 7.5f, distance);
        float occlusion01 = Mathf.Clamp01((outOfView ? 0.55f : 0f) + (localPosition.z < 0f ? 0.45f : 0f));
        float occlusionGain = Mathf.Clamp01(1f - environmentOcclusionStrength * occlusion01);
        float distanceAttenuation = Mathf.Lerp(1f, 1f - environmentDistanceAttenuation, distance01);
        float volumeGain = Mathf.Clamp01(objectiveScore * distanceAttenuation * occlusionGain);
        float lowPassHz = Mathf.Min(
            Mathf.Lerp(clearLowPassHz, farLowPassHz, distance01),
            Mathf.Lerp(clearLowPassHz, occludedLowPassHz, environmentOcclusionStrength * occlusion01));

        SimulationSample sample = new SimulationSample();
        sample.sampleId = "SIM_" + sampleIndex.ToString("D5", CultureInfo.InvariantCulture);
        sample.conditionLabel = "Simulation";
        sample.cueCondition = "SimulationObjective";
        sample.materialClass = materialClass.ToString();
        sample.targetId = "SimTarget";
        sample.roomScale = roomScale;
        sample.environmentReverbAmount = environmentReverbAmount;
        sample.environmentOcclusionStrength = environmentOcclusionStrength;
        sample.environmentDistanceAttenuation = environmentDistanceAttenuation;
        sample.environmentRt60 = environmentRt60;
        sample.environmentDrr = environmentDrr;
        sample.outOfView = outOfView;
        sample.approaching = approaching;
        sample.speaking = speaking;
        sample.gazing = gazing;
        sample.near = near;
        sample.crossing = crossing;
        sample.distance = distance;
        sample.viewAngle = Mathf.Abs(angle);
        sample.radialSpeed = radialSpeed;
        sample.lateralSpeed = lateralSpeed;
        sample.localX = localPosition.x;
        sample.localY = 0f;
        sample.localZ = localPosition.z;
        sample.directionLabel = PeripheralCueModel.SelectDirectionLabel(localPosition).ToString();
        sample.viewState = viewState.ToString();
        sample.motionState = motionState.ToString();
        sample.cueType = SelectObjectiveCueType(speaking, approaching, crossing, outOfView, near, objectiveScore).ToString();
        sample.presenceScore = sample.cueType == PeripheralCueType.None.ToString() ? 0f : objectiveScore;
        sample.volumeGain = sample.cueType == PeripheralCueType.None.ToString() ? 0f : volumeGain;
        sample.cueLowPassHz = lowPassHz;
        sample.cueReverbAmount = sample.cueType == PeripheralCueType.None.ToString() ? 0f : environmentReverbAmount;
        sample.cueOcclusionGain = occlusionGain;
        return sample;
    }

    private string ResolveOutputPath()
    {
        if (Path.IsPathRooted(fileName))
            return fileName;

        string root = writeToPersistentDataPath ? Application.persistentDataPath : Application.dataPath;
        return Path.Combine(root, fileName);
    }

    private static string BuildHeader()
    {
        return "sampleId,conditionLabel,cueCondition,roomScale,materialClass,environmentReverbAmount,environmentOcclusionStrength,environmentDistanceAttenuation,environmentRt60,environmentDrr,targetId,outOfView,approaching,speaking,gazing,near,crossing,distance,viewAngle,radialSpeed,lateralSpeed,localX,localY,localZ,directionLabel,viewState,motionState,cueType,presenceScore,volumeGain,cueLowPassHz,cueReverbAmount,cueOcclusionGain";
    }

    private static string BuildLine(SimulationSample sample)
    {
        List<string> values = new List<string>
        {
            Escape(sample.sampleId),
            Escape(sample.conditionLabel),
            Escape(sample.cueCondition),
            Format(sample.roomScale),
            Escape(sample.materialClass),
            Format(sample.environmentReverbAmount),
            Format(sample.environmentOcclusionStrength),
            Format(sample.environmentDistanceAttenuation),
            Format(sample.environmentRt60),
            Format(sample.environmentDrr),
            Escape(sample.targetId),
            sample.outOfView.ToString(),
            sample.approaching.ToString(),
            sample.speaking.ToString(),
            sample.gazing.ToString(),
            sample.near.ToString(),
            sample.crossing.ToString(),
            Format(sample.distance),
            Format(sample.viewAngle),
            Format(sample.radialSpeed),
            Format(sample.lateralSpeed),
            Format(sample.localX),
            Format(sample.localY),
            Format(sample.localZ),
            Escape(sample.directionLabel),
            Escape(sample.viewState),
            Escape(sample.motionState),
            Escape(sample.cueType),
            Format(sample.presenceScore),
            Format(sample.volumeGain),
            Format(sample.cueLowPassHz),
            Format(sample.cueReverbAmount),
            Format(sample.cueOcclusionGain)
        };

        return string.Join(",", values);
    }

    private static PeripheralCueType SelectObjectiveCueType(
        bool speaking,
        bool approaching,
        bool crossing,
        bool outOfView,
        bool near,
        float objectiveScore)
    {
        if (objectiveScore <= 0.05f)
            return PeripheralCueType.None;

        if (speaking)
            return PeripheralCueType.Voice;

        if (approaching)
            return PeripheralCueType.Footstep;

        if (crossing || outOfView || near)
            return PeripheralCueType.AmbientPresence;

        return PeripheralCueType.None;
    }

    private static PeripheralSimulationViewState GetViewState(float angle)
    {
        float absAngle = Mathf.Abs(Mathf.DeltaAngle(0f, angle));
        if (absAngle <= 50f)
            return PeripheralSimulationViewState.InView;
        if (absAngle <= 85f)
            return PeripheralSimulationViewState.Peripheral;
        return PeripheralSimulationViewState.OutOfView;
    }

    private static float GetRadialSpeed(PeripheralSimulationMotionState motionState)
    {
        switch (motionState)
        {
            case PeripheralSimulationMotionState.ApproachingSlow:
                return 0.45f;
            case PeripheralSimulationMotionState.ApproachingFast:
                return 1.4f;
            case PeripheralSimulationMotionState.Leaving:
                return -0.6f;
            default:
                return 0f;
        }
    }

    private static Vector3 DirectionToLocalPosition(float distance, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians) * distance, 0f, Mathf.Cos(radians) * distance);
    }

    private static string Format(float value)
    {
        return value.ToString("F6", CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    private struct SimulationSample
    {
        public string sampleId;
        public string conditionLabel;
        public string cueCondition;
        public string materialClass;
        public string targetId;
        public string directionLabel;
        public string viewState;
        public string motionState;
        public string cueType;
        public float roomScale;
        public float environmentReverbAmount;
        public float environmentOcclusionStrength;
        public float environmentDistanceAttenuation;
        public float environmentRt60;
        public float environmentDrr;
        public bool outOfView;
        public bool approaching;
        public bool speaking;
        public bool gazing;
        public bool near;
        public bool crossing;
        public float distance;
        public float viewAngle;
        public float radialSpeed;
        public float lateralSpeed;
        public float localX;
        public float localY;
        public float localZ;
        public float presenceScore;
        public float volumeGain;
        public float cueLowPassHz;
        public float cueReverbAmount;
        public float cueOcclusionGain;
    }
}
