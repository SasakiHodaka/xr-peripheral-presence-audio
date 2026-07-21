using System;

public interface ISemanticPacketTransport
{
    bool IsConnected { get; }
    event Action<SemanticPacketEnvelope> PacketReceived;
    bool TrySend(SemanticPacketEnvelope envelope);
}
