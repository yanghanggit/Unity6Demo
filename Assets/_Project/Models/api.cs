// 对应 Python models/api.py
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

// ────────────────────────────────────────────────────────────────────────────────
// Login / Logout
// ────────────────────────────────────────────────────────────────────────────────

public sealed class LoginRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class LoginResponse
{
    public string message = "";
}

public sealed class LogoutRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class LogoutResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// New Game
// ────────────────────────────────────────────────────────────────────────────────

public sealed class NewGameRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class NewGameResponse
{
    public Blueprint blueprint = new();
    public PlayerSession player_session = new();
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Advance
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeAdvanceRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> actors = new();
}

public sealed class HomeAdvanceResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Enter Dungeon
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeEnterDungeonRequest
{
    public string user_name = "";
    public string game_name = "";
    public string dungeon_name = "";
}

public sealed class HomeEnterDungeonResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Generate Dungeon
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeGenerateDungeonRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class HomeGenerateDungeonResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Roster
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeRosterAddRequest
{
    public string user_name = "";
    public string game_name = "";
    public string member_name = "";
}

public sealed class HomeRosterAddResponse
{
    public string message = "";
}

public sealed class HomeRosterRemoveRequest
{
    public string user_name = "";
    public string game_name = "";
    public string member_name = "";
}

public sealed class HomeRosterRemoveResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Item Move
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeItemMoveToInventoryRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> item_names = new();
}

public sealed class HomeItemMoveToInventoryResponse
{
    public string message = "";
}

public sealed class HomeItemMoveToStorageRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> item_names = new();
}

public sealed class HomeItemMoveToStorageResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Costume
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeWearCostumeRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public string target_name = "";
}

public sealed class HomeWearCostumeResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Craft
// ────────────────────────────────────────────────────────────────────────────────

public sealed class HomeCraftItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> materials = new();
}

public sealed class HomeCraftItemResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Player Action
// ────────────────────────────────────────────────────────────────────────────────

[JsonConverter(typeof(StringEnumConverter))]
public enum HomePlayerActionType
{
    [EnumMember(Value = "/speak")] SPEAK,
    [EnumMember(Value = "/switch_stage")] SWITCH_STAGE,
}

public sealed class HomePlayerActionRequest
{
    public string user_name = "";
    public string game_name = "";
    public HomePlayerActionType action;
    public Dictionary<string, string> arguments = new();
}

public sealed class HomePlayerActionResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Exit / Loot
// ────────────────────────────────────────────────────────────────────────────────

public sealed class DungeonExitRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonExitResponse
{
    public string message = "";
}

public sealed class DungeonCombatCollectLootRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonCombatCollectLootResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Retreat / Init / Draw Cards
// ────────────────────────────────────────────────────────────────────────────────

public sealed class DungeonCombatRetreatRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonCombatRetreatResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

public sealed class DungeonAdvanceStageRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonAdvanceStageResponse
{
    public string message = "";
}

public sealed class DungeonCombatInitRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonCombatInitResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

public sealed class DungeonCombatDrawCardsRequest
{
    public string user_name = "";
    public string game_name = "";
}

public sealed class DungeonCombatDrawCardsResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Play Cards / Pass Turn
// ────────────────────────────────────────────────────────────────────────────────

public sealed class DungeonCombatPlayCardsRequest
{
    public string user_name = "";
    public string game_name = "";
    public string actor_name = "";
    public string card_name = "";
    public List<string> targets = new();
}

public sealed class DungeonCombatPlayCardsResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

public sealed class DungeonCombatPassTurnRequest
{
    public string user_name = "";
    public string game_name = "";
    public string actor_name = "";
}

public sealed class DungeonCombatPassTurnResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Use Consumable / Gear
// ────────────────────────────────────────────────────────────────────────────────

public sealed class DungeonCombatUseConsumableItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public List<string> targets = new();
}

public sealed class DungeonCombatUseConsumableItemResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

public sealed class DungeonCombatUseGearItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public List<string> targets = new();
}

public sealed class DungeonCombatUseGearItemResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Query Responses
// ────────────────────────────────────────────────────────────────────────────────

public sealed class DungeonStateResponse
{
    public Dungeon dungeon = new();
}

public sealed class DungeonCombatResponse
{
    public Combat combat = new();
}

public sealed class DungeonRoomResponse
{
    public CombatRoom room = new();
}

public sealed class StagesStateResponse
{
    public Dictionary<string, List<string>> mapping = new();
}

public sealed class EntitiesDetailsResponse
{
    public List<EntitySerialization> entities_serialization = new();
}

public sealed class SessionMessageResponse
{
    public List<SessionMessage> session_messages = new();
}

public sealed class TaskTriggerResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

public sealed class TasksStatusResponse
{
    public List<TaskRecord> tasks = new();
}

public sealed class BlueprintListResponse
{
    public List<Blueprint> blueprints = new();
}

public sealed class DungeonListResponse
{
    public List<Dungeon> dungeons = new();
}
