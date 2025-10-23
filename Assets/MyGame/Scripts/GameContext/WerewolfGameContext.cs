
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEditor.Build.Content;
public partial class WerewolfGameContext
{
    private static WerewolfGameContext _instance;
    
    public static WerewolfGameContext Instance
    {
        get
        {
            lock (lockObj)
            {
                if (_instance == null)
                {
                    _instance = new WerewolfGameContext();
                }
                return _instance;
            }
        }
    }

    private WerewolfGameContext()
    {
    }

    private static readonly object lockObj = new object();

    private string _userName = string.Empty;

    public string UserName
    {
        get
        {
            if (string.IsNullOrEmpty(_userName))
            {
                _userName = $"player-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
                UnityEngine.Debug.Log($"UserName not set, generated default UserName: {_userName}");
            }
            return _userName;
        }
    }

    public readonly string GameName = "Game2";

    private RootResponse _rootResponse = new RootResponse();

    private string _stageName = "";

    private List<string> _actors = new List<string>();

    private int _gameTime = 0;

    private List<EntitySerialization> _actorEntities = new List<EntitySerialization>();

    public class MessageRecord
    {
        public string Actor { get; set; }
        public string Content { get; set; }
        public MessageRecordType MessageType { get; set; }
        public string Phase { get; set; } // 新增：记录消息所属阶段（如 "kickoff", "night", "day" 等）
    }

    public enum MessageRecordType
    {
        NightActionEvent,        // 新增：NIGHT_ACTION_EVENT 类型消息（夜晚行动）
        Mind,
        Discussion
    }
    private List<MessageRecord> _messageRecords = new List<MessageRecord>();
    
    public List<MessageRecord> MessageRecords => _messageRecords;

    // 新增：当前游戏阶段
    private string _currentPhase = "";
    public string CurrentPhase
    {
        get => _currentPhase;
        set => _currentPhase = value;
    }

    private int _lastSequenceId = 0;
    public int LastSequenceId
    {
        get
        {
            return _lastSequenceId;
        }
        set
        {
            _lastSequenceId = value;
        }
    }


    public void UpdateGameState(int gameTime, List<string> actors, string stageName)
    {
        _gameTime = gameTime;
        _actors = actors;
        _stageName = stageName;
    }

    public void UpdateActorEntities(List<EntitySerialization> actorEntities)
    {
        _actorEntities = actorEntities;
        for (int i = 0; i < _actorEntities.Count; i++)
        {
            var serializer = _actorEntities[i];
            UnityEngine.Debug.Log($"Actor Entity {i}:" + JsonConvert.SerializeObject(serializer));
        }

    }

