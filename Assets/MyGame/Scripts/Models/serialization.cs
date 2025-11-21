
using System.Collections.Generic;

/**
* GameContext class to manage game state and API endpoint configuration.
*/
[System.Serializable]
public class ComponentSerialization
{
    public string name = "";
    public Dictionary<string, object> data = new Dictionary<string, object>();
}

[System.Serializable]
public class EntitySerialization
{
    public string name = "";
    public List<ComponentSerialization> components = new List<ComponentSerialization>();
}