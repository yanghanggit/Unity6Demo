using System.Collections.Generic;

/**
 * URL configuration request/response classes.
 */
[System.Serializable]
public class URLConfigurationResponse
{
    public string api_version = "";
    public Dictionary<string, string> endpoints = new Dictionary<string, string>();
    public bool deprecated = false;
    public string notice = "";
}

/**
 * Login and Logout request/response classes.
 */
[System.Serializable]
public class LoginRequest
{
    public string user_name;
    public string game_name;
}

[System.Serializable]
public class LoginResponse
{
    public string message;
}

[System.Serializable]
public class LogoutRequest
{
    public string user_name;
    public string game_name;
}

[System.Serializable]
public class LogoutResponse
{
    public string message;
}

/**
 * Start request/response classes.
 */
[System.Serializable]
public class StartRequest
{
    public string user_name;
    public string game_name;
    public string actor_name;
}

[System.Serializable]
public class StartResponse
{
    public string message;
}

/**
 * Player request/response classes.
 */
[System.Serializable]
public class HomeGamePlayUserInput
{
    public string tag;
    public Dictionary<string, string> data;
}

[System.Serializable]
public class HomeGamePlayRequest
{
    public string user_name;
    public string game_name;
    public HomeGamePlayUserInput user_input;
}


[System.Serializable]
public class HomeGamePlayResponse
{
    public List<ClientMessage> client_messages;
}

/**
 * Dungeon request/response classes.
 */
[System.Serializable]
public class HomeTransDungeonRequest
{
    public string user_name;
    public string game_name;
}

[System.Serializable]
public class HomeTransDungeonResponse
{
    public string message;
}

/**
*/
[System.Serializable]
public class DungeonTransHomeRequest
{
    public string user_name;
    public string game_name;
}

/**
*/
[System.Serializable]
public class DungeonTransHomeResponse
{
    public string message;
}

/**
* Dungeon run request/response classes.
*/

[System.Serializable]
public class DungeonGamePlayUserInput
{
    public string tag;

    public Dictionary<string, string> data;
}


[System.Serializable]
public class DungeonGamePlayRequest
{
    public string user_name;
    public string game_name;
    public DungeonGamePlayUserInput user_input;
}

[System.Serializable]
public class DungeonGamePlayResponse
{
    public List<ClientMessage> client_messages;
}

/** * 
Dungeon view request/response classes.
 */
[System.Serializable]
public class ViewDungeonResponse
{
    public Dictionary<string, List<string>> mapping;
    public Dungeon dungeon;
}

/**
 * Home view request/response classes.
 */
[System.Serializable]
public class ViewHomeResponse
{
    public Dictionary<string, List<string>> mapping;
}

/**
 * Actor view request/response classes.
 */

[System.Serializable]
public class ViewActorResponse
{
    public List<EntitySnapshot> actor_snapshots;

    public List<AgentShortTermMemory> agent_short_term_memories;
}