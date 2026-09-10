// 对应 Python models/agent_memory.py
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class AgentMemory
{
    public string name = "";
    [JsonConverter(typeof(AnyContextMessageListConverter))]
    public List<BaseMessage> messages = new();
    public float context_usage_ratio = 0f;
}
