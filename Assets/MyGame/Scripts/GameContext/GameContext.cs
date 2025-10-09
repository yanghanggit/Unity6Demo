
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

    private URLConfigResponse _urlConfig = new URLConfigResponse();

    private bool _setupGame = false;

    public bool SetupGame
    {
        get { return _setupGame; }
        set { _setupGame = value; }
    }

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

    public URLConfigResponse URLConfig
    {
        get
        {
            return _urlConfig;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("APIEndpointConfiguration is null");
                return;
            }
            _urlConfig = value;
        }
    }

    public string LOGIN_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("LOGIN_URL") ? _urlConfig.endpoints["LOGIN_URL"] : "";
        }
    }

    public string LOGOUT_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("LOGOUT_URL") ? _urlConfig.endpoints["LOGOUT_URL"] : "";
        }
    }

    public string HOME_GAMEPLAY_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("HOME_GAMEPLAY_URL") ? _urlConfig.endpoints["HOME_GAMEPLAY_URL"] : "";
        }
    }

    public string HOME_STATE_URL
    {
        get
        {
            if (_urlConfig.endpoints.ContainsKey("HOME_STATE_URL"))
            {
                var baseUrl = _urlConfig.endpoints["HOME_STATE_URL"];
                return $"{baseUrl}{UserName}/{GameName}/state";
            }
            else
            {
                return "";
            }
        }
    }

    public string DUNGEON_STATE_URL
    {
        get
        {
            if (_urlConfig.endpoints.ContainsKey("DUNGEON_STATE_URL"))
            {
                var baseUrl = _urlConfig.endpoints["DUNGEON_STATE_URL"];
                return $"{baseUrl}{UserName}/{GameName}/state";
            }
            else
            {
                return "";
            }
        }
    }

    public string ACTOR_DETAILS_URL
    {
        get
        {
            if (_urlConfig.endpoints.ContainsKey("ACTOR_DETAILS_URL"))
            {
                var baseUrl = _urlConfig.endpoints["ACTOR_DETAILS_URL"];
                return $"{baseUrl}{UserName}/{GameName}/details";
            }
            else
            {
                return "";
            }
        }
    }

    public string START_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("START_URL") ? _urlConfig.endpoints["START_URL"] : "";
        }
    }

    public string HOME_TRANS_DUNGEON_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("HOME_TRANS_DUNGEON_URL") ? _urlConfig.endpoints["HOME_TRANS_DUNGEON_URL"] : "";
        }
    }

    public string DUNGEON_GAMEPLAY_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("DUNGEON_GAMEPLAY_URL") ? _urlConfig.endpoints["DUNGEON_GAMEPLAY_URL"] : "";
        }
    }

    public string DUNGEON_TRANS_HOME_URL
    {
        get
        {
            return _urlConfig.endpoints.ContainsKey("DUNGEON_TRANS_HOME_URL") ? _urlConfig.endpoints["DUNGEON_TRANS_HOME_URL"] : "";
        }
    }
}