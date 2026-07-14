using System;
using UnityEngine;

[Serializable]
public sealed class GroundTruthScenario
{
    public string scenarioId;
    public string description;
    public GroundTruthEvent[] events;
}

[Serializable]
public sealed class GroundTruthEvent
{
    public string eventId;
    public int sequence;
    public float expectedTime;

    public string speakerId;
    public SerializableVector3 speakerPosition;

    public SerializableVector3 listenerPosition;
    public float listenerRotationY;

    public string utteranceId;

    public string objectId;
    public SerializableVector3 objectPosition;

    public string taskState;
    public int priority;
}

[Serializable]
public sealed class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
