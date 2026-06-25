// 对应 Python models/serialization.py
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// data 是任意 JSON 对象（对应 Python Dict[str, Any]），由 Newtonsoft.Json 处理，不需要 Unity 内置序列化
public sealed class ComponentSerialization
{
    public string name = "";
    public JObject data = new();
}

public sealed class EntitySerialization
{
    public string name = "";
    public List<ComponentSerialization> components = new();
}
