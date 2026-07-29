// 对应 Python models/messages.py
// 自定义消息类型（仿 langchain 风格）；ContextMessage = Union[SystemMessage, HumanMessage, AIMessage, ToolMessage]（discriminator="type"）
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(AnyContextMessageConverter))]
public class BaseMessage
{
    public string type = "";
    public string content = "";
    public JObject additional_kwargs = new(); // LLM 响应的结构化附属数据（目前专用于存 reasoning_content）
}

public sealed class SystemMessage : BaseMessage
{
    public SystemMessage() { type = "system"; }
}

public sealed class HumanMessage : BaseMessage
{
    public HumanMessage() { type = "human"; }
}

public sealed class AIMessage : BaseMessage
{
    public AIMessage() { type = "ai"; }
}

public sealed class ToolMessage : BaseMessage
{
    public string tool_call_id = ""; // 对应 LLM 发出的 ToolCall.id
    public ToolMessage() { type = "tool"; }
}

// ContextMessage 判别联合类型转换器（对应 Python Union[SystemMessage, HumanMessage, AIMessage, ToolMessage] discriminator="type"）
public class AnyContextMessageConverter : JsonConverter<BaseMessage>
{
    public override BaseMessage ReadJson(JsonReader reader, Type objectType, BaseMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string typeStr = jo["type"]?.ToString();
        BaseMessage msg = typeStr switch
        {
            "system" => new SystemMessage(),
            "human" => new HumanMessage(),
            "ai" => new AIMessage(),
            "tool" => new ToolMessage(),
            _ => new BaseMessage()
        };
        // 禁用此 converter 以避免递归，直接 populate
        using (var subReader = jo.CreateReader())
        {
            var subSerializer = new JsonSerializer();
            foreach (var converter in serializer.Converters)
            {
                if (converter is not AnyContextMessageConverter)
                    subSerializer.Converters.Add(converter);
            }
            subSerializer.Populate(subReader, msg);
        }
        return msg;
    }

    public override void WriteJson(JsonWriter writer, BaseMessage value, JsonSerializer serializer)
    {
        var jo = JObject.FromObject(value, JsonSerializer.CreateDefault());
        jo.WriteTo(writer);
    }
}

// List<BaseMessage> (ContextMessage) 列表转换器（处理列表中的多态元素，用于 AgentContext.context）
public class AnyContextMessageListConverter : JsonConverter<List<BaseMessage>>
{
    private static readonly AnyContextMessageConverter _messageConverter = new();

    public override List<BaseMessage> ReadJson(JsonReader reader, Type objectType, List<BaseMessage> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var list = new List<BaseMessage>();
        foreach (var token in array)
        {
            using var tokenReader = token.CreateReader();
            list.Add(_messageConverter.ReadJson(tokenReader, typeof(BaseMessage), null, false, serializer));
        }
        return list;
    }

    public override void WriteJson(JsonWriter writer, List<BaseMessage> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
