// 对应 Python models/combat.py
using System.Collections.Generic;

public enum CombatState
{
    NONE = 0,
    INITIALIZATION = 1,
    ONGOING = 2,
    COMPLETE = 3,
    POST_COMBAT = 4,
}

public enum CombatResult
{
    NONE = 0,
    WIN = 1,
    LOSE = 2,
}

public sealed class Round
{
    public List<string> completed_actors = new List<string>();
    public List<List<string>> actor_order_snapshots = new List<List<string>>();
    public string current_turn_actor_name = null;
    public bool is_completed = false;
    public bool draw_completed = false;
    public List<string> cards_combat_log = new List<string>();
    public List<string> cards_narrative = new List<string>();
    public List<string> consumable_combat_log = new List<string>();
    public List<string> consumable_narrative = new List<string>();
    public int consumable_use_count = 0;
    public List<string> gear_combat_log = new List<string>();
    public List<string> gear_narrative = new List<string>();
    public int gear_use_count = 0;
}

public sealed class Combat
{
    public string name = "";
    public CombatState state = CombatState.NONE;
    public CombatResult result = CombatResult.NONE;
    public List<Round> rounds = new List<Round>();
    public bool retreated = false;
}
