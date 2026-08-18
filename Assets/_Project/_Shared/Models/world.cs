// 对应 Python models/world.py
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class AgentContext
{
    public string name = "";
    [JsonConverter(typeof(AnyContextMessageListConverter))]
    public List<BaseMessage> context = new(); // List[ContextMessage]，判别联合定义见 messages.cs
}

public sealed class World
{
    public int entity_counter = 0;
    public List<EntitySerialization> entities = new();
    public Dictionary<string, AgentContext> agents_context = new();
    public Dungeon dungeon = new();
    public Blueprint blueprint = new();
}
