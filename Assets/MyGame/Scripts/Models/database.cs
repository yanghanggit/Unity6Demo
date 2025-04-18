

using System.Collections.Generic;

/**
 * 
 */
[System.Serializable]
public class ActorCharacterSheet
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
public class StageCharacterSheet
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
    public Dictionary<string, ActorCharacterSheet> actors = new Dictionary<string, ActorCharacterSheet>();
    public Dictionary<string, StageCharacterSheet> stages = new Dictionary<string, StageCharacterSheet>();
}