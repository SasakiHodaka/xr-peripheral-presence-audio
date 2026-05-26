using UnityEngine;

public class PeripheralTrialController : MonoBehaviour
{
    [Header("Trial")]
    public float preTrialSeconds = 3f;
    public float trialDurationSeconds = 30f;
    public bool autoStopEditorPlayMode = false;
    public bool logTrialCompleted = true;

    private float startTime;
    private bool started;
    private bool completed;

    public float ElapsedSeconds
    {
        get { return Mathf.Max(0f, Time.time - startTime - preTrialSeconds); }
    }

    public float PreTrialRemainingSeconds
    {
        get { return Mathf.Max(0f, preTrialSeconds - (Time.time - startTime)); }
    }

    public float RemainingSeconds
    {
        get { return Mathf.Max(0f, trialDurationSeconds - ElapsedSeconds); }
    }

    public bool IsComplete
    {
        get { return completed; }
    }

    public bool IsRunning
    {
        get { return started && !completed; }
    }

    private void OnEnable()
    {
        startTime = Time.time;
        started = preTrialSeconds <= 0f;
        completed = false;
    }

    private void Update()
    {
        if (completed) return;

        if (!started)
        {
            if (PreTrialRemainingSeconds > 0f) return;
            started = true;
        }

        if (trialDurationSeconds <= 0f) return;
        if (ElapsedSeconds < trialDurationSeconds) return;

        completed = true;

        if (logTrialCompleted)
            Debug.Log("Peripheral trial completed after " + trialDurationSeconds.ToString("F1") + " seconds.", this);

#if UNITY_EDITOR
        if (autoStopEditorPlayMode)
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
