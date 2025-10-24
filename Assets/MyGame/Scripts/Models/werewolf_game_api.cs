using System.Collections.Generic;


[System.Serializable]
public class WerewolfGameStartRequest
{
    public string user_name;
    public string game_name;
}

[System.Serializable]
public class WerewolfGameStartResponse
{
    public string message;
}

[System.Serializable]
public class WerewolfGamePlayRequest
{
    public string user_name;
    public string game_name;
    public Dictionary<string, string> data;
}

[System.Serializable]
public class WerewolfGamePlayResponse
{
    public List<SessionMessage> session_messages;
}

[System.Serializable]
public class WerewolfGameStateResponse
{
    //public Dictionary<string, List<string>> mapping;

    public int game_time;
    public string victory_condition;
}

// [System.Serializable]
// public class WerewolfGameActorDetailsResponse
// {
//     public List<EntitySerialization> actor_entities_serialization;
// }