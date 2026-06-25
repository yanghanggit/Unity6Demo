// 对应 Python models/entities.py
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[System.Serializable]
public sealed class CharacterSheet
{
    public string name = "";
    public string type = "";
    public string profile = "";
    public string base_body = "";
}

[System.Serializable]
public sealed class StageProfile
{
    public string name = "";
    public string type = "";
    public string profile = "";
}

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

[System.Serializable]
public sealed class Actor
{
    public string name = "";
    public CharacterSheet character_sheet = new CharacterSheet();
    public string system_message = "";
    public CharacterStats character_stats = new CharacterStats();
    public CostumeItem custom_item = null; // Optional[CostumeItem]
    public List<string> keywords = new List<string>();
}

[System.Serializable]
public sealed class Stage
{
    public string name = "";
    public StageProfile stage_profile = new StageProfile();
    public string system_message = "";
    public List<Actor> actors = new List<Actor>();
}

[System.Serializable]
public sealed class WorldSystem
{
    public string name = "";
    public string system_message = "";
    public List<ComponentSerialization> components = new List<ComponentSerialization>();
}
