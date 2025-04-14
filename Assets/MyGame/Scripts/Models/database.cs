

using System.Collections.Generic;

/**
 * 
 */
[System.Serializable]
public class ActorPrototype
{
    public string name = "";
    public string type = "";
    public string profile = "";
    public string appearance = "";
}

/**
 * 
 */
[System.Serializable]
public class StagePrototype
{
    public string name = "";
    public string type = "";
    public string profile = "";
}

/**
 * 
 */
[System.Serializable]
public class DataBase
{
    public Dictionary<string, ActorPrototype> actors = new Dictionary<string, ActorPrototype>();
    public Dictionary<string, StagePrototype> stages = new Dictionary<string, StagePrototype>();
}