//using System.Collections.Generic;

public static class TaskStatus
{
    public const string RUNNING = "running";
    public const string COMPLETED = "completed";
    public const string FAILED = "failed";
}

[System.Serializable]
public class TaskRecord
{
    public string task_id = "";
    public string status = "";
    public string start_time = "";
    public string end_time = "";
    public string error = "";
}