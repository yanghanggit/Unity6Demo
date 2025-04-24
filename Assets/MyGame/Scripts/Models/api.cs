using System.Collections.Generic;

/**
 * GameContext class to manage game state and API endpoint configuration.
 */
[System.Serializable]
public class APIEndpointConfiguration
{
    public string TEST_URL = "";
    public string LOGIN_URL = "";
    public string LOGOUT_URL = "";
    public string START_URL = "";
    public string HOME_GAMEPLAY_URL = "";
    public string HOME_TRANS_DUNGEON_URL = "";
    public string DUNGEON_GAMEPLAY_URL = "";
    public string DUNGEON_TRANS_HOME_URL = "";
    public string VIEW_HOME_URL = "";
    public string VIEW_DUNGEON_URL = "";
    public string VIEW_ACTOR_URL = "";
}

[System.Serializable]
public class APIEndpointConfigurationResponse
{
    public APIEndpointConfiguration api_endpoints = new APIEndpointConfiguration();
    public int error = 0;
    public string message = "";
}

/**
 * Login and Logout request/response classes.
 */
[System.Serializable]
public class LoginRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public class LoginResponse
{
    public int error = 0;
    public string message = "";
}

[System.Serializable]
public class LogoutRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public class LogoutResponse
{
    public int error = 0;
    public string message = "";
}

/**
 * Start request/response classes.
 */
[System.Serializable]
public class StartRequest
{
    public string user_name = "";
    public string game_name = "";
    public string actor_name = "";
}

[System.Serializable]
public class StartResponse
{
    public int error = 0;
    public string message = "";
}

/**
 * Player request/response classes.
 */
[System.Serializable]
public class HomeGamePlayUserInput
{
    public string tag = "";
    public Dictionary<string, string> data = new Dictionary<string, string>();
}

[System.Serializable]
public class HomeGamePlayRequest
{
    public string user_name = "";
    public string game_name = "";
    public HomeGamePlayUserInput user_input = new HomeGamePlayUserInput();
}


[System.Serializable]
public class HomeGamePlayResponse
{
    public List<ClientMessage> client_messages = new List<ClientMessage>();
    public int error = 0;
    public string message = "";
}

/**
 * Dungeon request/response classes.
 */
[System.Serializable]
public class HomeTransDungeonRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public class HomeTransDungeonResponse
{
    public int error = 0;
    public string message = "";
}

/**
*/
[System.Serializable]
public class DungeonTransHomeRequest
{
    public string user_name = "";
    public string game_name = "";
}

/**
*/
[System.Serializable]
public class DungeonTransHomeResponse
{
    public int error = 0;
    public string message = "";
}

/**
* Dungeon run request/response classes.
*/

[System.Serializable]
public class DungeonGamePlayUserInput
{
    public string tag = "";

    public Dictionary<string, string> data = new Dictionary<string, string>();
}


[System.Serializable]
public class DungeonGamePlayRequest
{
    public string user_name = "";
    public string game_name = "";
    public DungeonGamePlayUserInput user_input = new DungeonGamePlayUserInput();
}

[System.Serializable]
public class DungeonGamePlayResponse
{
    public List<ClientMessage> client_messages = new List<ClientMessage>();
    public int error = 0;
    public string message = "";
}

/** * 
Dungeon view request/response classes.
 */
[System.Serializable]
public class ViewDungeonResponse
{
    public Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
    public Dungeon dungeon = new Dungeon();
    public int error = 0;
    public string message = "";
}

/**
 * Home view request/response classes.
 */
[System.Serializable]
public class ViewHomeResponse
{
    public Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
    public int error = 0;
    public string message = "";
}

/**
 * Actor view request/response classes.
 */

[System.Serializable]
public class ViewActorResponse
{
    public List<EntitySnapshot> actor_snapshots = new List<EntitySnapshot>();
    public int error = 0;
    public string message = "";
}