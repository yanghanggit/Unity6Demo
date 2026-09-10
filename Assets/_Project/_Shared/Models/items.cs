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
    public string uuid = System.Guid.NewGuid().ToString();
    public string description = "";
    public ItemType type;
    public int count = 1;
}

public sealed class GearItem : Item
{
    public GearItem() { type = ItemType.GEAR_ITEM; }
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> resources = new(); // 合成时消耗的原料列表
    public List<Card> cards = new();     // 可转化为手牌的卡牌列表
}

public sealed class CostumeItem : Item
{
    public CostumeItem() { type = ItemType.COSTUME_ITEM; }
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> resources = new(); // 合成时消耗的原料列表
}

public sealed class ConsumableItem : Item
{
    public ConsumableItem() { type = ItemType.CONSUMABLE_ITEM; }
    public List<string> on_use_prompt = new(); // 使用效果提示词列表
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> resources = new();       // 合成时消耗的原料列表
}

public sealed class MaterialItem : Item
{
    public MaterialItem() { type = ItemType.MATERIAL_ITEM; }
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
