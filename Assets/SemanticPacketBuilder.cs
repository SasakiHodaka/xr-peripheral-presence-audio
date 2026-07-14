using System.Collections.Generic;
using UnityEngine;

public sealed class SemanticPacketBuilder : MonoBehaviour
{
    private readonly Dictionary<string, int> speakerIds = new Dictionary<string, int>();
    private readonly Dictionary<string, int> targetIds = new Dictionary<string, int>();
    private int nextSpeakerId = 1;
    private int nextTargetId = 1;
    private int nextSequence = 1;

    public SemanticPacket Build(
        GeneratedSceneToken token,
        SelectionResult selection
    )
    {
        if (token == null || selection == null || !selection.selected)
        {
            return null;
        }

        float timestamp = Time.time;
        float semanticLifetime = GetSemanticLifetimeSeconds(token.priority);

        return new SemanticPacket
        {
            version = 1,
            sequence = nextSequence++,
            timestamp = timestamp,
            expireTime = timestamp + semanticLifetime,
            flags = BuildFlags(selection.level),
            speaker = GetOrCreateId(speakerIds, token.speakerId, ref nextSpeakerId),
            direction = EncodeDirection(token.direction),
            intent = EncodeIntent(token.priority),
            target = GetOrCreateId(targetIds, token.targetObjectId, ref nextTargetId)
        };
    }

    private static int BuildFlags(CommunicationLevel level)
    {
        switch (level)
        {
            case CommunicationLevel.TokenOnly:
                return SemanticPacketFlags.Semantic | SemanticPacketFlags.Spatial;
            case CommunicationLevel.AudioOnly:
                return SemanticPacketFlags.Audio | SemanticPacketFlags.Spatial;
            case CommunicationLevel.AudioAndToken:
                return SemanticPacketFlags.Semantic |
                       SemanticPacketFlags.Audio |
                       SemanticPacketFlags.Spatial;
            case CommunicationLevel.None:
            default:
                return 0;
        }
    }

    private static int EncodeDirection(string direction)
    {
        switch (direction)
        {
            case "Front":
                return 0;
            case "Right":
                return 1;
            case "Left":
                return 2;
            case "Behind":
                return 3;
            default:
                return -1;
        }
    }

    private static int EncodeIntent(int priority)
    {
        return priority >= 2 ? 1 : 0;
    }

    private static float GetSemanticLifetimeSeconds(int priority)
    {
        return priority >= 2 ? 0.2f : 2.0f;
    }

    private static int GetOrCreateId(
        Dictionary<string, int> ids,
        string key,
        ref int nextId
    )
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        if (ids.TryGetValue(key, out int id))
        {
            return id;
        }

        id = nextId++;
        ids[key] = id;
        return id;
    }
}
