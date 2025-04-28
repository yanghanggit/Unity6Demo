
using System.Collections.Generic;

[System.Serializable]
public class BaseMessage
{
    public string content = "";
    public string type = "";
}

[System.Serializable]
public class AgentShortTermMemory
{
    public string name = "";
    public List<BaseMessage> chat_history = new List<BaseMessage>();
}