using UnityEngine;

public class PeripheralTrialController : MonoBehaviour
{
    [Header("Trial")]
    public float trialDurationSeconds = 30f;
    public bool autoStopEditorPlayMode = false;
    public bool logTrialCompleted = true;

    private float startTime;
    private bool completed;

    public float ElapsedSeconds
    {
        get { return Mathf.Max(0f, Time.time - startTime); }
    }

    public float RemainingSeconds
    {
        get { return Mathf.Max(0f, trialDurationSeconds - ElapsedSeconds); }
    }

    public bool IsComplete
    {
        get { return completed; }
    }

    private void OnEnable()
    {
        startTime = Time.time;
        completed = false;
    }

    private void Update()
    {
        if (completed) return;
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
