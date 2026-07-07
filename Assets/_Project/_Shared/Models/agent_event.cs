// 对应 Python models/agent_event.py
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum EventType
{
    NONE = 0,
    SPEAK = 1,
    WHISPER = 2,
    ANNOUNCE = 3,
    MIND = 4,
    QUERY = 5,
    TRANS_STAGE = 6,
    COMBAT_INITIATION = 7,
    COMBAT_ARBITRATION = 8,
    COMBAT_ARCHIVE = 9,
    APPEARANCE_UPDATE = 10,
}

[JsonConverter(typeof(AnyAgentEventConverter))]
public class AgentEvent
{
    public int type = (int)EventType.NONE;
    public string message = "";
}

public sealed class SpeakEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string target = "";
    public string content = "";
}

public sealed class WhisperEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string target = "";
    public string content = "";
}

public sealed class AnnounceEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string content = "";
}

public sealed class MindEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string content = "";
}

public sealed class QueryEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string question = "";
}

public sealed class TransStageEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string target = "";
}

public sealed class CombatInitiationEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
}

public sealed class CombatArbitrationEvent : AgentEvent
{
    public string stage = "";
    public string combat_log = "";
    public string narrative = "";
}

public sealed class CombatArchiveEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string summary = "";
}

public sealed class AppearanceUpdateEvent : AgentEvent
{
    public string actor = "";
    public string stage = "";
    public string appearance = "";
}

// AnyAgentEvent 判别联合类型转换器（对应 Python AnyAgentEvent discriminator="type"）
public class AnyAgentEventConverter : JsonConverter<AgentEvent>
{
    public override AgentEvent ReadJson(JsonReader reader, Type objectType, AgentEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject jo = JObject.Load(reader);
        int typeValue = jo["type"]?.ToObject<int>() ?? (int)EventType.NONE;
        AgentEvent evt = (EventType)typeValue switch
        {
            EventType.SPEAK => new SpeakEvent(),
            EventType.WHISPER => new WhisperEvent(),
            EventType.ANNOUNCE => new AnnounceEvent(),
            EventType.MIND => new MindEvent(),
            EventType.QUERY => new QueryEvent(),
            EventType.TRANS_STAGE => new TransStageEvent(),
            EventType.COMBAT_INITIATION => new CombatInitiationEvent(),
            EventType.COMBAT_ARBITRATION => new CombatArbitrationEvent(),
            EventType.COMBAT_ARCHIVE => new CombatArchiveEvent(),
            EventType.APPEARANCE_UPDATE => new AppearanceUpdateEvent(),
            _ => new AgentEvent()
        };
        // 禁用此 converter 以避免递归，直接 populate
        using (var subReader = jo.CreateReader())
        {
            var subSerializer = new JsonSerializer();
            foreach (var converter in serializer.Converters)
            {
                if (converter is not AnyAgentEventConverter)
                    subSerializer.Converters.Add(converter);
            }
            subSerializer.Populate(subReader, evt);
        }
        return evt;
    }

    public override void WriteJson(JsonWriter writer, AgentEvent value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        var jo = JObject.FromObject(value, JsonSerializer.CreateDefault());
        jo.WriteTo(writer);
    }
}
