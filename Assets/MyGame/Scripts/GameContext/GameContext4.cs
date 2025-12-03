using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// 垃圾先往这里放！！！
public partial class GameContext
{
    private List<string> _agentEventLogs = new();

    private List<AgentEvent> _agentEvents = new();

    public List<string> AgentEventLogs
    {
        get
        {
            return _agentEventLogs;
        }
        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("AgentEventLogs is null");
                return;
            }
            _agentEventLogs = value;
        }
    }

    public List<AgentEvent> AgentEvents
    {
        get
        {
            return _agentEvents;
        }
        set
        {
            if (value == null)
            {
                Debug.LogError("AgentEvents is null");
                return;
            }
            _agentEvents = value;
        }
    }

    public void ProcessClientMessages(List<SessionMessage> client_messages)
    {
        AgentEventLogs.Clear();
        AgentEvents.Clear();

        for (int i = 0; i < client_messages.Count; i++)
        {
            SessionMessage clientMessage = client_messages[i];
            Debug.Log("clientMessage = " + JsonConvert.SerializeObject(clientMessage));

            switch (clientMessage.message_type)
            {
                case (int)MessageType.AGENT_EVENT:
                    JToken dataToken = JToken.FromObject(clientMessage.data);
                    //AgentEvent agentEventMessage = dataToken.ToObject<AgentEvent>();
                    HandleAgentEventMessage(dataToken);
                    //AgentEvents.Add(agentEventMessage);
                    break;

                default:
                    Debug.LogWarning("Unknown client message type: " + clientMessage.message_type);
                    break;
            }
        }
    }

    private void HandleAgentEventMessage(JToken dataToken)
    {
        //string body = dataToken.ToString();
        Debug.Log("body = " + dataToken.ToString());

        var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
        switch ((EventHead)eventHead)
        {
            case EventHead.NONE:
                var message = dataToken["message"]?.ToString() ?? "No message";
                Debug.Log("NONE: " + message);
                AgentEventLogs.Add(message);
                AgentEvents.Add(dataToken.ToObject<AgentEvent>());
                break;

            case EventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = dataToken.ToObject<SpeakEvent>();
                Debug.Log($"SPEAK_EVENT: {speakEvent.actor} => {speakEvent.target}: {speakEvent.content}");
                AgentEventLogs.Add($"{speakEvent.actor} : @{speakEvent.target} {speakEvent.content}");
                AgentEvents.Add(speakEvent);
                break;

            case EventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = dataToken.ToObject<WhisperEvent>();
                Debug.Log($"WHISPER_EVENT: {whisperEvent.actor} => {whisperEvent.target}: {whisperEvent.content}");
                AgentEventLogs.Add($"{whisperEvent.actor} : ......{whisperEvent.target} {whisperEvent.content}");
                AgentEvents.Add(whisperEvent);
                break;

            case EventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = dataToken.ToObject<AnnounceEvent>();
                Debug.Log($"ANNOUNCE_EVENT: {announceEvent.actor} from {announceEvent.stage}: {announceEvent.content}");
                AgentEventLogs.Add($"{announceEvent.actor}({announceEvent.stage}) : !!{announceEvent.content}");
                AgentEvents.Add(announceEvent);
                break;

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = dataToken.ToObject<MindEvent>();
                Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.actor}: {mindVoiceEvent.content}");
                AgentEventLogs.Add($"{mindVoiceEvent.actor} % {mindVoiceEvent.content}");
                AgentEvents.Add(mindVoiceEvent);
                break;

            case EventHead.COMBAT_COMPLETE_EVENT:
                CombatCompleteEvent combatCompleteEvent = dataToken.ToObject<CombatCompleteEvent>();
                Debug.Log($"COMBAT_COMPLETE_EVENT: {combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                AgentEventLogs.Add($"{combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                AgentEvents.Add(combatCompleteEvent);
                break;

            case EventHead.TRANS_STAGE_EVENT:
                AgentEvent transStageEvent = dataToken.ToObject<AgentEvent>();
                Debug.Log($"TRANS_STAGE_EVENT: {transStageEvent.message}");
                AgentEventLogs.Add($"[场景转换] {transStageEvent.message}");
                AgentEvents.Add(transStageEvent);
                break;

            default:
                Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }
    }

}