// 对应 Python models/session_message.py
using Newtonsoft.Json.Linq;

public enum MessageType
{
    NONE = 0,
    AGENT_EVENT = 1,
}

public sealed class SessionMessage
{
    public int message_type = (int)MessageType.NONE;
    public JObject data = new();
    public int sequence_id = 0;
}
