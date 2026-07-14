// 对应 Python models/cards.py
using System.Collections.Generic;

public sealed class StatusEffect
{
    public string name = "";
    public string description = "";
    public int duration = 3;
    public PhaseType phase = PhaseType.ARBITRATION;
    public int counter = 0;
    public string source = "";
    public int speed = 0;
    public int defense = 0;
    public string uuid = "";
}

public sealed class Card
{
    public string name = "";
    public string description = "";
    public List<string> affixes = new();
    public List<string> modifiers = new();
    public bool playable = true;
    public bool exhaust = false;
    public int cost = 1;
    public int damage_dealt = 0;
    public int energy_delta = 0;
    public int hit_count = 1;
    public TargetType target_type = TargetType.ENEMY_SINGLE;
    public string source = "";
    public string uuid = "";
    public Card original_data = null; // Optional[Card]
}
