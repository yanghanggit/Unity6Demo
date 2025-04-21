
// using System.Collections.Generic;
// using System.Diagnostics;
// using Newtonsoft.Json;
// using UnityEngine;
public partial class GameContext
{
    private static GameContext instance;

    public static GameContext Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = new GameContext();
                }
                return instance;
            }
        }
    }

    private GameContext()
    {
    }

    private static readonly object lockObj = new object();

    public string UserName = "";
    public string GameName = "";
    public string ActorName = "";

    private APIEndpointConfiguration _apiEndpointConfiguration = new APIEndpointConfiguration();


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