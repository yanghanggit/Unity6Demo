using System.Collections.Generic;
using Newtonsoft.Json;

public partial class GameContext
{
    private List<string> _agentEventLogs = new List<string>();
    
    private Dictionary<string, List<string>> _mapping = new Dictionary<string, List<string>>();
    
    private List<EntitySnapshot> _actorSnapshots = new List<EntitySnapshot>();

    private Dungeon _dungeon = new Dungeon();

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

    public Dictionary<string, List<string>> Mapping
    {
        get
        {
            return _mapping;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("Mapping is null");
                return;
            }
            _mapping = value;
        }
    }

    public Dungeon Dungeon
    {
        get
        {
            return _dungeon;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("Dungeon is null");
                return;
            }
            _dungeon = value;
        }
    }

    public List<EntitySnapshot> ActorSnapshots
    {
        get
        {
            return _actorSnapshots;
        }
        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("ActorSnapshots is null");
                return;
            }
            _actorSnapshots = value;
        }
    }

    public void ProcessClientMessages(List<ClientMessage> client_messages)
    {
        AgentEventLogs.Clear();

        for (int i = 0; i < client_messages.Count; i++)
        {
            ClientMessage clientMessage = client_messages[i];
            UnityEngine.Debug.Log("clientMessage = " + JsonConvert.SerializeObject(clientMessage));

            switch (clientMessage.head)
            {
                case ClientMessageHead.AGENT_EVENT:
                    AgentEvent agentEventMessage = JsonConvert.DeserializeObject<AgentEvent>(clientMessage.body);
                    HandleAgentEventMessage(agentEventMessage, clientMessage.body);
                    break;

                default:
                    UnityEngine.Debug.LogWarning("Unknown client message head: " + clientMessage.head);
                    break;
            }
        }
    }

    private void HandleAgentEventMessage(AgentEvent agentEvent, string body)
    {
        UnityEngine.Debug.Log("body = " + body);

        switch ((AgentEventHead)agentEvent.head)
        {
            case AgentEventHead.NONE:
                UnityEngine.Debug.Log("NONE: " + agentEvent.message);
                AgentEventLogs.Add(agentEvent.message);
                break;

            case AgentEventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = JsonConvert.DeserializeObject<SpeakEvent>(body);
                UnityEngine.Debug.Log($"SPEAK_EVENT: {speakEvent.speaker} => {speakEvent.listener}: {speakEvent.dialogue}");
                AgentEventLogs.Add($"{speakEvent.speaker} : @{speakEvent.listener} {speakEvent.dialogue}");
                break;

            case AgentEventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = JsonConvert.DeserializeObject<WhisperEvent>(body);
                UnityEngine.Debug.Log($"WHISPER_EVENT: {whisperEvent.speaker} => {whisperEvent.listener}: {whisperEvent.dialogue}");
                AgentEventLogs.Add($"{whisperEvent.speaker} : ......{whisperEvent.listener} {whisperEvent.dialogue}");
                break;

            case AgentEventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = JsonConvert.DeserializeObject<AnnounceEvent>(body);
                UnityEngine.Debug.Log($"ANNOUNCE_EVENT: {announceEvent.announcement_speaker} from {announceEvent.event_stage}: {announceEvent.announcement_message}");
                AgentEventLogs.Add($"{announceEvent.announcement_speaker}({announceEvent.event_stage}) : !!{announceEvent.announcement_message}");
                break;

            case AgentEventHead.MIND_VOICE_EVENT:
                MindVoiceEvent mindVoiceEvent = JsonConvert.DeserializeObject<MindVoiceEvent>(body);
                UnityEngine.Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.speaker}: {mindVoiceEvent.dialogue}");
                AgentEventLogs.Add($"{mindVoiceEvent.speaker} % {mindVoiceEvent.dialogue}");
                break;

            case AgentEventHead.COMBAT_KICK_OFF_EVENT:
                CombatKickOffEvent combatKickOffEvent = JsonConvert.DeserializeObject<CombatKickOffEvent>(body);
                UnityEngine.Debug.Log($"COMBAT_KICK_OFF_EVENT: {combatKickOffEvent.actor} => {combatKickOffEvent.description}");
                AgentEventLogs.Add($"{combatKickOffEvent.actor} => {combatKickOffEvent.description}");
                break;

            case AgentEventHead.COMBAT_COMPLETE_EVENT:
                CombatCompleteEvent combatCompleteEvent = JsonConvert.DeserializeObject<CombatCompleteEvent>(body);
                UnityEngine.Debug.Log($"COMBAT_COMPLETE_EVENT: {combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                AgentEventLogs.Add($"{combatCompleteEvent.actor} => {combatCompleteEvent.summary}");
                break;


            default:
                UnityEngine.Debug.LogWarning("Unknown agent event head: " + agentEvent.head);
                break;
        }
    }
}