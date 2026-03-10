using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


public partial class GameContext
{

    /// <summary>
    /// 当前游戏阶段-角色状态映射关系字典，键为阶段名称，值为该阶段包含的角色列表。
    /// 通过 <see cref="StagesState"/> 可获取当前最新的完整映射关系。
    /// </summary>
    public Dictionary<string, List<string>> StagesState { get; set; } = new Dictionary<string, List<string>>();


    /// <summary>
    /// 按轮次累积的代理事件历史记录，每个元素对应一轮的「角色 → 事件列表」映射。
    /// 通过 <see cref="GetAgentEventsHistory"/> 可获取合并后的全量数据。
    /// </summary>
    private readonly List<Dictionary<string, List<AgentEvent>>> _agentEventsHistory = new();

    /// <summary>
    /// 合并历史所有轮次的代理事件，返回按角色分组的总展开字典。
    /// </summary>
    private Dictionary<string, List<AgentEvent>> GetAgentEventsHistory()
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

    /// <summary>
    /// 获取指定角色在最近一轮中的事件列表。
    /// 倒序遍历用于历史，返回最新轮次中包含该角色的数据。
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>该角色最近一轮的事件列表，未找到则返回空列表</returns>
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
    /// 从会话消息列表中解析代理事件，按角色分组后作为新轮次累积到 _agentEventsHistory。
    /// 每次调用均为一个独立轮次。
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    public void CollectEventsByActor(List<SessionMessage> sessionMessages)
    {
        // 为 _agentEventsHistory 添加一个新的轮次字典
        var agentEventsByActor = new Dictionary<string, List<AgentEvent>>();
        _agentEventsHistory.Add(agentEventsByActor);

        // 遍历会话消息，解析并收集代理事件
        for (int i = 0; i < sessionMessages.Count; i++)
        {
            SessionMessage sessionMessage = sessionMessages[i];
            Debug.Log("sessionMessage = " + JsonConvert.SerializeObject(sessionMessage));
            if (sessionMessage.message_type != (int)MessageType.AGENT_EVENT)
            {
                Debug.LogWarning($"Skipping non-agent event message, type: {sessionMessage.message_type}");
                continue;
            }

            var agentEvent = GameUtils.ParseAgentEvent(sessionMessage);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse agent event from session message");
                continue;
            }

            AddEventToActorCollection(agentEvent, agentEventsByActor);
        }

        // 调试：输出当前合并历史所有事件
        var agentEventsHistory = GetAgentEventsHistory();
        foreach (var kvp in agentEventsHistory)
        {
            string actor = kvp.Key;
            List<AgentEvent> events = kvp.Value;
            Debug.Log($"Actor: {actor}, Events Count: {events.Count}");
            for (int i = 0; i < events.Count; i++)
            {
                AgentEvent agentEvent = events[i];
                try
                {
                    // 直接将 AgentEvent 序列化为 JSON 字符串
                    string jsonString = JsonConvert.SerializeObject(agentEvent, Formatting.Indented);
                    Debug.Log($"Actor: {actor}, Event[{i}] JSON:\n{jsonString}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to serialize Actor: {actor}, Event[{i}] to JSON: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 将事件添加到按角色分组的事件字典中，若角色不存在则自动创建列表。
    /// </summary>
    /// <param name="collection">目标事件集合字典</param>
    /// <param name="actorName">角色名称</param>
    /// <param name="agentEvent">要添加的事件</param>
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
    /// 根据事件类型将已解析的事件识别所属角色，并存入对应的事件集合中。
    /// 支持类型：SPEAK、WHISPER、ANNOUNCE、MIND、TRANS_STAGE。
    /// </summary>
    /// <param name="agentEvent">已解析的代理事件对象</param>
    /// <param name="agentEventsByActor">按角色分组的目标事件字典</param>
    private void AddEventToActorCollection(AgentEvent agentEvent, Dictionary<string, List<AgentEvent>> agentEventsByActor)
    {
        // 分类解析
        switch ((EventHead)agentEvent.head)
        {
            case EventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = (SpeakEvent)agentEvent;
                AddEventToActor(agentEventsByActor, speakEvent.actor, speakEvent);
                break;

            case EventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = (WhisperEvent)agentEvent;
                AddEventToActor(agentEventsByActor, whisperEvent.actor, whisperEvent);
                break;

            case EventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = (AnnounceEvent)agentEvent;
                AddEventToActor(agentEventsByActor, announceEvent.actor, announceEvent);
                break;

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = (MindEvent)agentEvent;
                AddEventToActor(agentEventsByActor, mindVoiceEvent.actor, mindVoiceEvent);
                break;

            case EventHead.TRANS_STAGE_EVENT:
                TransStageEvent transStageEvent = (TransStageEvent)agentEvent;
                AddEventToActor(agentEventsByActor, transStageEvent.actor, transStageEvent);
                break;

            default:
                Debug.LogWarning("Unknown agent event head: " + agentEvent.head);
                break;
        }
    }
}