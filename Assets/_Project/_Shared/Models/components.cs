// 对应 Python models/components.py
// ECS 组件 DTO，由 ComponentSerialization.data (JObject) 承载，仅用于序列化/反序列化。
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class IdentityComponent
{
    public string name = "";
    public int creation_order = 0;
    public string entity_id = "";
}

public sealed class WorldComponent
{
    public string name = "";
}

public sealed class StageComponent
{
    public string name = "";
    public string character_sheet_name = "";
}

public sealed class ActorComponent
{
    public string name = "";
    public string character_sheet_name = "";
    public string current_stage = "";
}

public sealed class StageDescriptionComponent
{
    public string name = "";
    public string narrative = "";
}

public sealed class PlayerComponent
{
    public string name = "";
    public string player_name = "";
}

public sealed class DestroyComponent
{
    public string name = "";
}

public sealed class AppearanceComponent
{
    public string name = "";
    public string base_body = "";
    public string appearance = "";
}

public sealed class HomeComponent
{
    public string name = "";
}

public sealed class DungeonComponent
{
    public string name = "";
}

public sealed class NPCComponent
{
    public string name = "";
}

public sealed class PartyMemberComponent
{
    public string name = "";
}

public sealed class PartyRosterComponent
{
    public string name = "";
    public List<string> members = new();
}

public sealed class MonsterComponent
{
    public string name = "";
}

public sealed class HandComponent
{
    public string name = "";
    public List<Card> cards = new();
}

public sealed class RoundStatsComponent
{
    public string name = "";
    public int energy = 0;
}

public sealed class DeathComponent
{
    public string name = "";
}

public sealed class CharacterStatsComponent
{
    public string name = "";
    public CharacterStats stats = new();
}

public sealed class StatusEffectsComponent
{
    public string name = "";
    public List<StatusEffect> status_effects = new();
}

public sealed class PlayerActionAuditComponent
{
    public string name = "";
}

public sealed class DungeonGenerationComponent
{
    public string name = "";
}

public sealed class WorkshopComponent
{
    public string name = "";
}

public sealed class DrawPileComponent
{
    public string name = "";
    public List<Card> cards = new();
}

public sealed class ExhaustPileComponent
{
    public string name = "";
    public List<Card> cards = new();
}

public sealed class DiscardPileComponent
{
    public string name = "";
    public List<Card> cards = new();
}

public sealed class DeckComponent
{
    public string name = "";
    public List<Card> cards = new();
    public List<string> keywords = new();
}

public sealed class InventoryComponent
{
    public string name = "";
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> items = new();
}

public sealed class StorageComponent
{
    public string name = "";
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> items = new();
}

public sealed class CombatLootComponent
{
    public string name = "";
    [JsonConverter(typeof(AnyItemListConverter))]
    public List<Item> items = new();
}

public sealed class CostumeComponent
{
    public string name = "";
    public CostumeItem item = null;
}

public sealed class EquippedGearComponent
{
    public string name = "";
    public GearItem item = null;
}
