
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
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


    public List<string> ConvertClientMessagesToText(List<SessionMessage> clientMessages)
    {
        List<string> processedMessages = new List<string>();

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
                    processedMessages.Add("[GAME]: " + JsonConvert.SerializeObject(clientMessage.data));
                    break;

                default:
                    UnityEngine.Debug.LogWarning("Unknown client message type: " + clientMessage.message_type);
                    break;
            }
        }

        return processedMessages;
    }

    public bool ShowMindEvents = false; // 添加配置字段
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

            case EventHead.MIND_EVENT:
                if (!ShowMindEvents) // 检查配置
                {
                    return string.Empty;
                }
                MindEvent mindVoiceEvent = dataToken.ToObject<MindEvent>();
                UnityEngine.Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.actor}: {mindVoiceEvent.content}");
                return $"{mindVoiceEvent.actor}...: {mindVoiceEvent.content}";

            case EventHead.DISCUSSION_EVENT:
                DiscussionEvent discussionEvent = dataToken.ToObject<DiscussionEvent>();
                UnityEngine.Debug.Log($"DISCUSSION_EVENT: {discussionEvent.actor}: {discussionEvent.content}");
                return $"{discussionEvent.actor} says: {discussionEvent.content}";


            default:
                UnityEngine.Debug.LogWarning("Unknown agent event head: " + eventHead);
                break;
        }

        return "Unknown message type";
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

}