    public RootResponse Root
    {
        get
        {
            return _rootResponse;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("_rootResponse is null");
                return;
            }
            _rootResponse = value;
            UnityEngine.Debug.Assert(_rootResponse.endpoints != null, "endpoints is null");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_game_start"), "endpoints does not contain werewolf_game_start");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_gameplay"), "endpoints does not contain werewolf_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_game_state"), "endpoints does not contain werewolf_game_state");
        }
    }

    public string StartUrl
    {
        get
        {
            return _rootResponse.endpoints["werewolf_game_start"];
        }
    }


    public string ActorDetailsUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["werewolf_game_actor_details"];
            return $"{baseUrl}{UserName}/{GameName}/details";
        }
    }


    public string StateUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["werewolf_game_state"];
            return $"{baseUrl}{UserName}/{GameName}/state";
        }
    }

    public string GameplayUrl
    {
        get
        {
            return _rootResponse.endpoints["werewolf_gameplay"];
        }
    }

    public string SessionMessagesUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["session_messages"];
            return $"{baseUrl}{UserName}/{GameName}/since";
        }
    }


    public List<string> ConvertClientMessagesToText(List<SessionMessage> clientMessages, string phase = "")
    {
        List<string> processedMessages = new List<string>();

        // 如果指定了阶段，更新当前阶段
        if (!string.IsNullOrEmpty(phase))
        {
            _currentPhase = phase;
        }

        for (int i = 0; i < clientMessages.Count; i++)
        {
            SessionMessage clientMessage = clientMessages[i];
            UnityEngine.Debug.Log("clientMessage = " + JsonConvert.SerializeObject(clientMessage));

            switch (clientMessage.message_type)
            {
                case (int)MessageType.AGENT_EVENT:
                    JToken dataToken = JToken.FromObject(clientMessage.data);
                    var handledMessage = FormatAgentEventAsText(dataToken);
                    processedMessages.Add(handledMessage);
                    break;

                case (int)MessageType.GAME:
                    // 处理系统事件
                    var gameMessage = FormatGameMessageAsText(clientMessage.data);
                    processedMessages.Add(gameMessage);
                    break;

                default:
                    UnityEngine.Debug.LogWarning("Unknown client message type: " + clientMessage.message_type);
                    break;
            }
        }
        UnityEngine.Debug.Log("processedMessages = " + JsonConvert.SerializeObject(processedMessages));
        return processedMessages;
    }

    private string FormatAgentEventAsText(JToken dataToken)
    {
        UnityEngine.Debug.Log("body = " + dataToken.ToString());

        var eventHead = dataToken["head"]?.ToObject<int>() ?? -1;
        switch ((EventHead)eventHead)
        {
            case EventHead.NONE:
                var message = dataToken["message"]?.ToString() ?? "No message";
                UnityEngine.Debug.Log("NONE: " + message);
                return message;

            case EventHead.NIGHT_ACTION_EVENT:
                NightActionEvent nightActionEvent = dataToken.ToObject<NightActionEvent>();
                UnityEngine.Debug.Log($"NIGHT_ACTION_EVENT: {nightActionEvent.actor}: {nightActionEvent.message}");

                // 存储 NIGHT_ACTION_EVENT 类型消息
                _messageRecords.Add(new MessageRecord
                {
                    Actor = nightActionEvent.actor,
                    Content = nightActionEvent.message,
                    MessageType = MessageRecordType.NightActionEvent,
                    Phase = _currentPhase,
                });

                return $"{nightActionEvent.actor} performed a night action.";

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = dataToken.ToObject<MindEvent>();
                UnityEngine.Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.actor}: {mindVoiceEvent.content}");
                
                // 存储心声消息
                _messageRecords.Add(new MessageRecord
                {
                    Actor = mindVoiceEvent.actor,
                    Content = mindVoiceEvent.content,
                    MessageType = MessageRecordType.Mind,
                    Phase = _currentPhase,
                });

                return $"{mindVoiceEvent.actor}...: {mindVoiceEvent.content}";

            case EventHead.DISCUSSION_EVENT:
                DiscussionEvent discussionEvent = dataToken.ToObject<DiscussionEvent>();
                UnityEngine.Debug.Log($"DISCUSSION_EVENT: {discussionEvent.actor}: {discussionEvent.content}");
                
                // 存储讨论消息
                _messageRecords.Add(new MessageRecord
                {
                    Actor = discussionEvent.actor,
                    Content = discussionEvent.content,
                    MessageType = MessageRecordType.Discussion,
                    Phase = _currentPhase,
                });

                return $"{discussionEvent.actor} says: {discussionEvent.content}";


            default:
                UnityEngine.Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }

        return "Unknown message type";
    }

    private string FormatGameMessageAsText(object data)
    {
        try
        {
            JObject gameData = JObject.FromObject(data);
            
            // 检查是否包含 phase 和 turn_number
            if (gameData["phase"] != null && gameData["turn_number"] != null)
            {
                int turnNumber = gameData["turn_number"].ToObject<int>();
                string phase = gameData["phase"].ToString();
                
                // 计算是第几个回合（从1开始）
                int roundNumber = (turnNumber + 1) / 2;
                
                // 判断是夜晚还是白天（奇数为夜晚，偶数为白天）
                string timeOfDay = (turnNumber % 2 == 1) ? "夜晚" : "白天";
                
                // 格式化输出
                string ordinal = GetOrdinalNumber(roundNumber);
                return $"回合{turnNumber}：{ordinal}{timeOfDay}";
            }
            
            // 如果不是预期的格式，返回原始JSON
            return "[GAME]: " + JsonConvert.SerializeObject(data);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Error formatting game message: {ex.Message}");
            return "[GAME]: " + JsonConvert.SerializeObject(data);
        }
    }
    
    private string GetOrdinalNumber(int number)
    {
        return $"第{number}个";
    }
    
    // 添加辅助方法以根据条件获取消息记录
     public List<MessageRecord> GetMessagesByActor(string actorName)
    {
        return _messageRecords.Where(m => m.Actor == actorName).ToList();
    }

    // 新增：根据角色和阶段获取消息（包括该角色的所有类型消息）
    public List<MessageRecord> GetMessagesByActorAndPhase(string actorName, string phase)
    {
        // 获取该角色在该阶段的所有消息（包括 Mind、Discussion 和 None 类型）
        return _messageRecords
            .Where(m => m.Phase == phase && m.Actor == actorName)
            .ToList();
    }
    
    public void ClearMessageRecords()
    {
        _messageRecords.Clear();
    }

    public string GetActorAppearance(int actorIndex)
    {
        if (actorIndex < 0 || actorIndex >= _actorEntities.Count)
        {
            UnityEngine.Debug.LogWarning($"Invalid actor index: {actorIndex}");
            return "N/A";
        }

        var serializer = _actorEntities[actorIndex];
        JObject jObj = JObject.Parse(JsonConvert.SerializeObject(serializer));
        
        var components = jObj["components"] as JArray;
        var appearanceComponent = components?.FirstOrDefault(c => 
            c["name"]?.ToString() == "AppearanceComponent");
        
        return appearanceComponent?["data"]?["appearance"]?.ToString() ?? "N/A";
    }

    public List<string> GetAllActorAppearances()
    {
        List<string> appearances = new List<string>();
        for (int i = 0; i < _actorEntities.Count; i++)
        {
            appearances.Add(GetActorAppearance(i));
        }
        return appearances;
    }

    // 新增：获取所有 actor 名称（按实体顺序），用于消息匹配
    public List<string> GetAllActorNames()
    {
        List<string> actorNames = new List<string>();
        for (int i = 0; i < _actorEntities.Count; i++)
        {
            var serializer = _actorEntities[i];
            JObject jObj = JObject.Parse(JsonConvert.SerializeObject(serializer));
            
            // 从实体中提取 name 字段
            string actorName = jObj["name"]?.ToString() ?? "Unknown";
            actorNames.Add(actorName);
            UnityEngine.Debug.Log($"Actor Entity {i} name: {actorName}");
        }
        return actorNames;
    }
}