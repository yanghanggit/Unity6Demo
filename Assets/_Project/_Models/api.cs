// 对应 Python models/api.py
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

// ────────────────────────────────────────────────────────────────────────────────
// Login / Logout
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class LoginRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class LoginResponse
{
    public string message = "";
}

[System.Serializable]
public sealed class LogoutRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class LogoutResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// New Game
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class NewGameRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class NewGameResponse
{
    public Blueprint blueprint = new Blueprint();
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Advance
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeAdvanceRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> actors = new List<string>();
}

[System.Serializable]
public sealed class HomeAdvanceResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Enter Dungeon
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeEnterDungeonRequest
{
    public string user_name = "";
    public string game_name = "";
    public string dungeon_name = "";
}

[System.Serializable]
public sealed class HomeEnterDungeonResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Generate Dungeon
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeGenerateDungeonRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class HomeGenerateDungeonResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Roster
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeRosterAddRequest
{
    public string user_name = "";
    public string game_name = "";
    public string member_name = "";
}

[System.Serializable]
public sealed class HomeRosterAddResponse
{
    public string message = "";
}

[System.Serializable]
public sealed class HomeRosterRemoveRequest
{
    public string user_name = "";
    public string game_name = "";
    public string member_name = "";
}

[System.Serializable]
public sealed class HomeRosterRemoveResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Item Move
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeItemMoveToInventoryRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> item_names = new List<string>();
}

[System.Serializable]
public sealed class HomeItemMoveToInventoryResponse
{
    public string message = "";
}

[System.Serializable]
public sealed class HomeItemMoveToStorageRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> item_names = new List<string>();
}

[System.Serializable]
public sealed class HomeItemMoveToStorageResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Costume
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeWearCostumeRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public string target_name = "";
}

[System.Serializable]
public sealed class HomeWearCostumeResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Home: Craft
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class HomeCraftItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public List<string> materials = new List<string>();
}

[System.Serializable]
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
    public Dictionary<string, string> arguments = new Dictionary<string, string>();
}

[System.Serializable]
public sealed class HomePlayerActionResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Exit / Loot
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class DungeonExitRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonExitResponse
{
    public string message = "";
}

[System.Serializable]
public sealed class DungeonCombatCollectLootRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonCombatCollectLootResponse
{
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Retreat / Init / Draw Cards
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class DungeonCombatRetreatRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonCombatRetreatResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

[System.Serializable]
public sealed class DungeonAdvanceStageRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonAdvanceStageResponse
{
    public string message = "";
}

[System.Serializable]
public sealed class DungeonCombatInitRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonCombatInitResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

[System.Serializable]
public sealed class DungeonCombatDrawCardsRequest
{
    public string user_name = "";
    public string game_name = "";
}

[System.Serializable]
public sealed class DungeonCombatDrawCardsResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Play Cards / Pass Turn
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class DungeonCombatPlayCardsRequest
{
    public string user_name = "";
    public string game_name = "";
    public string actor_name = "";
    public string card_name = "";
    public List<string> targets = new List<string>();
}

[System.Serializable]
public sealed class DungeonCombatPlayCardsResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

[System.Serializable]
public sealed class DungeonCombatPassTurnRequest
{
    public string user_name = "";
    public string game_name = "";
    public string actor_name = "";
}

[System.Serializable]
public sealed class DungeonCombatPassTurnResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Dungeon: Combat – Use Consumable / Gear
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class DungeonCombatUseConsumableItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public List<string> targets = new List<string>();
}

[System.Serializable]
public sealed class DungeonCombatUseConsumableItemResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

[System.Serializable]
public sealed class DungeonCombatUseGearItemRequest
{
    public string user_name = "";
    public string game_name = "";
    public string item_name = "";
    public List<string> targets = new List<string>();
}

[System.Serializable]
public sealed class DungeonCombatUseGearItemResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

// ────────────────────────────────────────────────────────────────────────────────
// Query Responses
// ────────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public sealed class DungeonStateResponse
{
    public Dungeon dungeon = new Dungeon();
}

[System.Serializable]
public sealed class DungeonCombatResponse
{
    public Combat combat = new Combat();
}

[System.Serializable]
public sealed class DungeonRoomResponse
{
    public CombatRoom room = new CombatRoom();
}

public sealed class StagesStateResponse
{
    public Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>();
}

[System.Serializable]
public sealed class EntitiesDetailsResponse
{
    public List<EntitySerialization> entities_serialization = new List<EntitySerialization>();
}

[System.Serializable]
public sealed class SessionMessageResponse
{
    public List<SessionMessage> session_messages = new List<SessionMessage>();
}

[System.Serializable]
public sealed class TaskTriggerResponse
{
    public string task_id = "";
    public string status = "";
    public string message = "";
}

[System.Serializable]
public sealed class TasksStatusResponse
{
    public List<TaskRecord> tasks = new List<TaskRecord>();
}

[System.Serializable]
public sealed class BlueprintListResponse
{
    public List<Blueprint> blueprints = new List<Blueprint>();
}

[System.Serializable]
public sealed class DungeonListResponse
{
    public List<Dungeon> dungeons = new List<Dungeon>();
}
