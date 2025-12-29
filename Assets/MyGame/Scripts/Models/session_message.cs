using System.Collections.Generic;

/// <summary>
/// 消息类型枚举
/// </summary>
public enum MessageType
{
    NONE = 0,
    AGENT_EVENT = 1,
    GAME = 2,
}

/// <summary>
/// 客户端消息类
/// </summary>
[System.Serializable]
public sealed class SessionMessage
{
    public int message_type = (int)MessageType.NONE;
    public Dictionary<string, object> data = new();
    public int sequence_id = 0;
}