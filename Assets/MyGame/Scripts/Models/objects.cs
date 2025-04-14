

using System.Collections.Generic;


/**
*/
public class ActorType
{
    public const string NONE = "None";
    public const string HERO = "Hero";
    public const string MONSTER = "Monster";
}

/**
*/
public class StageType
{
    public const string NONE = "None";
    public const string HOME = "Home";
    public const string DUNGEON = "Dungeon";
}

[System.Serializable]
public class RPGCharacterProfile
{
    public int experience = 0;
    public int fixed_level = 1;
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
    public int base_magic_defense = 6;

    // 成长系数
    public int strength_per_level = 2;
    public int dexterity_per_level = 1;
    public int wisdom_per_level = 1;

    public int max_hp => base_max_hp + (strength * 10);
    
    public int progression_level => experience / 1000;

    public int level => fixed_level + progression_level;

    public int strength => base_strength + (strength_per_level * progression_level);

    public int dexterity => base_dexterity + (dexterity_per_level * progression_level);

    public int wisdom => base_wisdom + (wisdom_per_level * progression_level);

    public int physical_attack => base_physical_attack + (strength * 2);

    public int physical_defense => base_physical_defense + strength;

    public int magic_attack => base_magic_attack + (wisdom * 2);

    public int magic_defense => base_magic_defense + wisdom;
}

/**
*/
[System.Serializable]
public class Actor
{
    public string name = "";
    public ActorPrototype prototype = new ActorPrototype();
    public string system_message = "";
    public string kick_off_message = "";
    public RPGCharacterProfile rpg_character_profile = new RPGCharacterProfile();
}

/**
*/
[System.Serializable]
public class Stage
{
    public string name = "";
    public StagePrototype prototype = new StagePrototype();
    public string system_message = "";
    public string kick_off_message = "";
    public List<Actor> actors = new List<Actor>();
}

/**
*/
[System.Serializable]
public class WorldSystem
{
    public string name = "";
    public string system_message = "";
    public string kick_off_message = "";
}