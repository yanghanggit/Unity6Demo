
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

    private URLConfigurationResponse _urlConfiguration = new URLConfigurationResponse();

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

    public URLConfigurationResponse URLConfiguration
    {
        get
        {
            return _urlConfiguration;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("APIEndpointConfiguration is null");
                return;
            }
            _urlConfiguration = value;
        }
    }

    public string LOGIN_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("LOGIN_URL") ? _urlConfiguration.endpoints["LOGIN_URL"] : "";
        }
    }

    public string LOGOUT_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("LOGOUT_URL") ? _urlConfiguration.endpoints["LOGOUT_URL"] : "";
        }
    }

    public string HOME_GAMEPLAY_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("HOME_GAMEPLAY_URL") ? _urlConfiguration.endpoints["HOME_GAMEPLAY_URL"] : "";
        }
    }

    public string VIEW_HOME_URL
    {
        get
        {
            if (_urlConfiguration.endpoints.ContainsKey("VIEW_HOME_URL"))
            {
                var baseUrl = _urlConfiguration.endpoints["VIEW_HOME_URL"];
                return $"{baseUrl}{UserName}/{GameName}";
            }
            else
            {
                return "";
            }
        }
    }

    public string VIEW_DUNGEON_URL
    {
        get
        {
            if (_urlConfiguration.endpoints.ContainsKey("VIEW_DUNGEON_URL"))
            {
                var baseUrl = _urlConfiguration.endpoints["VIEW_DUNGEON_URL"];
                return $"{baseUrl}{UserName}/{GameName}";
            }
            else
            {
                return "";
            }
        }
    }

    public string VIEW_ACTOR_URL
    {
        get
        {
            if (_urlConfiguration.endpoints.ContainsKey("VIEW_ACTOR_URL"))
            {
                var baseUrl = _urlConfiguration.endpoints["VIEW_ACTOR_URL"];
                return $"{baseUrl}{UserName}/{GameName}";
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
            return _urlConfiguration.endpoints.ContainsKey("START_URL") ? _urlConfiguration.endpoints["START_URL"] : "";
        }
    }

    public string HOME_TRANS_DUNGEON_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("HOME_TRANS_DUNGEON_URL") ? _urlConfiguration.endpoints["HOME_TRANS_DUNGEON_URL"] : "";
        }
    }

    public string DUNGEON_GAMEPLAY_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("DUNGEON_GAMEPLAY_URL") ? _urlConfiguration.endpoints["DUNGEON_GAMEPLAY_URL"] : "";
        }
    }

    public string DUNGEON_TRANS_HOME_URL
    {
        get
        {
            return _urlConfiguration.endpoints.ContainsKey("DUNGEON_TRANS_HOME_URL") ? _urlConfiguration.endpoints["DUNGEON_TRANS_HOME_URL"] : "";
        }
    }
}