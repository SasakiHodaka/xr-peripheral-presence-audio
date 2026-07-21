using System;
using UnityEditor;
using UnityEngine;

public static class SemanticTransportSelfCheck
{
    [MenuItem("Tools/Semantic Spatial Audio/Run Semantic Transport Self Check")]
    public static void RunFromMenu()
    {
        Run();
        Debug.Log("[SemanticTransportSelfCheck] Passed.");
    }

    public static void RunForBatch()
    {
        try
        {
            Run();
            Debug.Log("[SemanticTransportSelfCheck] Passed.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Run()
    {
        GameObject testObject = new GameObject("SemanticTransportSelfCheck");

        try
        {
            LocalLoopbackSemanticPacketTransport loopback =
                testObject.AddComponent<LocalLoopbackSemanticPacketTransport>();
            SemanticPacketTransportRouter router =
                testObject.AddComponent<SemanticPacketTransportRouter>();
            router.ConfigureIdentity("self-check-session", "self-check-participant");

            SemanticPacketEnvelope receivedEnvelope = null;
            SemanticPacket receivedPacket = null;
            router.PacketReceived += (envelope, packet) =>
            {
                receivedEnvelope = envelope;
                receivedPacket = packet;
            };

            SemanticPacket sentPacket = new SemanticPacket
            {
                version = 1,
                sequence = 42,
                timestamp = 1.25f,
                expireTime = 3.25f,
                flags = SemanticPacketFlags.Semantic | SemanticPacketFlags.Spatial,
                speaker = 2,
                direction = 1,
                intent = 1,
                target = 7
            };

            Require(loopback.IsConnected, "Loopback transport should be connected.");
            Require(router.Publish("self-check-event", sentPacket),
                "Router should publish a valid packet.");
            Require(receivedEnvelope != null, "Router should receive its loopback envelope.");
            Require(receivedPacket != null, "Envelope should decode into a packet.");
            Require(receivedEnvelope.sessionId == "self-check-session", "Session ID was not preserved.");
            Require(receivedEnvelope.senderParticipantId == "self-check-participant",
                "Participant ID was not preserved.");
            Require(receivedEnvelope.eventId == "self-check-event", "Event ID was not preserved.");
            Require(receivedPacket.sequence == sentPacket.sequence, "Packet sequence was not preserved.");
            Require(receivedPacket.flags == sentPacket.flags, "Packet flags were not preserved.");
            Require(receivedPacket.target == sentPacket.target, "Packet target was not preserved.");

            SemanticPacketEnvelope invalid = new SemanticPacketEnvelope
            {
                schemaVersion = 999,
                packetJson = "{}"
            };
            Require(!loopback.TrySend(invalid), "Loopback should reject an unsupported schema.");

            loopback.SetConnected(false);
            Require(!router.Publish("disconnected-event", sentPacket),
                "Router should reject sends while disconnected.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
