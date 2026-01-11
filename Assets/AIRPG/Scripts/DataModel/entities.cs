
using System.Collections.Generic;


/**
 * 
 */
[System.Serializable]
public class CharacterSheet
{
    public string name = "";
    public string type = "";
    public string profile = "";
    public string appearance = "";
}

/**
 * 
 */
[System.Serializable]
public class StageProfile
{
    public string name = "";
    public string type = "";
    public string profile = "";
}

/**
 * 角色类型（对应 Python StrEnum）
 */
public static class ActorType
{
    public const string NONE = "None";
    public const string ALLY = "Ally";        // 我方/盟友/好人阵营
    public const string ENEMY = "Enemy";      // 敌方/怪物/坏人阵营
}

/**
 * 场景类型（对应 Python StrEnum）
 */
public static class StageType
{
    public const string NONE = "None";
    public const string HOME = "Home";
    public const string DUNGEON = "Dungeon";
}

/**
 * 物品类型（对应 Python StrEnum）
 */
public static class ItemType
{
    public const string WEAPON_ITEM = "WeaponItem";          // 武器
    public const string EQUIPMENT_ITEM = "EquipmentItem";           // 装备
    public const string CONSUMABLE_ITEM = "ConsumableItem";  // 消耗品
    public const string MATERIAL_ITEM = "MaterialItem";      // 材料
    public const string UNIQUE_ITEM = "UniqueItem"; // 独特物品/任务物品
}

/**
 * 物品基类
 */
[System.Serializable]
public class Item
{
    public string name = "";
    public string uuid = "";
    public string description = "";
    public string type = "";  // 使用 string 类型存储
    public int count = 1;  // 物品数量，默认为1
}

/**
 * 武器类，继承自物品基类
 */
[System.Serializable]
public class WeaponItem : Item
{
    public WeaponItem()
    {
        type = ItemType.WEAPON_ITEM;
    }
}

/**
 * 装备类，继承自物品基类
 */
[System.Serializable]
public class EquipmentItem : Item
{
    public EquipmentItem()
    {
        type = ItemType.EQUIPMENT_ITEM;
    }
}

/**
 * 消耗品类，继承自物品基类
 */
[System.Serializable]
public class ConsumableItem : Item
{
    public ConsumableItem()
    {
        type = ItemType.CONSUMABLE_ITEM;
    }
}

/**
 * 材料类，继承自物品基类
 */
[System.Serializable]
public class MaterialItem : Item
{
    public MaterialItem()
    {
        type = ItemType.MATERIAL_ITEM;
    }
}

/**
 * 珍贵物品类，继承自物品基类
 */
[System.Serializable]
public class UniqueItem : Item
{
    public UniqueItem()
    {
        type = ItemType.UNIQUE_ITEM;
    }
}

/**
 * 技能类
 */
[System.Serializable]
public class Skill
{
    public string name = "";        // 技能名称
    public string description = ""; // 技能描述
}

/**
 * 角色属性类 - 简化版本，只包含核心战斗属性
 */
[System.Serializable]
public class CharacterStats
{
    // 当前生命值
    public int hp = 0;
    // 最大生命值
    public int max_hp = 50;
    // 攻击力
    public int attack = 10;
    // 防御力
    public int defense = 5;
}

/**
 * 角色类
 */
[System.Serializable]
public class Actor
{
    public string name = "";
    public CharacterSheet character_sheet = new();
    public string system_message = "";
    public string kick_off_message = "";
    public CharacterStats character_stats = new();
    public List<Item> items = new();
    public List<Skill> skills = new();
}

/**
 * 场景类
 */
[System.Serializable]
public class Stage
{
    public string name = "";
    public StageProfile stage_profile = new();
    public string system_message = "";
    public string kick_off_message = "";
    public List<Actor> actors = new();
}

/**
 * 世界系统类
 */
[System.Serializable]
public class WorldSystem
{
    public string name = "";
    public string system_message = "";
    public string kick_off_message = "";
}