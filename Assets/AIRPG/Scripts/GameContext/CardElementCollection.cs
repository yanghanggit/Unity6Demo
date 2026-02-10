using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌要素类型枚举
/// </summary>
public enum CardElementType
{
    None,           // 未指定
    TargetActor,    // 目标角色
    Skill,          // 技能
    StatusEffect    // 状态效果
}

/// <summary>
/// 卡牌要素数据
/// 包含构建一张卡牌所需的各种要素
/// </summary>
[System.Serializable]
public class CardElementData
{
    public CardElementType elementType;

    // 三种要素类型，根据 elementType 使用对应的字段，其他字段为 null
    public EntitySerialization targetActor;      // 当 elementType == TargetActor 时使用
    public Skill skill;                          // 当 elementType == Skill 时使用
    public StatusEffect statusEffect;            // 当 elementType == StatusEffect 时使用

    // 构造函数重载，方便创建不同类型的要素
    public CardElementData(EntitySerialization actor)
    {
        elementType = CardElementType.TargetActor;
        targetActor = actor;
    }

    public CardElementData(Skill skillData)
    {
        elementType = CardElementType.Skill;
        skill = skillData;
    }

    public CardElementData(StatusEffect effect)
    {
        elementType = CardElementType.StatusEffect;
        statusEffect = effect;
    }
}

/// <summary>
/// 卡牌要素集合管理器
/// 管理当前正在构建的卡牌的所有要素
/// </summary>
public static class CardElementCollection
{
    private static List<CardElementData> _elements = new List<CardElementData>();

    /// <summary>
    /// 获取所有卡牌要素（只读）
    /// </summary>
    public static IReadOnlyList<CardElementData> Elements => _elements.AsReadOnly();

    /// <summary>
    /// 添加一个卡牌要素
    /// </summary>
    public static void AddElement(CardElementData element)
    {
        if (element == null)
        {
            Debug.LogWarning("[CardElementCollection] 尝试添加 null 要素");
            return;
        }
        _elements.Add(element);
        Debug.Log($"[CardElementCollection] 添加要素: {element.elementType}, 当前总数: {_elements.Count}");
    }

    /// <summary>
    /// 移除指定索引的要素
    /// </summary>
    public static void RemoveElementAt(int index)
    {
        if (index >= 0 && index < _elements.Count)
        {
            var removed = _elements[index];
            _elements.RemoveAt(index);
            Debug.Log($"[CardElementCollection] 移除要素: {removed.elementType}, 剩余: {_elements.Count}");
        }
    }

    /// <summary>
    /// 清空所有要素
    /// </summary>
    public static void Clear()
    {
        _elements.Clear();
        Debug.Log("[CardElementCollection] 已清空所有要素");
    }

    /// <summary>
    /// 获取指定索引的要素
    /// </summary>
    public static CardElementData GetElement(int index)
    {
        if (index >= 0 && index < _elements.Count)
        {
            return _elements[index];
        }
        return null;
    }

    /// <summary>
    /// 获取要素数量
    /// </summary>
    public static int Count => _elements.Count;

    /// <summary>
    /// 判断是否包含指定类型的要素
    /// </summary>
    public static bool HasElementOfType(CardElementType type)
    {
        return _elements.Exists(e => e.elementType == type);
    }

    /// <summary>
    /// 获取指定类型的所有要素
    /// </summary>
    public static List<CardElementData> GetElementsByType(CardElementType type)
    {
        return _elements.FindAll(e => e.elementType == type);
    }
}
