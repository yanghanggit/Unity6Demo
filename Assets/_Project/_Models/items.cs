// 对应 Python models/items.py
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum ItemType
{
    [EnumMember(Value = "GearItem")] GEAR_ITEM,
    [EnumMember(Value = "CostumeItem")] COSTUME_ITEM,
    [EnumMember(Value = "ConsumableItem")] CONSUMABLE_ITEM,
    [EnumMember(Value = "MaterialItem")] MATERIAL_ITEM,
}

[JsonConverter(typeof(AnyItemConverter))]
public class Item
{
    public string name = "";
    public string uuid = "";
    public string description = "";
    public ItemType type;
    public int count = 1;
}

public sealed class GearItem : Item
{
    public CharacterStats stat_bonuses = new();
    public TargetType target_type = TargetType.ALLY_SINGLE;
    public List<string> equip_affixes = new();
    public List<string> on_hit_affixes = new();
    public List<string> modifiers = new();
    public int max_durability = 3;
    public int durability = 3;
    public List<Item> craft_materials = new();
}

public sealed class CostumeItem : Item
{
    public List<Item> craft_materials = new();
}

public sealed class ConsumableItem : Item
{
    public TargetType target_type = TargetType.SELF_ONLY;
    public List<string> affixes = new();
    public List<string> modifiers = new();
    public List<Item> craft_materials = new();
}

public sealed class MaterialItem : Item
{
}

// AnyItem 判别联合类型转换器（对应 Python AnyItem = Union[GearItem, CostumeItem, ConsumableItem, MaterialItem] discriminator="type"）
public class AnyItemConverter : JsonConverter<Item>
{
    public override Item ReadJson(JsonReader reader, Type objectType, Item existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string typeStr = jo["type"]?.ToString();
        Item item = typeStr switch
        {
            "GearItem" => new GearItem(),
            "CostumeItem" => new CostumeItem(),
            "ConsumableItem" => new ConsumableItem(),
            "MaterialItem" => new MaterialItem(),
            _ => new Item()
        };
        // 禁用此 converter 以避免递归，直接 populate
        using (var subReader = jo.CreateReader())
        {
            var subSerializer = new JsonSerializer();
            foreach (var converter in serializer.Converters)
            {
                if (converter is not AnyItemConverter)
                    subSerializer.Converters.Add(converter);
            }
            subSerializer.Populate(subReader, item);
        }
        return item;
    }

    public override void WriteJson(JsonWriter writer, Item value, JsonSerializer serializer)
    {
        var jo = JObject.FromObject(value, JsonSerializer.CreateDefault());
        jo.WriteTo(writer);
    }
}
