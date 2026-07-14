using UnityEngine;

public sealed class GroundTruthSceneTokenGenerator : MonoBehaviour
{
    public GeneratedSceneToken Generate(
        string scenarioId,
        GroundTruthEvent scenarioEvent
    )
    {
        if (scenarioEvent == null)
        {
            Debug.LogWarning("Cannot generate Scene Token from null event.");
            return null;
        }

        Vector3 speakerPosition = scenarioEvent.speakerPosition.ToVector3();
        Vector3 listenerPosition = scenarioEvent.listenerPosition.ToVector3();
        float relativeAngle = CalculateRelativeAngle(
            listenerPosition,
            scenarioEvent.listenerRotationY,
            speakerPosition
        );

        return new GeneratedSceneToken
        {
            scenarioId = scenarioId,
            eventId = scenarioEvent.eventId,
            sequence = scenarioEvent.sequence,
            expectedTime = scenarioEvent.expectedTime,
            speakerId = scenarioEvent.speakerId,
            direction = ClassifyDirection(relativeAngle),
            targetObjectId = scenarioEvent.objectId,
            utteranceId = scenarioEvent.utteranceId,
            taskState = scenarioEvent.taskState,
            priority = scenarioEvent.priority,
            relativeAngle = relativeAngle,
            sourceX = speakerPosition.x,
            sourceY = speakerPosition.y,
            sourceZ = speakerPosition.z
        };
    }

    private static float CalculateRelativeAngle(
        Vector3 listenerPosition,
        float listenerRotationY,
        Vector3 speakerPosition
    )
    {
        Vector3 toSpeaker = speakerPosition - listenerPosition;
        toSpeaker.y = 0.0f;

        if (toSpeaker.sqrMagnitude <= Mathf.Epsilon)
        {
            return 0.0f;
        }

        Vector3 listenerForward =
            Quaternion.Euler(0.0f, listenerRotationY, 0.0f) * Vector3.forward;

        float angle = Vector3.SignedAngle(
            listenerForward,
            toSpeaker.normalized,
            Vector3.up
        );

        return NormalizeAngle(angle);
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180.0f)
        {
            angle -= 360.0f;
        }

        while (angle <= -180.0f)
        {
            angle += 360.0f;
        }

        return angle;
    }

    private static string ClassifyDirection(float relativeAngle)
    {
        // Direction bins: Front [-45, 45), Right [45, 135), Left [-135, -45), Behind otherwise.
        if (relativeAngle >= -45.0f && relativeAngle < 45.0f)
        {
            return "Front";
        }

        if (relativeAngle >= 45.0f && relativeAngle < 135.0f)
        {
            return "Right";
        }

        if (relativeAngle >= -135.0f && relativeAngle < -45.0f)
        {
            return "Left";
        }

        return "Behind";
    }
}
