/**
* GameContext class to manage game state and API endpoint configuration.
*/
public class ClientMessageHead
{
    public const int NONE = 0;
    public const int AGENT_EVENT = 1;
}

/**
* GameContext class to manage game state and API endpoint configuration.
*/
[System.Serializable]
public class ClientMessage
{
    public int head = ClientMessageHead.NONE;
    public string body = "";
}