using System.Collections.Generic;

/// <summary>
/// 卡牌构建数据
/// </summary>
[System.Serializable]
public class CardBuildData
{
    public EntitySerialization owner = null;              // 卡牌构建者
    public List<EntitySerialization> targetActors = new();
    public Skill skill = new();
    public List<StatusEffect> statusEffects = new();
}