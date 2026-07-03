// 对应 Python models/world.py
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class AgentContext
{
    public string name = "";
    public List<JObject> context = new(); // List[ContextMessage]，ContextMessage 为服务端内部类型
}

public sealed class Blueprint
{
    public string name = "";
    public string player_actor = "";
    public string campaign_setting = "";
    public List<Stage> stages = new();
    public List<WorldSystem> world_systems = new();
    public string storage_entity = "";
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> storage = new();
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> inventory = new();
    public List<Artifact> artifacts = new();
}

// List<Item> (AnyItem) 转换器（处理列表中的多态元素）
public class AnyItemListConverter : JsonConverter<List<Item>>
{
    private static readonly AnyItemConverter _itemConverter = new();

    public override List<Item> ReadJson(JsonReader reader, Type objectType, List<Item> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var list = new List<Item>();
        foreach (var token in array)
        {
            using var tokenReader = token.CreateReader();
            list.Add(_itemConverter.ReadJson(tokenReader, typeof(Item), null, false, serializer));
        }
        return list;
    }

    public override void WriteJson(JsonWriter writer, List<Item> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

public sealed class World
{
    public int entity_counter = 0;
    public int home_planning_turn_index = 0;
    public List<EntitySerialization> entities_serialization = new();
    public Dictionary<string, AgentContext> agents_context = new();
    public Dungeon dungeon = new();
    public Blueprint blueprint = new();
}
