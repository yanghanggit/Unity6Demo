// 对应 Python models/artifacts.py
using System.Collections.Generic;

public sealed class Artifact
{
    public string name = "";
    public string description = "";
    public List<string> modifiers = new();
    public string uuid = "";
}
