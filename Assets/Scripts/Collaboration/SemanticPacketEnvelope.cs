using System;
using UnityEngine;

[Serializable]
public sealed class SemanticPacketEnvelope
{
    public const int CurrentSchemaVersion = 1;

    public int schemaVersion = CurrentSchemaVersion;
    public string sessionId;
    public string senderParticipantId;
    public string eventId;
    public double sentAtSeconds;
    public string packetJson;

    public static SemanticPacketEnvelope Create(
        string sessionId,
        string senderParticipantId,
        string eventId,
        SemanticPacket packet,
        double sentAtSeconds)
    {
        if (packet == null) throw new ArgumentNullException(nameof(packet));

        return new SemanticPacketEnvelope
        {
            schemaVersion = CurrentSchemaVersion,
            sessionId = sessionId ?? string.Empty,
            senderParticipantId = senderParticipantId ?? string.Empty,
            eventId = eventId ?? string.Empty,
            sentAtSeconds = sentAtSeconds,
            packetJson = packet.ToJson()
        };
    }

    public bool TryGetPacket(out SemanticPacket packet)
    {
        packet = null;
        if (schemaVersion != CurrentSchemaVersion || string.IsNullOrWhiteSpace(packetJson))
        {
            return false;
        }

        try
        {
            packet = JsonUtility.FromJson<SemanticPacket>(packetJson);
            return packet != null && packet.version > 0;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
