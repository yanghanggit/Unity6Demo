
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
 * 狼人杀角色名称（对应 Python StrEnum）
 */
public static class WerewolfCharacterSheetName
{
    public const string MODERATOR = "ww.moderator";
    public const string WEREWOLF = "ww.werewolf";
    public const string SEER = "ww.seer";
    public const string WITCH = "ww.witch";
    public const string VILLAGER = "ww.villager";
    public const string HUNTER = "ww.hunter";
}

/**
 * 女巫道具名称（对应 Python StrEnum）
 */
public static class WitchItemName
{
    public const string CURE = "道具.解药";
    public const string POISON = "道具.毒药";
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
 * 角色属性类
 */
[System.Serializable]
public class CharacterStats
{
    public int experience = 0;
    public int initial_level = 1;
    public int hp = 0;

    // 基础属性
    public int base_max_hp = 50;
    public int base_strength = 5;
    public int base_dexterity = 6;
    public int base_wisdom = 5;

    // 基础战斗属性
    public int base_physical_attack = 8;
    public int base_physical_defense = 5;
    public int base_magic_attack = 7;
    // public int base_magic_defense = 6;

    // 成长系数
    public int strength_per_level = 2;
    public int dexterity_per_level = 1;
    public int wisdom_per_level = 1;

    public int max_hp => base_max_hp + (strength * 10);
    
    public int progression_level => experience / 1000;

    public int level => initial_level + progression_level;

    public int strength => base_strength + (strength_per_level * progression_level);

    public int dexterity => base_dexterity + (dexterity_per_level * progression_level);

    public int wisdom => base_wisdom + (wisdom_per_level * progression_level);

    public int physical_attack => base_physical_attack + (strength * 2);

    public int physical_defense => base_physical_defense + strength;

    public int magic_attack => base_magic_attack + (wisdom * 2);

    // public int magic_defense => base_magic_defense + wisdom;
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