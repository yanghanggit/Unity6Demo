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
    public List<string> completed_actors = new();
    public List<string> action_order = new();
    public string current_actor = null;
    public bool is_completed = false;
    public bool draw_completed = false;
    public List<string> cards_log = new();
    public List<string> cards_narrative = new();
    public List<string> consumable_log = new();
    public List<string> consumable_narrative = new();
    public int consumable_use_count = 0;
    public List<string> gear_log = new();
    public List<string> gear_narrative = new();
    public int gear_equip_count = 0;
    public List<string> artifact_log = new();
    public List<string> artifact_narrative = new();
}

public sealed class Combat
{
    public string name = "";
    public CombatState state = CombatState.NONE;
    public CombatResult result = CombatResult.NONE;
    public List<Round> rounds = new();
    public bool retreated = false;

    // 对应 Python Combat 上的只读属性
    public bool is_ongoing => state == CombatState.ONGOING;
    public bool is_combat_completed => state == CombatState.COMPLETE;
    public bool is_initializing => state == CombatState.INITIALIZATION;
    public bool is_post_combat => state == CombatState.POST_COMBAT;
    public bool is_won => result == CombatResult.WIN;
    public bool is_lost => result == CombatResult.LOSE;
    public Round latest_round => rounds.Count == 0 ? null : rounds[rounds.Count - 1];
}
