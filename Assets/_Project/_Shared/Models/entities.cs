// 对应 Python models/entities.py
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum ActorType
{
    [EnumMember(Value = "None")] NONE,
    [EnumMember(Value = "NPC")] NPC,
    [EnumMember(Value = "Monster")] MONSTER,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum StageType
{
    [EnumMember(Value = "None")] NONE,
    [EnumMember(Value = "Home")] HOME,
    [EnumMember(Value = "Dungeon")] DUNGEON,
}

public sealed class Actor
{
    public string name = "";
    public ActorType type;
    public string profile = "";
    public string base_body = "";
    public string system_message = "";
    public CharacterStats character_stats = new();
    public List<ComponentSerialization> components = new();
}

public sealed class Stage
{
    public string name = "";
    public StageType type;
    public string profile = "";
    public string system_message = "";
    public List<Actor> actors = new();
    public List<ComponentSerialization> components = new();
}

public sealed class World
{
    public string name = "";
    public string system_message = "";
    public List<ComponentSerialization> components = new();
}
