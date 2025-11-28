using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// 垃圾先往这里放
public partial class GameContext
{
    // 临时写死。
    public const string CampName = "场景.安全屋";
    public const string RestaurantName = "场景.餐馆";
    // public const string WarriorName = "角色.战士.卡恩";
    // public const string WizardName = "角色.法师.奥露娜";

    private List<string> _agentEventLogs = new List<string>();

    private List<AgentEvent> _agentEvents = new List<AgentEvent>();

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
                UnityEngine.Debug.LogError("AgentEvents is null");
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
            UnityEngine.Debug.Log("clientMessage = " + JsonConvert.SerializeObject(clientMessage));

            switch (clientMessage.message_type)
            {
                case (int)MessageType.AGENT_EVENT:
                    JToken dataToken = JToken.FromObject(clientMessage.data);
                    //AgentEvent agentEventMessage = dataToken.ToObject<AgentEvent>();
                    HandleAgentEventMessage(dataToken);
                    //AgentEvents.Add(agentEventMessage);
                    break;

                default:
                    UnityEngine.Debug.LogWarning("Unknown client message type: " + clientMessage.message_type);
                    break;
            }
        }
    }

    private void HandleAgentEventMessage(JToken dataToken)
    {
        //string body = dataToken.ToString();
        UnityEngine.Debug.Log("body = " + dataToken.ToString());

        var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
        switch ((EventHead)eventHead)
        {
            case EventHead.NONE:
                var message = dataToken["message"]?.ToString() ?? "No message";
                UnityEngine.Debug.Log("NONE: " + message);
                AgentEventLogs.Add(message);
                AgentEvents.Add(dataToken.ToObject<AgentEvent>());
                break;

            case EventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = dataToken.ToObject<SpeakEvent>();
                UnityEngine.Debug.Log($"SPEAK_EVENT: {speakEvent.actor} => {speakEvent.target}: {speakEvent.content}");
                AgentEventLogs.Add($"{speakEvent.actor} : @{speakEvent.target} {speakEvent.content}");
                AgentEvents.Add(speakEvent);
                break;

            case EventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = dataToken.ToObject<WhisperEvent>();
                UnityEngine.Debug.Log($"WHISPER_EVENT: {whisperEvent.actor} => {whisperEvent.target}: {whisperEvent.content}");
                AgentEventLogs.Add($"{whisperEvent.actor} : ......{whisperEvent.target} {whisperEvent.content}");
                AgentEvents.Add(whisperEvent);
                break;

            case EventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = dataToken.ToObject<AnnounceEvent>();
                UnityEngine.Debug.Log($"ANNOUNCE_EVENT: {announceEvent.actor} from {announceEvent.stage}: {announceEvent.content}");
                AgentEventLogs.Add($"{announceEvent.actor}({announceEvent.stage}) : !!{announceEvent.content}");
                AgentEvents.Add(announceEvent);
                break;

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = dataToken.ToObject<MindEvent>();
                UnityEngine.Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.actor}: {mindVoiceEvent.content}");
                AgentEventLogs.Add($"{mindVoiceEvent.actor} % {mindVoiceEvent.content}");
                AgentEvents.Add(mindVoiceEvent);
                break;

            case EventHead.COMBAT_KICK_OFF_EVENT:
                CombatKickOffEvent combatKickOffEvent = dataToken.ToObject<CombatKickOffEvent>();
                UnityEngine.Debug.Log($"COMBAT_KICK_OFF_EVENT: {combatKickOffEvent.actor} => {combatKickOffEvent.description}");
                AgentEventLogs.Add($"{combatKickOffEvent.actor} => {combatKickOffEvent.description}");
                AgentEvents.Add(combatKickOffEvent);
                break;

            case EventHead.COMBAT_COMPLETE_EVENT:
                CombatCompleteEvent combatCompleteEvent = dataToken.ToObject<CombatCompleteEvent>();
                UnityEngine.Debug.Log($"COMBAT_COMPLETE_EVENT: {combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                AgentEventLogs.Add($"{combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                AgentEvents.Add(combatCompleteEvent);
                break;

            case EventHead.TRANS_STAGE_EVENT:
                AgentEvent transStageEvent = dataToken.ToObject<AgentEvent>();
                UnityEngine.Debug.Log($"TRANS_STAGE_EVENT: {transStageEvent.message}");
                AgentEventLogs.Add($"[场景转换] {transStageEvent.message}");
                AgentEvents.Add(transStageEvent);
                break;

            default:
                UnityEngine.Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }
    }

}