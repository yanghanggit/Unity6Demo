using System.Collections.Generic;


/// <summary>
/// 角色外观信息组件。
/// </summary>
[System.Serializable]
public sealed class AppearanceComponent
{
    public string name = "";
    public string appearance = "";
}


/// <summary>
/// 盟友组件。
/// </summary>
[System.Serializable]
public sealed class AllyComponent
{
    public string name = "";
}

/// <summary>
/// 敌人组件。
/// </summary>
[System.Serializable]
public sealed class EnemyComponent
{
    public string name = "";
}


/// <summary>
/// 手牌组件。
/// </summary>
[System.Serializable]
public sealed class HandComponent
{
    public string name = "";
    public List<Card> cards = new();
}


/// <summary>
/// 战斗属性组件。
/// </summary>
[System.Serializable]
public sealed class CombatStatsComponent
{
    public string name = "";
    public CharacterStats stats = new();
    public List<StatusEffect> status_effects = new();
}

/// <summary>
/// 死亡组件。
/// </summary>
[System.Serializable]
public sealed class DeathComponent
{
    public string name = "";
}