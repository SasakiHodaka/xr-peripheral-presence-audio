using System;
using UnityEngine;

public sealed class SemanticPacketTransportRouter : MonoBehaviour
{
    [SerializeField] private MonoBehaviour transportComponent;
    [SerializeField] private string sessionId = "local-experiment";
    [SerializeField] private string participantId = "local-participant";

    private ISemanticPacketTransport transport;

    public event Action<SemanticPacketEnvelope, SemanticPacket> PacketReceived;
    public bool IsConnected => transport != null && transport.IsConnected;

    private void Awake()
    {
        ResolveTransport();
    }

    private void OnEnable()
    {
        ResolveTransport();
        if (transport != null) transport.PacketReceived += HandlePacketReceived;
    }

    private void OnDisable()
    {
        if (transport != null) transport.PacketReceived -= HandlePacketReceived;
    }

    public bool Publish(string eventId, SemanticPacket packet)
    {
        if (packet == null) return false;
        ResolveTransport();
        if (transport == null || !transport.IsConnected) return false;

        SemanticPacketEnvelope envelope = SemanticPacketEnvelope.Create(
            sessionId,
            participantId,
            eventId,
            packet,
            Time.realtimeSinceStartupAsDouble);

        return transport.TrySend(envelope);
    }

    public void ConfigureIdentity(string newSessionId, string newParticipantId)
    {
        sessionId = string.IsNullOrWhiteSpace(newSessionId) ? "local-experiment" : newSessionId;
        participantId = string.IsNullOrWhiteSpace(newParticipantId) ? "local-participant" : newParticipantId;
    }

    private void ResolveTransport()
    {
        if (transport != null) return;

        if (transportComponent != null)
        {
            transport = transportComponent as ISemanticPacketTransport;
        }

        if (transport == null)
        {
            MonoBehaviour[] candidates = GetComponents<MonoBehaviour>();
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] is ISemanticPacketTransport candidate)
                {
                    transportComponent = candidates[i];
                    transport = candidate;
                    break;
                }
            }
        }
    }

    private void HandlePacketReceived(SemanticPacketEnvelope envelope)
    {
        if (envelope == null || !envelope.TryGetPacket(out SemanticPacket packet))
        {
            Debug.LogWarning("Semantic packet transport rejected an invalid envelope.", this);
            return;
        }

        PacketReceived?.Invoke(envelope, packet);
    }
}
