using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


public partial class GameContext
{
    /// <summary>
    /// 按轮次累积的代理事件历史记录
    /// 每次调用 CollectEventsByActor 会添加一个新的字典到此列表中
    /// 通过 AgentEventsCollection 属性可以获取合并后的所有历史数据
    /// </summary>
    private List<Dictionary<string, List<AgentEvent>>> _agentEventsHistory = new();


    /// <summary>
    /// 获取合并后的所有历史代理事件，按角色分组
    /// 会遍历 _agentEventsHistory 中的所有轮次数据并合并返回
    /// </summary>
    public Dictionary<string, List<AgentEvent>> AgentEventsHistory
    {
        get
        {
            var agentEventsByActor = new Dictionary<string, List<AgentEvent>>();
            foreach (var dict in _agentEventsHistory)
            {
                foreach (var kvp in dict)
                {
                    if (!agentEventsByActor.ContainsKey(kvp.Key))
                    {
                        agentEventsByActor[kvp.Key] = new List<AgentEvent>();
                    }
                    agentEventsByActor[kvp.Key].AddRange(kvp.Value);
                }
            }
            return agentEventsByActor;
        }
    }

    // 添加一个Getter 获取最后一次的 AgentEventsHistory
    public Dictionary<string, List<AgentEvent>> LastAgentEventsHistory
    {
        get
        {
            if (_agentEventsHistory.Count == 0)
            {
                return new Dictionary<string, List<AgentEvent>>();
            }
            return _agentEventsHistory[_agentEventsHistory.Count - 1];
        }
    }

    /// <summary>
    /// 获取指定角色在最近一轮中的事件列表
    /// 倒序遍历事件历史,返回最近一次包含该角色事件的轮次数据
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>该角色最近一轮的事件列表,如果未找到则返回空列表</returns>
    public List<AgentEvent> GetLatestRoundEventsForActor(string actorName)
    {
        for (int i = _agentEventsHistory.Count - 1; i >= 0; i--)
        {
            var dict = _agentEventsHistory[i];
            if (dict.ContainsKey(actorName))
            {
                return dict[actorName];
            }
        }
        return new List<AgentEvent>();
    }

    /// <summary>
    /// 从会话消息列表中收集代理事件，并按角色分组累积存储到 _agentEventsHistory 中
    /// 每次调用会创建一个新的字典并添加到历史记录列表中，实现数据累积
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    public void CollectEventsByActor(List<SessionMessage> sessionMessages)
    {
        // 为 _agentEventsHistory 添加一个新的轮次字典
        var agentEventsByActor = new Dictionary<string, List<AgentEvent>>();
        _agentEventsHistory.Add(agentEventsByActor);

        //
        for (int i = 0; i < sessionMessages.Count; i++)
        {
            SessionMessage sessionMessage = sessionMessages[i];
            Debug.Log("sessionMessage = " + JsonConvert.SerializeObject(sessionMessage));
            if (sessionMessage.message_type != (int)MessageType.AGENT_EVENT)
            {
                Debug.LogWarning($"Skipping non-agent event message, type: {sessionMessage.message_type}");
                continue;
            }

            // 解析数据部分
            JToken dataToken = JToken.FromObject(sessionMessage.data);
            var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
            Debug.Log("body = " + dataToken.ToString());

            if (eventHead < 0)
            {
                Debug.LogWarning("Invalid event head: " + eventHead);
                continue;
            }

            // 故意忽略 NONE 事件
            if (eventHead == (int)EventHead.NONE)
            {
                Debug.Log($"NONE event encountered, skipping processing. message = {dataToken["message"]?.ToString() ?? "No message"}");
                continue;
            }

            AddEventToActorCollection(dataToken, agentEventsByActor);
        }
    }

    /// <summary>
    /// 清除所有累积的代理事件历史记录
    /// 调用此方法会清空 _agentEventsHistory，释放内存
    /// 适用于游戏重置、场景切换等需要清空历史数据的场景
    /// </summary>
    public void ClearAgentEventsHistory()
    {
        _agentEventsHistory.Clear();
        Debug.Log("[GameContext] Agent events history cleared");
    }

    /// <summary>
    /// 安全地将事件添加到指定角色的事件列表中,如果角色不存在则自动创建列表
    /// </summary>
    /// <param name="collection">事件集合字典</param>
    /// <param name="actorName">角色名称</param>
    /// <param name="agentEvent">要添加的事件</param>
    private void AddEventToActor(Dictionary<string, List<AgentEvent>> collection, string actorName, AgentEvent agentEvent)
    {
        if (!collection.ContainsKey(actorName))
        {
            collection[actorName] = new List<AgentEvent>();
        }
        collection[actorName].Add(agentEvent);
    }

    /// <summary>
    /// 将事件数据解析并添加到指定角色的事件集合中。支持 SPEAK、WHISPER、ANNOUNCE、MIND、COMBAT_COMPLETE、TRANS_STAGE 等事件类型
    /// </summary>
    /// <param name="dataToken">事件数据的 JSON 对象</param>
    /// <param name="agentEventsByActor">按角色分组的事件集合字典</param>
    private void AddEventToActorCollection(JToken dataToken, Dictionary<string, List<AgentEvent>> agentEventsByActor)
    {
        // 获取事件头
        var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
        Debug.Assert(eventHead >= 0, "Invalid event head: " + eventHead);

        // 分类解析
        switch ((EventHead)eventHead)
        {
            case EventHead.NONE:
                Debug.Log($"NONE event encountered, skipping processing. message = {dataToken["message"]?.ToString() ?? "No message"}");
                break;

            case EventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = dataToken.ToObject<SpeakEvent>();
                AddEventToActor(agentEventsByActor, speakEvent.actor, speakEvent);
                break;

            case EventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = dataToken.ToObject<WhisperEvent>();
                AddEventToActor(agentEventsByActor, whisperEvent.actor, whisperEvent);
                break;

            case EventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = dataToken.ToObject<AnnounceEvent>();
                AddEventToActor(agentEventsByActor, announceEvent.actor, announceEvent);
                break;

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = dataToken.ToObject<MindEvent>();
                AddEventToActor(agentEventsByActor, mindVoiceEvent.actor, mindVoiceEvent);
                break;

            case EventHead.COMBAT_COMPLETE_EVENT:
                CombatCompleteEvent combatCompleteEvent = dataToken.ToObject<CombatCompleteEvent>();
                AddEventToActor(agentEventsByActor, combatCompleteEvent.actor, combatCompleteEvent);
                break;

            case EventHead.TRANS_STAGE_EVENT:
                TransStageEvent transStageEvent = dataToken.ToObject<TransStageEvent>();
                AddEventToActor(agentEventsByActor, transStageEvent.actor, transStageEvent);
                break;

            default:
                Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }
    }
}