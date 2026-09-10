// 对应 Python models/utils.py
// models 层纯计算工具，不依赖 ECS Entity。
public static class ModelUtils
{
    /// <summary>返回角色的基础属性（当前不做任何加成，仅作副本返回）。</summary>
    public static CharacterStats ComputeEffectiveStats(CharacterStats baseStats)
    {
        return new CharacterStats
        {
            hp = baseStats.hp,
            max_hp = baseStats.max_hp,
            attack = baseStats.attack,
            defense = baseStats.defense,
        };
    }

    /// <summary>计算手牌提供的总格挡（block 之和）。</summary>
    public static int ComputeHandBlock(HandComponent handComponent)
    {
        if (handComponent == null) return 0;
        int total = 0;
        foreach (var card in handComponent.cards)
            total += card.block;
        return total;
    }

    /// <summary>
    /// 将角色属性叠加到卡牌上并返回该卡牌（原地修改）。
    /// 规则：卡牌自身 damage/block 非 0 时，分别叠加角色的 attack/defense。
    /// </summary>
    public static Card ApplyStatsToCard(Card card, CharacterStats stats)
    {
        if (card.damage != 0) card.damage += stats.attack;
        if (card.block != 0) card.block += stats.defense;
        return card;
    }
}
