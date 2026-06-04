using UnityEngine;

public enum PeripheralCueCandidate
{
    NoCue,
    PredictedCue,
    Footstep,
    Breathing,
    ClothRustle,
    Voice,
    AmbientPresence,
    MixedCue
}

public class PeripheralCueExperimentController : MonoBehaviour
{
    [Header("References")]
    public PeripheralTrialController trialController;
    public PeripheralTrialConditionController conditionController;

    [Header("Cue Candidate")]
    public PeripheralCueCandidate cueCandidate = PeripheralCueCandidate.PredictedCue;
    public bool includeCueCandidateInConditionLabel = false;

    [Header("Response Keys")]
    public KeyCode detectionResponseKey = KeyCode.Space;
    public KeyCode leftDirectionKey = KeyCode.LeftArrow;
    public KeyCode rightDirectionKey = KeyCode.RightArrow;
    public KeyCode frontDirectionKey = KeyCode.UpArrow;
    public KeyCode rearDirectionKey = KeyCode.DownArrow;

    [Header("Rating Keys")]
    public KeyCode rating1Key = KeyCode.Alpha1;
    public KeyCode rating2Key = KeyCode.Alpha2;
    public KeyCode rating3Key = KeyCode.Alpha3;
    public KeyCode rating4Key = KeyCode.Alpha4;
    public KeyCode rating5Key = KeyCode.Alpha5;

    private bool wasRunning;
    private bool responseGiven;
    private float reactionTime = -1f;
    private string responseKey = string.Empty;
    private string directionResponse = string.Empty;
    private int subjectiveRating;
    private PeripheralCueCandidate previousCueCandidate;

    public bool ResponseGiven
    {
        get { return responseGiven; }
    }

    public float ReactionTime
    {
        get { return reactionTime; }
    }

    public string ResponseKey
    {
        get { return responseKey; }
    }

    public string DirectionResponse
    {
        get { return directionResponse; }
    }

    public int SubjectiveRating
    {
        get { return subjectiveRating; }
    }

    public string CueCandidateLabel
    {
        get { return cueCandidate.ToString(); }
    }

    private void Awake()
    {
        if (trialController == null)
            trialController = GetComponent<PeripheralTrialController>();

        if (conditionController == null)
            conditionController = GetComponent<PeripheralTrialConditionController>();

        previousCueCandidate = cueCandidate;
    }

    private void Update()
    {
        bool running = trialController != null && trialController.IsRunning;
        if (running && !wasRunning)
            ResetResponse();

        if (cueCandidate != previousCueCandidate)
        {
            previousCueCandidate = cueCandidate;
            ResetResponse();
        }

        wasRunning = running;

        if (!running) return;

        CaptureDetectionResponse();
        CaptureDirectionResponse();
        CaptureRating();
    }

    public string BuildConditionLabel(string baseLabel)
    {
        if (!includeCueCandidateInConditionLabel)
            return baseLabel;

        return string.IsNullOrEmpty(baseLabel) ? CueCandidateLabel : baseLabel + "_" + CueCandidateLabel;
    }

    public void ResetResponse()
    {
        responseGiven = false;
        reactionTime = -1f;
        responseKey = string.Empty;
        directionResponse = string.Empty;
        subjectiveRating = 0;
    }

    private void CaptureDetectionResponse()
    {
        if (responseGiven) return;
        if (!Input.GetKeyDown(detectionResponseKey)) return;

        responseGiven = true;
        reactionTime = trialController != null ? trialController.ElapsedSeconds : Time.time;
        responseKey = detectionResponseKey.ToString();
    }

    private void CaptureDirectionResponse()
    {
        if (Input.GetKeyDown(leftDirectionKey))
            directionResponse = "Left";
        else if (Input.GetKeyDown(rightDirectionKey))
            directionResponse = "Right";
        else if (Input.GetKeyDown(frontDirectionKey))
            directionResponse = "Front";
        else if (Input.GetKeyDown(rearDirectionKey))
            directionResponse = "Rear";
    }

    private void CaptureRating()
    {
        if (Input.GetKeyDown(rating1Key))
            subjectiveRating = 1;
        else if (Input.GetKeyDown(rating2Key))
            subjectiveRating = 2;
        else if (Input.GetKeyDown(rating3Key))
            subjectiveRating = 3;
        else if (Input.GetKeyDown(rating4Key))
            subjectiveRating = 4;
        else if (Input.GetKeyDown(rating5Key))
            subjectiveRating = 5;
    }
}
