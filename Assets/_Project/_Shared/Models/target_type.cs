// 对应 Python models/target_type.py
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum TargetType
{
    [EnumMember(Value = "single")] SINGLE,
    [EnumMember(Value = "all")] ALL,
    [EnumMember(Value = "spread")] SPREAD,
    [EnumMember(Value = "self")] SELF,
}
