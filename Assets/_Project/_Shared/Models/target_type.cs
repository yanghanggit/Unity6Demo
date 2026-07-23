// 对应 Python models/target_type.py
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum TargetType
{
    [EnumMember(Value = "enemy_single")] ENEMY_SINGLE,
    [EnumMember(Value = "enemy_all")] ENEMY_ALL,
    [EnumMember(Value = "enemy_spread")] ENEMY_SPREAD,
    [EnumMember(Value = "ally_single")] ALLY_SINGLE,
    [EnumMember(Value = "ally_all")] ALLY_ALL,
    [EnumMember(Value = "self_only")] SELF_ONLY,
}
