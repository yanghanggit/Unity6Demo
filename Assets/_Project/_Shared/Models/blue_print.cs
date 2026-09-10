// 对应 Python models/blueprint.py
using System.Collections.Generic;

// 生成世界的根文件，就是世界的起点
public sealed class Blueprint
{
    public string name = "";
    public string player_actor = "";
    public string campaign_setting = "";
    public string system_rules = "";
    public Dictionary<string, List<string>> knowledge_base = new(); // 蓝图关联的 RAG 知识库（按分类组织）
    public List<Stage> stages = new();
    public List<World> world_entities = new();
}
