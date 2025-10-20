
public partial class GameContext
{
    private static GameContext _instance;

    public static GameContext Instance
    {
        get
        {
            lock (lockObj)
            {
                if (_instance == null)
                {
                    _instance = new GameContext();
                }
                return _instance;
            }
        }
    }

    private GameContext()
    {
    }

    private static readonly object lockObj = new object();

    private string _userName = "";

    private string _gameName = "";

    private string _actorName = "";

    private RootResponse _rootResponse = new RootResponse();

    // private bool _setupGame = false;

    // public bool SetupGame
    // {
    //     get { return _setupGame; }
    //     set { _setupGame = value; }
    // }

    public string UserName
    {
        get
        {
            return _userName;
        }
        set
        {
            _userName = value;
        }
    }

    public string GameName
    {
        get
        {
            return _gameName;
        }
        set
        {
            _gameName = value;
        }
    }

    public string ActorName
    {
        get
        {
            return _actorName;
        }
        set
        {
            _actorName = value;
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

            // 验证关键字段
            UnityEngine.Debug.Assert(_rootResponse.endpoints != null, "endpoints is null");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("login"), "endpoints does not contain login");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("logout"), "endpoints does not contain logout");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("home_gameplay"), "endpoints does not contain home_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("home_state"), "endpoints does not contain home_state");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_state"), "endpoints does not contain dungeon_state");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("actor_details"), "endpoints does not contain actor_details");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("start"), "endpoints does not contain start");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("home_trans_dungeon"), "endpoints does not contain home_trans_dungeon");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_gameplay"), "endpoints does not contain dungeon_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("dungeon_trans_home"), "endpoints does not contain dungeon_trans_home");
        }
    }

    public string LoginUrl
    {
        get
        {
            return _rootResponse.endpoints["login"];
        }
    }

    public string LogoutUrl
    {
        get
        {
            return _rootResponse.endpoints["logout"];
        }
    }

    public string HomeGameplayUrl
    {
        get
        {
            return _rootResponse.endpoints["home_gameplay"];
        }
    }

    public string HomeStateUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["home_state"];
            return $"{baseUrl}{UserName}/{GameName}/state";
        }
    }

    public string DungeonStateUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["dungeon_state"];
            return $"{baseUrl}{UserName}/{GameName}/state";
        }
    }

    public string ActorDetailsUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["actor_details"];
            return $"{baseUrl}{UserName}/{GameName}/details";
        }
    }

    public string StartUrl
    {
        get
        {
            return _rootResponse.endpoints["start"];
        }
    }

    public string HomeTransDungeonUrl
    {
        get
        {
            return _rootResponse.endpoints["home_trans_dungeon"];
        }
    }

    public string DungeonGameplayUrl
    {
        get
        {
            return _rootResponse.endpoints["dungeon_gameplay"];
        }
    }

    public string DungeonTransHomeUrl
    {
        get
        {
            return _rootResponse.endpoints["dungeon_trans_home"];
        }
    }
}