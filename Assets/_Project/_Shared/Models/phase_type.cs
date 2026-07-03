// 对应 Python models/phase_type.py
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum PhaseType
{
    [EnumMember(Value = "draw")] DRAW,
    [EnumMember(Value = "arbitration")] ARBITRATION,
    [EnumMember(Value = "round_end")] ROUND_END,
}
