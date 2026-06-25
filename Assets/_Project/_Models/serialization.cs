// 对应 Python models/serialization.py
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[System.Serializable]
public sealed class ComponentSerialization
{
    public string name = "";
    public JObject data = new JObject();
}

[System.Serializable]
public sealed class EntitySerialization
{
    public string name = "";
    public List<ComponentSerialization> components = new List<ComponentSerialization>();
}
