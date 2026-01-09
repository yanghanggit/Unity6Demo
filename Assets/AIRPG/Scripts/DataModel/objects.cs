
using System.Collections.Generic;


/**
 * 
 */
[System.Serializable]
public class ActorCharacterSheet
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
public class StageCharacterSheet
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
    public const string NEUTRAL = "Neutral";  // 中立角色
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
    public const string NONE = "None";
    public const string WEAPON = "Weapon";          // 武器
    public const string ARMOR = "Armor";            // 防具
    public const string CONSUMABLE = "Consumable";  // 消耗品
    public const string MATERIAL = "Material";      // 材料
    public const string ACCESSORY = "Accessory";    // 饰品
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
    public string type = ItemType.NONE;  // 使用 string 类型存储
    public string description = "";
    public int count = 1;  // 物品数量，默认为1
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
    public ActorCharacterSheet character_sheet = new();
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
    public StageCharacterSheet character_sheet = new();
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