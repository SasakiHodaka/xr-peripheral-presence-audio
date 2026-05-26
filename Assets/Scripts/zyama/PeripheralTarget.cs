using UnityEngine;

public class PeripheralTarget : MonoBehaviour
{
    [Header("Identity")]
    public string targetId = "Target_01";

    [Header("Transforms")]
    public Transform bodyTransform;
    public Transform headTransform;
    public Transform gazeTransform;

    [Header("State")]
    public bool isSpeaking;

    [Header("Optional Existing Components")]
    public PresenceTarget presenceTarget;
    public GroupWorkPresenceAudio presenceAudio;

    public Vector3 Position
    {
        get
        {
            if (headTransform != null) return headTransform.position;
            if (bodyTransform != null) return bodyTransform.position;
            return transform.position;
        }
    }

    public Vector3 Forward
    {
        get
        {
            if (gazeTransform != null) return gazeTransform.forward;
            if (headTransform != null) return headTransform.forward;
            if (bodyTransform != null) return bodyTransform.forward;
            return transform.forward;
        }
    }

    private void Reset()
    {
        bodyTransform = transform;
        headTransform = transform;
        gazeTransform = transform;
        presenceTarget = GetComponent<PresenceTarget>();
        presenceAudio = GetComponent<GroupWorkPresenceAudio>();
    }

    private void Awake()
    {
        if (bodyTransform == null) bodyTransform = transform;
        if (headTransform == null) headTransform = bodyTransform;
        if (gazeTransform == null) gazeTransform = headTransform;
        if (presenceTarget == null) presenceTarget = GetComponent<PresenceTarget>();
        if (presenceAudio == null) presenceAudio = GetComponent<GroupWorkPresenceAudio>();
    }

    private void Update()
    {
        if (presenceTarget != null)
        {
            isSpeaking = presenceTarget.isSpeaking;
        }
        else if (presenceAudio != null)
        {
            isSpeaking = presenceAudio.isSpeaking;
        }
    }

    public void SetSpeaking(bool value)
    {
        isSpeaking = value;

        if (presenceTarget != null)
            presenceTarget.isSpeaking = value;

        if (presenceAudio != null)
            presenceAudio.SetSpeaking(value);
    }
}
