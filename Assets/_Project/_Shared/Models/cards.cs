// 对应 Python models/card.py
using System.Collections.Generic;

public sealed class Card
{
    public string name = "";
    public string description = "";
    public List<string> on_play_affixes = new();      // 即时词缀；本卡被打出时结算
    public List<string> on_hit_affixes = new();       // 受击词缀；持有者被本次出牌命中时触发
    public List<string> on_turn_end_affixes = new();  // 回合结束词缀；持有者每次 pass turn 结算一次
    public bool playable = true;
    public bool exhaust = false;
    public bool retain = false;
    public bool ethereal = false;
    public bool transferable = false;
    public int cost = 1;
    public int damage = 0;
    public int hit_count = 1;
    public int block = 0;
    public TargetType target_type = TargetType.SINGLE;
    public bool self_target = false;
    public string source = "";
    public string uuid = System.Guid.NewGuid().ToString();
}
