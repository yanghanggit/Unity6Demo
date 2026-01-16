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
    /// 获取地下城战斗序列中最后一个回合
    /// </summary>
    /// <param name="dungeon">地下城对象</param>
    /// <returns>如果存在则返回最后一个回合，否则返回 null</returns>
    public static Round GetLastRound(Dungeon dungeon)
    {
        if (dungeon?.combat_sequence?.combats == null || dungeon.combat_sequence.combats.Count == 0)
            return null;

        var lastCombat = dungeon.combat_sequence.combats[dungeon.combat_sequence.combats.Count - 1];

        if (lastCombat.rounds == null || lastCombat.rounds.Count == 0)
            return null;

        return lastCombat.rounds[lastCombat.rounds.Count - 1];
    }

    /// <summary>
    /// 获取地下城战斗序列中最后一场战斗
    /// </summary>
    /// <param name="dungeon">地下城对象</param>
    /// <returns>如果存在则返回最后一场战斗，否则返回 null</returns>
    public static Combat GetCurrentCombat(Dungeon dungeon)
    {
        if (dungeon?.combat_sequence?.combats == null || dungeon.combat_sequence.combats.Count == 0)
            return null;

        return dungeon.combat_sequence.combats[dungeon.combat_sequence.combats.Count - 1];
    }

    /// <summary>
    /// 获取地下城最后一场战斗的结果
    /// 从战斗序列中获取最后一个战斗对象的结果状态
    /// </summary>
    /// <param name="dungeon">地下城对象</param>
    /// <returns>返回最后一场战斗的结果（WIN/LOSE/NONE），如果没有战斗则返回 CombatResult.NONE</returns>
    public static CombatResult GetLastCombatResult(Dungeon dungeon)
    {
        if (dungeon?.combat_sequence?.combats == null || dungeon.combat_sequence.combats.Count == 0)
            return CombatResult.NONE;

        var lastCombat = dungeon.combat_sequence.combats[dungeon.combat_sequence.combats.Count - 1];
        return lastCombat.result;
    }

    /// <summary>
    /// 判断地下城最后一场战斗是否胜利
    /// 严格判断最后一场战斗的结果是否为 WIN 状态
    /// </summary>
    /// <param name="dungeon">地下城对象</param>
    /// <returns>如果最后一场战斗胜利（result == CombatResult.WIN）返回 true，否则返回 false</returns>
    public static bool IsLastCombatWin(Dungeon dungeon)
    {
        return GetLastCombatResult(dungeon) == CombatResult.WIN;
    }

    /// <summary>
    /// 判断地下城最后一场战斗是否失败
    /// 严格判断最后一场战斗的结果是否为 LOSE 状态
    /// </summary>
    /// <param name="dungeon">地下城对象</param>
    /// <returns>如果最后一场战斗失败（result == CombatResult.LOSE）返回 true，否则返回 false</returns>
    public static bool IsLastCombatLose(Dungeon dungeon)
    {
        return GetLastCombatResult(dungeon) == CombatResult.LOSE;
    }
}