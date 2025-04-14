
using System.Collections.Generic;

/**
* GameContext class to manage game state and API endpoint configuration.
*/
[System.Serializable]
public class ComponentSnapshot
{
    public string name = "";
    public Dictionary<string, object> data = new Dictionary<string, object>();
}

[System.Serializable]
public class EntitySnapshot
{
    public string name = "";
    public List<ComponentSnapshot> components = new List<ComponentSnapshot>();
}