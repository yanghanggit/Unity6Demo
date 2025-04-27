
// using System.Collections.Generic;
// using System.Diagnostics;
// using Newtonsoft.Json;
// using UnityEngine;
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

    private APIEndpointConfiguration _apiEndpointConfiguration = new APIEndpointConfiguration();

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

    public APIEndpointConfiguration ApiEndpointConfiguration
    {
        get
        {
            return _apiEndpointConfiguration;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("APIEndpointConfiguration is null");
                return;
            }
            _apiEndpointConfiguration = value;
        }
    }

    public string TEST_URL
    {
        get
        {
            return _apiEndpointConfiguration.TEST_URL;
        }
    }

    public string LOGIN_URL
    {
        get
        {
            return _apiEndpointConfiguration.LOGIN_URL;
        }
    }

    public string LOGOUT_URL
    {
        get
        {
            return _apiEndpointConfiguration.LOGOUT_URL;
        }
    }

    public string HOME_GAMEPLAY_URL
    {
        get
        {
            return _apiEndpointConfiguration.HOME_GAMEPLAY_URL;
        }
    }

    public string VIEW_HOME_URL
    {
        get
        {
            return $"{_apiEndpointConfiguration.VIEW_HOME_URL}{UserName}/{GameName}";
        }
    }

    public string VIEW_DUNGEON_URL
    {
        get
        {
            return $"{_apiEndpointConfiguration.VIEW_DUNGEON_URL}{UserName}/{GameName}";
        }
    }

    public string VIEW_ACTOR_URL
    {
        get
        {
            return $"{_apiEndpointConfiguration.VIEW_ACTOR_URL}{UserName}/{GameName}";
        }
    }

    public string START_URL
    {
        get
        {
            return _apiEndpointConfiguration.START_URL;
        }
    }

    public string HOME_TRANS_DUNGEON_URL
    {
        get
        {
            return _apiEndpointConfiguration.HOME_TRANS_DUNGEON_URL;
        }
    }

    public string DUNGEON_GAMEPLAY_URL
    {
        get
        {
            return _apiEndpointConfiguration.DUNGEON_GAMEPLAY_URL;
        }
    }

    public string DUNGEON_TRANS_HOME_URL
    {
        get
        {
            return _apiEndpointConfiguration.DUNGEON_TRANS_HOME_URL;
        }
    }
}