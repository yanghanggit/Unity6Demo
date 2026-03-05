using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

public static partial class GameUtils
{
    /// <summary>
    /// 从完整的角色名称中提取显示名称（最后一部分）
    /// 例如："角色.战士.卡恩" -> "卡恩"
    /// </summary>
    /// <param name="fullName">完整的角色名称</param>
    /// <returns>提取的显示名称，如果输入为空则返回空字符串</returns>
    public static string GetDisplayName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        int lastDotIndex = fullName.LastIndexOf('.');
        if (lastDotIndex >= 0 && lastDotIndex < fullName.Length - 1)
        {
            return fullName.Substring(lastDotIndex + 1);
        }

        return fullName;
    }

    /// <summary>
    /// 从实体序列化数据中获取指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="actorEntitySerialization">实体序列化数据</param>
    /// <returns>如果找到返回组件实例，否则返回 null</returns>
    public static T GetComponent<T>(EntitySerialization actorEntitySerialization) where T : class
    {
        if (actorEntitySerialization?.components == null)
            return null;

        string componentName = typeof(T).Name;
        foreach (var component in actorEntitySerialization.components)
        {
            if (component.name == componentName)
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(component.data));
            }
        }

        return null;
    }

    /// <summary>
    /// 从代理事件列表中提取指定类型的事件
    /// 使用泛型筛选并转换事件类型，避免手动遍历和类型判断
    /// </summary>
    /// <typeparam name="T">目标事件类型，必须继承自 AgentEvent</typeparam>
    /// <param name="agentEvents">代理事件列表</param>
    /// <returns>指定类型的事件列表，如果没有则返回空列表</returns>
    public static List<T> FilterEventsByType<T>(List<AgentEvent> agentEvents) where T : AgentEvent
    {
        var filteredEvents = new List<T>();

        foreach (var agentEvent in agentEvents)
        {
            if (agentEvent is T typedEvent)
            {
                filteredEvents.Add(typedEvent);
            }
        }

        return filteredEvents;
    }

    /// <summary>
    /// 从代理事件历史中获取所有拥有指定类型事件的角色集合
    /// 遍历所有角色的事件历史,查找包含指定类型事件的角色
    /// </summary>
    /// <typeparam name="T">要查找的事件类型,必须继承自 AgentEvent</typeparam>
    /// <param name="agentEventsHistory">代理事件历史字典,键为角色名称,值为该角色的事件列表</param>
    /// <returns>返回拥有指定类型事件的角色名称集合,如果没有则返回空集合</returns>
    public static HashSet<string> GetActorsWithEventType<T>(Dictionary<string, List<AgentEvent>> agentEventsHistory) where T : AgentEvent
    {
        HashSet<string> actorsWithEvent = new();

        foreach (var kvp in agentEventsHistory)
        {
            string actorName = kvp.Key;
            List<AgentEvent> events = kvp.Value;

            // 使用 FilterEventsByType 筛选指定类型的事件
            var filteredEvents = FilterEventsByType<T>(events);

            // 如果该角色有目标类型的事件,将其添加到结果集
            if (filteredEvents.Count > 0)
            {
                actorsWithEvent.Add(actorName);
            }
        }

        return actorsWithEvent;
    }

    /// <summary>
    /// 从会话消息中解析代理事件对象，根据事件头类型反序列化为对应的 AgentEvent 子类实例
    /// </summary>
    /// <param name="sessionMessage">会话消息对象，必须是 AGENT_EVENT 类型</param>
    /// <returns>解析成功返回对应的 AgentEvent 实例，失败返回 null</returns>
    public static AgentEvent ParseAgentEvent(SessionMessage sessionMessage)
    {
        if (sessionMessage == null || sessionMessage.message_type != (int)MessageType.AGENT_EVENT)
        {
            return null;
        }

        JToken dataToken = JToken.FromObject(sessionMessage.data);
        var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
        if (eventHead < 0)
        {
            Debug.Assert(false, "Invalid event head in session message");
            return null;
        }

        switch ((EventHead)eventHead)
        {
            case EventHead.NONE:
                Debug.Log($"NONE event encountered, skipping processing. message = {dataToken["message"]?.ToString() ?? "No message"}");
                break;

            case EventHead.SPEAK_EVENT:
                return dataToken.ToObject<SpeakEvent>();

            case EventHead.WHISPER_EVENT:
                return dataToken.ToObject<WhisperEvent>();

            case EventHead.ANNOUNCE_EVENT:
                return dataToken.ToObject<AnnounceEvent>();

            case EventHead.MIND_EVENT:
                return dataToken.ToObject<MindEvent>();

            case EventHead.TRANS_STAGE_EVENT:
                return dataToken.ToObject<TransStageEvent>();

            case EventHead.COMBAT_INITIATION_EVENT:
                return dataToken.ToObject<CombatInitiationEvent>();

            case EventHead.COMBAT_ARBITRATION_EVENT:
                return dataToken.ToObject<CombatArbitrationEvent>();

            case EventHead.COMBAT_ARCHIVE_EVENT:
                return dataToken.ToObject<CombatArchiveEvent>();

            default:
                Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }

        return null;
    }

    /// <summary>
    /// 按照行动顺序对角色实体列表进行排序
    /// 
    /// 以 <paramref name="actionOrderNames"/> 中角色名称的顺序为基准，
    /// 从 <paramref name="actorEntityPool"/> 中依次查找并收集对应的实体，
    /// 返回按行动顺序排列的新列表。
    /// 
    /// 注意：若某个行动顺序中的角色名在实体池中找不到对应实体，则该角色会被跳过。
    /// </summary>
    /// <param name="actorEntityPool">待排序的角色实体集合（顺序任意）</param>
    /// <param name="actionOrderNames">行动顺序中各角色的名称列表（决定输出顺序）</param>
    /// <returns>按行动顺序排列的角色实体列表</returns>
    public static List<EntitySerialization> SortActorsByActionOrder(
        List<EntitySerialization> actorEntityPool,
        List<string> actionOrderNames)
    {
        var sortedEntities = new List<EntitySerialization>();

        foreach (var actorName in actionOrderNames)
        {
            foreach (var entity in actorEntityPool)
            {
                if (entity.name == actorName)
                {
                    sortedEntities.Add(entity);
                    break;
                }
            }
        }

        return sortedEntities;
    }


    /// <summary>
    /// 按创建顺序对角色实体列表进行排序
    /// 
    /// 读取每个实体的 <see cref="IdentityComponent.creation_order"/> 字段，
    /// 返回按该值从小到大排列的新列表。
    /// 
    /// 注意：若某个实体无法获取到 <see cref="IdentityComponent"/>，
    /// 其 creation_order 视为 <see cref="int.MaxValue"/>，排在最后。
    /// </summary>
    /// <param name="actorEntities">待排序的角色实体列表</param>
    /// <returns>按 creation_order 升序排列的新列表</returns>
    public static List<EntitySerialization> SortActorsByCreationOrder(List<EntitySerialization> actorEntities)
    {
        var sorted = new List<EntitySerialization>(actorEntities);
        sorted.Sort((a, b) =>
        {
            var identityA = GetComponent<IdentityComponent>(a);
            Debug.Assert(identityA != null, $"Entity {a.name} is missing IdentityComponent");
            var identityB = GetComponent<IdentityComponent>(b);
            Debug.Assert(identityB != null, $"Entity {b.name} is missing IdentityComponent");
            int orderA = identityA?.creation_order ?? int.MaxValue;
            int orderB = identityB?.creation_order ?? int.MaxValue;
            return orderA.CompareTo(orderB);
        });
        return sorted;
    }
}