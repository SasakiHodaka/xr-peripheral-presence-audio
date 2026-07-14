using System;

[Serializable]
public sealed class GeneratedSceneToken
{
    public string scenarioId;
    public string eventId;
    public int sequence;
    public float expectedTime;

    public string speakerId;
    public string direction;
    public string targetObjectId;
    public string utteranceId;
    public string taskState;
    public int priority;

    public float relativeAngle;
    public float sourceX;
    public float sourceY;
    public float sourceZ;
}
