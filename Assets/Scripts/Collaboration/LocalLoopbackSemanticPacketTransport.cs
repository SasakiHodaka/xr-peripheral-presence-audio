using System;
using UnityEngine;

public sealed class LocalLoopbackSemanticPacketTransport : MonoBehaviour, ISemanticPacketTransport
{
    [SerializeField] private bool connected = true;

    public bool IsConnected => connected && isActiveAndEnabled;
    public event Action<SemanticPacketEnvelope> PacketReceived;

    public bool TrySend(SemanticPacketEnvelope envelope)
    {
        if (!IsConnected || envelope == null || !envelope.TryGetPacket(out _))
        {
            return false;
        }

        PacketReceived?.Invoke(envelope);
        return true;
    }

    public void SetConnected(bool value)
    {
        connected = value;
    }
}
