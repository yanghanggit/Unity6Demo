// 对应 Python models/blue_print.py
using System.Collections.Generic;
using Newtonsoft.Json;

// 生成世界的根文件，就是世界的起点
public sealed class Blueprint
{
    public string name = "";
    public string player_actor = "";
    public string campaign_setting = "";
    public Dictionary<string, List<string>> knowledge_base = new(); // 蓝图关联的 RAG 知识库（按分类组织）
    public List<Stage> stages = new();
    public List<WorldSystem> world_systems = new();
    public string storage_entity = ""; // 全局储物箱实体名
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> storage = new(); // 蓝图初始储物箱道具库
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> inventory = new(); // 蓝图初始玩家背包道具库
    public List<Artifact> artifacts = new(); // 蓝图初始世界神器/古物库
}
