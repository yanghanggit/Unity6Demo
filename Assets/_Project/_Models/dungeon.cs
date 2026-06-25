// 对应 Python models/dungeon.py
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class DungeonRoom
{
    public string room_type = "base";
    public Stage stage = new Stage();
    public GeneratedImage image = new GeneratedImage();
}

[System.Serializable]
public sealed class CombatRoom : DungeonRoom
{
    public Combat combat = new Combat();
}

// DungeonRoomUnion 判别联合转换器（discriminator: room_type）
public class DungeonRoomConverter : JsonConverter<DungeonRoom>
{
    public override DungeonRoom ReadJson(JsonReader reader, Type objectType, DungeonRoom existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string roomType = jo["room_type"]?.ToString();
        DungeonRoom room = roomType == "combat" ? new CombatRoom() : new DungeonRoom();
        using (var subReader = jo.CreateReader())
        {
            var subSerializer = new JsonSerializer();
            foreach (var converter in serializer.Converters)
            {
                if (converter is not DungeonRoomConverter)
                    subSerializer.Converters.Add(converter);
            }
            subSerializer.Populate(subReader, room);
        }
        return room;
    }

    public override void WriteJson(JsonWriter writer, DungeonRoom value, JsonSerializer serializer)
    {
        var jo = JObject.FromObject(value, JsonSerializer.CreateDefault());
        jo.WriteTo(writer);
    }
}

[System.Serializable]
public sealed class Dungeon
{
    [JsonConverter(typeof(DungeonRoomListConverter))]
    public List<DungeonRoom> rooms = new List<DungeonRoom>();
    public string name = "";
    public string ecology = "";
    public string created_at = "";
    public int current_room_index = -1;
    public bool setup_entities = false;
    public GeneratedImage image = new GeneratedImage();
}

// List<DungeonRoom> 转换器（处理列表中的多态元素）
public class DungeonRoomListConverter : JsonConverter<List<DungeonRoom>>
{
    private static readonly DungeonRoomConverter _itemConverter = new DungeonRoomConverter();

    public override List<DungeonRoom> ReadJson(JsonReader reader, Type objectType, List<DungeonRoom> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var list = new List<DungeonRoom>();
        foreach (var token in array)
        {
            using var tokenReader = token.CreateReader();
            list.Add(_itemConverter.ReadJson(tokenReader, typeof(DungeonRoom), null, serializer));
        }
        return list;
    }

    public override void WriteJson(JsonWriter writer, List<DungeonRoom> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
