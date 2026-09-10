// 对应 Python models/task.py
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum BackgroundTaskStatus
{
    [EnumMember(Value = "running")] RUNNING,
    [EnumMember(Value = "completed")] COMPLETED,
    [EnumMember(Value = "failed")] FAILED,
}

public sealed class TaskStatusView
{
    public string job_id = "";
    public BackgroundTaskStatus status = BackgroundTaskStatus.RUNNING;
    public string error = null;
}
