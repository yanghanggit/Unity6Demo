using System;
using System.Collections.Generic;
using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using UnityEngine;


public partial class GameContext
{
    /// <summary>
    /// 按轮次累积的代理事件历史记录
    /// 每次调用 CollectEventsByActor 会添加一个新的字典到此列表中
    /// 通过 AgentEventsCollection 属性可以获取合并后的所有历史数据
    /// </summary>
    private readonly List<Dictionary<string, List<AgentEvent>>> _agentEventsHistory = new();


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
    /// 处理流程:
    /// 1. 创建新的轮次字典并添加到 _agentEventsHistory
    /// 2. 遍历会话消息列表，过滤出 AGENT_EVENT 类型消息
    /// 3. 使用 GameUtils.ParseAgentEvent 解析每条消息
    /// 4. 调用 AddEventToActorCollection 将解析后的事件按角色分类存储
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

        // 测试下！！！！！
        foreach (var kvp in AgentEventsHistory)
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
    /// 将已解析的代理事件添加到指定角色的事件集合中
    /// 根据事件类型(head)进行分类处理，将事件添加到对应角色的事件列表中
    /// 支持的事件类型: SPEAK_EVENT, WHISPER_EVENT, ANNOUNCE_EVENT, MIND_EVENT, TRANS_STAGE_EVENT
    /// 处理流程:
    /// 1. 根据 agentEvent.head 判断事件类型
    /// 2. 将 AgentEvent 转换为具体的事件类型(SpeakEvent, WhisperEvent 等)
    /// 3. 提取事件中的 actor 字段
    /// 4. 调用 AddEventToActor 将事件添加到该角色的事件列表中
    /// </summary>
    /// <param name="agentEvent">已解析的代理事件对象</param>
    /// <param name="agentEventsByActor">按角色分组的事件集合字典</param>
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