// 对应 Python models/artifacts.py
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum ArtifactTag
{
    [EnumMember(Value = "post_arbitration")] POST_ARBITRATION,
}

public sealed class Artifact
{
    public string name = "";
    public string description = "";
    public List<string> modifiers = new();
    public List<ArtifactTag> tags = new();
    public string source = "";
    public string uuid = System.Guid.NewGuid().ToString();
}
