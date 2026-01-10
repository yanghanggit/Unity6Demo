
using System.Collections.Generic;

[System.Serializable]
public class Blueprint
{
    public string name = "";
    public string player_actor = "";
    public string player_only_stage = "";
    public string campaign_setting = "";
    public List<Stage> stages = new();
    public List<WorldSystem> world_systems = new();
}