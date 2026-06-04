using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class PeripheralStateLogger : MonoBehaviour
{
    [Header("References")]
    public PeripheralStateDetector detector;
    public PeripheralTrialController trialController;
    public PeripheralCueModel cueModel;
    public PeripheralCueAudioEmitter audioEmitter;
    public PeripheralCueExperimentController experimentController;

    [Header("Experiment")]
    public string participantId = "P001";
    public string conditionLabel = "demo";
    public string trialId = "T001";

    [Header("CSV")]
    public string fileName = "peripheral_state_log.csv";
    public bool includeExperimentMetadataInFileName = true;
    public bool appendTimestampToFileName = true;
    public float logInterval = 0.1f;
    public bool logNoneState = true;
    public bool flushEachWrite = true;

    private StreamWriter writer;
    private float timer;
    private string filePath;
    private bool warnedMissingDetector;
    private bool warnedMissingUserHead;

    public string FilePath
    {
        get { return filePath; }
    }

    private void Awake()
    {
        if (detector == null)
            detector = GetComponent<PeripheralStateDetector>();

        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();

        if (cueModel == null)
            cueModel = GetComponent<PeripheralCueModel>();

        if (audioEmitter == null)
            audioEmitter = GetComponent<PeripheralCueAudioEmitter>();

        if (experimentController == null)
            experimentController = GetComponent<PeripheralCueExperimentController>();
    }

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, BuildFileName());
        writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("participantId,conditionLabel,trialId,cueCondition,cueCandidate,roomScale,materialClass,environmentReverbAmount,environmentOcclusionStrength,environmentDistanceAttenuation,environmentRt60,environmentDrr,time,trialElapsed,trialDuration,targetId,state,outOfView,approaching,speaking,gazing,near,crossing,distance,viewAngle,radialSpeed,lateralSpeed,localX,localY,localZ,expectedDirection,cueType,presenceScore,volumeGain,cueLowPassHz,cueReverbAmount,cueOcclusionGain,responseGiven,reactionTime,responseKey,directionResponse,directionCorrect,subjectiveRating,playbackCue,playbackActive,playbackVolume,playbackLowPassHz,playbackReverbAmount,footstepInterval");
        writer.Flush();

        Debug.Log("Peripheral CSV created: " + filePath);
    }

    private void Update()
    {
        if (writer == null) return;
        if (trialController != null && !trialController.IsRunning) return;

        if (detector == null)
        {
            if (!warnedMissingDetector)
            {
                Debug.LogWarning("PeripheralStateLogger has no detector assigned.", this);
                warnedMissingDetector = true;
            }

            return;
        }

        if (detector.userHead == null)
        {
            if (!warnedMissingUserHead)
            {
                Debug.LogWarning("PeripheralStateDetector.userHead is empty. Assign Main Camera or XR Origin/Main Camera.", detector);
                warnedMissingUserHead = true;
            }

            return;
        }

        timer += Time.deltaTime;
        if (timer < logInterval) return;

        timer = 0f;

        IReadOnlyList<PeripheralDetectionResult> results = detector.LatestResults;
        for (int i = 0; i < results.Count; i++)
        {
            WriteResult(results[i]);
        }

        if (flushEachWrite)
            writer.Flush();
    }

    private string BuildFileName()
    {
        if (!includeExperimentMetadataInFileName && !appendTimestampToFileName)
            return fileName;

        string directory = Path.GetDirectoryName(fileName);
        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        string suffix = BuildFileNameSuffix();
        string stampedFileName = string.IsNullOrEmpty(suffix) ? name + extension : name + "_" + suffix + extension;

        if (string.IsNullOrEmpty(directory))
            return stampedFileName;

        return Path.Combine(directory, stampedFileName);
    }

    private string BuildFileNameSuffix()
    {
        List<string> parts = new List<string>();

        if (includeExperimentMetadataInFileName)
        {
            AddFileNamePart(parts, participantId);
            AddFileNamePart(parts, conditionLabel);
            AddFileNamePart(parts, trialId);
        }

        if (appendTimestampToFileName)
            parts.Add(System.DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));

        return string.Join("_", parts);
    }

    private static void AddFileNamePart(List<string> parts, string value)
    {
        string sanitized = SanitizeFileNamePart(value);
        if (!string.IsNullOrEmpty(sanitized))
            parts.Add(sanitized);
    }

    private static string SanitizeFileNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        char[] invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            bool invalid = false;
            for (int j = 0; j < invalidChars.Length; j++)
            {
                if (c == invalidChars[j])
                {
                    invalid = true;
                    break;
                }
            }

            builder.Append(invalid || char.IsWhiteSpace(c) ? '_' : c);
        }

        return builder.ToString().Trim('_');
    }

    private void WriteResult(PeripheralDetectionResult result)
    {
        if (!logNoneState && result.state == PeripheralState.None)
            return;

        PeripheralCuePrediction cue = cueModel != null ? cueModel.Predict(result) : new PeripheralCuePrediction();
        PeripheralCuePlaybackState playback = audioEmitter != null ? audioEmitter.GetPlaybackState(result.targetId) : new PeripheralCuePlaybackState();
        string expectedDirection = GetExpectedDirection(result);
        string directionResponse = GetDirectionResponse();

        string line = string.Join(",",
            Escape(participantId),
            Escape(conditionLabel),
            Escape(trialId),
            Escape(GetCueConditionLabel()),
            Escape(GetCueCandidateLabel()),
            GetRoomScale().ToString("F3", CultureInfo.InvariantCulture),
            Escape(GetMaterialClassLabel()),
            GetEnvironmentReverbAmount().ToString("F3", CultureInfo.InvariantCulture),
            GetEnvironmentOcclusionStrength().ToString("F3", CultureInfo.InvariantCulture),
            GetEnvironmentDistanceAttenuation().ToString("F3", CultureInfo.InvariantCulture),
            GetEnvironmentRt60().ToString("F3", CultureInfo.InvariantCulture),
            GetEnvironmentDrr().ToString("F3", CultureInfo.InvariantCulture),
            Time.time.ToString("F3", CultureInfo.InvariantCulture),
            GetTrialElapsed().ToString("F3", CultureInfo.InvariantCulture),
            GetTrialDuration().ToString("F3", CultureInfo.InvariantCulture),
            Escape(result.targetId),
            Escape(result.state.ToString()),
            HasState(result.state, PeripheralState.OutOfView),
            HasState(result.state, PeripheralState.Approaching),
            HasState(result.state, PeripheralState.Speaking),
            HasState(result.state, PeripheralState.Gazing),
            HasState(result.state, PeripheralState.Near),
            HasState(result.state, PeripheralState.Crossing),
            result.distance.ToString("F3", CultureInfo.InvariantCulture),
            result.viewAngle.ToString("F2", CultureInfo.InvariantCulture),
            result.radialSpeed.ToString("F3", CultureInfo.InvariantCulture),
            result.lateralSpeed.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.x.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.y.ToString("F3", CultureInfo.InvariantCulture),
            result.userLocalPosition.z.ToString("F3", CultureInfo.InvariantCulture),
            Escape(expectedDirection),
            cue.cueType.ToString(),
            cue.presenceScore.ToString("F3", CultureInfo.InvariantCulture),
            cue.volumeGain.ToString("F3", CultureInfo.InvariantCulture),
            cue.lowPassHz.ToString("F0", CultureInfo.InvariantCulture),
            cue.reverbAmount.ToString("F3", CultureInfo.InvariantCulture),
            cue.occlusionGain.ToString("F3", CultureInfo.InvariantCulture),
            GetResponseGiven(),
            FormatOptionalTime(GetReactionTime()),
            Escape(GetResponseKey()),
            Escape(directionResponse),
            FormatOptionalBool(GetDirectionCorrect(directionResponse, expectedDirection)),
            GetSubjectiveRating().ToString(CultureInfo.InvariantCulture),
            Escape(playback.cueCandidate.ToString()),
            playback.playbackActive,
            playback.outputVolume.ToString("F3", CultureInfo.InvariantCulture),
            playback.lowPassHz.ToString("F0", CultureInfo.InvariantCulture),
            playback.reverbAmount.ToString("F3", CultureInfo.InvariantCulture),
            playback.footstepInterval.ToString("F3", CultureInfo.InvariantCulture)
        );

        writer.WriteLine(line);
    }

    private string GetCueConditionLabel()
    {
        return cueModel != null ? cueModel.comparisonCondition.ToString() : string.Empty;
    }

    private string GetCueCandidateLabel()
    {
        return experimentController != null ? experimentController.CueCandidateLabel : string.Empty;
    }

    private EnvironmentAcousticProfile GetEnvironmentProfile()
    {
        return cueModel != null ? cueModel.environmentProfile : null;
    }

    private float GetRoomScale()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.roomScale : 1f;
    }

    private string GetMaterialClassLabel()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.materialClass.ToString() : string.Empty;
    }

    private float GetEnvironmentReverbAmount()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.reverbAmount : 0f;
    }

    private float GetEnvironmentOcclusionStrength()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.occlusionStrength : 0f;
    }

    private float GetEnvironmentDistanceAttenuation()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.distanceAttenuation : 0f;
    }

    private float GetEnvironmentRt60()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.rt60 : 0f;
    }

    private float GetEnvironmentDrr()
    {
        EnvironmentAcousticProfile profile = GetEnvironmentProfile();
        return profile != null ? profile.drr : 0f;
    }

    private float GetTrialElapsed()
    {
        return trialController != null ? trialController.ElapsedSeconds : Time.time;
    }

    private float GetTrialDuration()
    {
        return trialController != null ? trialController.trialDurationSeconds : 0f;
    }

    private bool GetResponseGiven()
    {
        return experimentController != null && experimentController.ResponseGiven;
    }

    private float GetReactionTime()
    {
        return experimentController != null ? experimentController.ReactionTime : -1f;
    }

    private string GetResponseKey()
    {
        return experimentController != null ? experimentController.ResponseKey : string.Empty;
    }

    private string GetDirectionResponse()
    {
        return experimentController != null ? experimentController.DirectionResponse : string.Empty;
    }

    private static string GetExpectedDirection(PeripheralDetectionResult result)
    {
        Vector3 local = result.userLocalPosition;
        if (Mathf.Abs(local.x) > Mathf.Abs(local.z))
            return local.x < 0f ? "Left" : "Right";

        return local.z < 0f ? "Rear" : "Front";
    }

    private static bool? GetDirectionCorrect(string response, string expected)
    {
        if (string.IsNullOrEmpty(response) || string.IsNullOrEmpty(expected))
            return null;

        return string.Equals(response, expected, System.StringComparison.OrdinalIgnoreCase);
    }

    private int GetSubjectiveRating()
    {
        return experimentController != null ? experimentController.SubjectiveRating : 0;
    }

    private static string FormatOptionalTime(float value)
    {
        return value >= 0f ? value.ToString("F3", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string FormatOptionalBool(bool? value)
    {
        return value.HasValue ? value.Value.ToString() : string.Empty;
    }

    private static bool HasState(PeripheralState value, PeripheralState state)
    {
        return (value & state) != 0;
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    private void OnDestroy()
    {
        CloseWriter();
    }

    private void OnApplicationQuit()
    {
        CloseWriter();
    }

    private void CloseWriter()
    {
        if (writer == null) return;

        writer.Flush();
        writer.Close();
        writer = null;
    }
}
