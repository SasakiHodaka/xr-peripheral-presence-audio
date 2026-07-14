using System;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class SemanticPacket
{
    public int version;
    public int sequence;
    public float timestamp;
    public float expireTime;
    public int flags;
    public int speaker;
    public int direction;
    public int intent;
    public int target;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public int GetPacketBytes()
    {
        return Encoding.UTF8.GetByteCount(ToJson());
    }
}

public static class SemanticPacketFlags
{
    public const int Semantic = 1 << 0;
    public const int Audio = 1 << 1;
    public const int Spatial = 1 << 2;
}
