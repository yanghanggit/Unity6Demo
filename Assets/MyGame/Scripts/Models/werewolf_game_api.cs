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
    public List<ClientMessage> client_messages;
}

[System.Serializable]
public class WerewolfGameStateResponse
{
    public string message;
}