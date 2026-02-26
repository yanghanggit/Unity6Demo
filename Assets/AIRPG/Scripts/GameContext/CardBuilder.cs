using System.Collections.Generic;
using UnityEngine;

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

    // 方便获取要素名称的属性，根据类型返回对应的名字
    public string Name
    {
        get
        {
            return elementType switch
            {
                CardElementType.TargetActor => targetActor?.name ?? "[空角色]",
                CardElementType.Skill => skill?.name ?? "[空技能]",
                CardElementType.StatusEffect => statusEffect?.name ?? "[空状态]",
                _ => "[未知类型]"
            };
        }
    }

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
/// 卡牌构建器
/// 管理当前正在构建的卡牌的所有要素和构建数据
/// </summary>
public static class CardBuilder
{
    private static readonly List<CardElementData> _elements = new();
    private static CardBuildData _build = new();

    /// <summary>
    /// 获取所有卡牌要素（只读）
    /// </summary>
    public static IReadOnlyList<CardElementData> Elements => _elements.AsReadOnly();

    /// <summary>
    /// 获取或设置卡牌构建数据
    /// </summary>
    public static CardBuildData Build
    {
        get => _build;
        set => _build = value ?? new CardBuildData();
    }

    /// <summary>
    /// 添加一个卡牌要素
    /// </summary>
    public static void AddElement(CardElementData element)
    {
        if (element == null)
        {
            Debug.LogWarning("[CardBuilder] 尝试添加 null 要素");
            return;
        }
        _elements.Add(element);
        Debug.Log($"[CardBuilder] 添加要素: {element.elementType}, 当前总数: {_elements.Count}");
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
            Debug.Log($"[CardBuilder] 移除要素: {removed.elementType}, 剩余: {_elements.Count}");
        }
    }

    /// <summary>
    /// 清空所有要素和构建数据
    /// </summary>
    public static void Clear()
    {
        _elements.Clear();
        _build = new CardBuildData();
        Debug.Log("[CardBuilder] 已清空所有要素和构建数据");
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

    /// <summary>
    /// 切换指定索引的要素在构建数据中的状态（存在则删除，不存在则添加）
    /// </summary>
    /// <param name="elementIndex">要素索引</param>
    /// <returns>是否成功修改</returns>
    public static bool TryToggleElementInBuild(int elementIndex)
    {
        // 检查卡牌构建数据是否存在
        if (Build == null)
        {
            Debug.LogWarning("[CardBuilder] 卡牌构建数据不存在，请先选择构建者（点击角色槽位）");
            return false;
        }

        // 获取对应的要素数据
        var elementData = GetElement(elementIndex);
        if (elementData == null)
        {
            Debug.LogWarning($"[CardBuilder] 未找到索引为 {elementIndex} 的卡牌要素数据");
            return false;
        }

        // 根据要素类型修改 Build 数据
        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                if (elementData.targetActor != null)
                {
                    var existingActorIndex = Build.targetActors.FindIndex(
                        actor => actor.name == elementData.targetActor.name);

                    if (existingActorIndex >= 0)
                    {
                        Build.targetActors.RemoveAt(existingActorIndex);
                        Debug.Log($"[CardBuilder] 删除目标角色: {elementData.targetActor.name}");
                    }
                    else
                    {
                        Build.targetActors.Add(elementData.targetActor);
                        Debug.Log($"[CardBuilder] 添加目标角色: {elementData.targetActor.name}");
                    }
                }
                break;

            case CardElementType.Skill:
                if (Build.skill != null &&
                    !string.IsNullOrEmpty(Build.skill.name) &&
                    Build.skill.name == elementData.skill?.name)
                {
                    Build.skill = new Skill();
                    Debug.Log($"[CardBuilder] 删除技能: {elementData.skill?.name}");
                }
                else
                {
                    Build.skill = elementData.skill;
                    Debug.Log($"[CardBuilder] 设置技能: {elementData.skill?.name ?? "[空技能]"}");
                }
                break;

            case CardElementType.StatusEffect:
                if (elementData.statusEffect != null)
                {
                    var existingEffectIndex = Build.statusEffects.FindIndex(
                        effect => effect.name == elementData.statusEffect.name);

                    if (existingEffectIndex >= 0)
                    {
                        Build.statusEffects.RemoveAt(existingEffectIndex);
                        Debug.Log($"[CardBuilder] 删除状态效果: {elementData.statusEffect.name}");
                    }
                    else
                    {
                        Build.statusEffects.Add(elementData.statusEffect);
                        Debug.Log($"[CardBuilder] 添加状态效果: {elementData.statusEffect.name}");
                    }
                }
                break;

            case CardElementType.None:
            default:
                Debug.LogWarning($"[CardBuilder] 未知的卡牌要素类型: {elementData.elementType}");
                return false;
        }

        return true;
    }

    /// <summary>
    /// 检查指定要素是否已在 Build 中被选中
    /// </summary>
    /// <param name="elementData">要检查的要素数据</param>
    /// <returns>是否已选中</returns>
    public static bool IsElementSelectedInBuild(CardElementData elementData)
    {
        if (Build == null || elementData == null)
        {
            return false;
        }

        switch (elementData.elementType)
        {
            case CardElementType.TargetActor:
                if (elementData.targetActor != null)
                {
                    return Build.targetActors.Exists(
                        actor => actor.name == elementData.targetActor.name);
                }
                break;

            case CardElementType.Skill:
                if (elementData.skill != null &&
                    !string.IsNullOrEmpty(elementData.skill.name))
                {
                    return Build.skill != null &&
                           !string.IsNullOrEmpty(Build.skill.name) &&
                           Build.skill.name == elementData.skill.name;
                }
                break;

            case CardElementType.StatusEffect:
                if (elementData.statusEffect != null)
                {
                    return Build.statusEffects.Exists(
                        effect => effect.name == elementData.statusEffect.name);
                }
                break;
        }

        return false;
    }
}