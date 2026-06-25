// 对应 Python models/utils.py
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// models 层纯计算工具，不依赖 ECS Entity。
/// </summary>
public static class ModelUtils
{
    /// <summary>
    /// 计算角色的最终有效属性，聚合基础属性与装备加成、状态效果加成。
    /// 对应 Python compute_effective_stats。
    /// </summary>
    /// <param name="baseStats">角色基础属性</param>
    /// <param name="statusEffects">当前状态效果列表，null 时不计算</param>
    /// <param name="equippedGear">已装备的装备，null 时不计算</param>
    public static CharacterStats ComputeEffectiveStats(
        CharacterStats baseStats,
        List<StatusEffect> statusEffects = null,
        GearItem equippedGear = null)
    {
        int bonusHp      = 0;
        int bonusMaxHp   = 0;
        int bonusAttack  = 0;
        int bonusDefense = 0;
        int bonusEnergy  = 0;
        int bonusSpeed   = 0;

        if (equippedGear != null)
        {
            bonusHp      += equippedGear.stat_bonuses.hp;
            bonusMaxHp   += equippedGear.stat_bonuses.max_hp;
            bonusAttack  += equippedGear.stat_bonuses.attack;
            bonusDefense += equippedGear.stat_bonuses.defense;
            bonusEnergy  += equippedGear.stat_bonuses.energy;
            bonusSpeed   += equippedGear.stat_bonuses.speed;
        }

        if (statusEffects != null)
        {
            foreach (var se in statusEffects)
            {
                bonusSpeed   += se.speed;
                bonusDefense += se.defense;
            }
        }

        // 对应 Python 的 assert：状态效果不应直接修改 HP
        Debug.Assert(bonusHp == 0,
            "当前设计中装备/状态效果不应直接修改 HP，若需要请改为修改 max_hp 或通过其他机制实现");

        return new CharacterStats
        {
            hp      = baseStats.hp      + bonusHp,
            max_hp  = baseStats.max_hp  + bonusMaxHp,
            attack  = baseStats.attack  + bonusAttack,
            defense = baseStats.defense + bonusDefense,
            energy  = baseStats.energy  + bonusEnergy,
            speed   = baseStats.speed   + bonusSpeed,
        };
    }
}
