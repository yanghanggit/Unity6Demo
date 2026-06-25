// 对应 Python models/task.py
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum TaskStatus
{
    [EnumMember(Value = "running")] RUNNING,
    [EnumMember(Value = "completed")] COMPLETED,
    [EnumMember(Value = "failed")] FAILED,
}

[System.Serializable]
public sealed class TaskRecord
{
    public string task_id = "";
    public TaskStatus status = TaskStatus.RUNNING;
    public string start_time = "";
    public string end_time = null;
    public string error = null;
}
