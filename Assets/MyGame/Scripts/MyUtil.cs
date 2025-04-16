using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public static class MyUtils
{
    public static string MappingDisplayText(Dictionary<string, List<string>> mapping)
    {
        var mapping_text = "";
        foreach (var kvp in mapping)
        {
            mapping_text += kvp.Key + ": ";
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                mapping_text += kvp.Value[i];
                if (i < kvp.Value.Count - 1)
                {
                    mapping_text += ", ";
                }
            }
            mapping_text += "\n";
        }
        return mapping_text;
    }

    public static string DungeonDisplayText(Dungeon dungeon, Dictionary<string, List<string>> mapping)
    {
        var dungeon_text = "";
        dungeon_text += "Dungeon: " + dungeon.name + "\n";
        for (int i = 0; i < dungeon.levels.Count; i++)
        {
            dungeon_text += "Level " + (i + 1) + ": " + dungeon.levels[i].name + "\n";
            dungeon_text += "Actors: " + string.Join(", ", dungeon.levels[i].actors.Select(a => a.name)) + "\n";
        }

        if (dungeon.engagement.combats.Count > 0)
        {
            var last_combat = dungeon.engagement.combats[dungeon.engagement.combats.Count - 1];
            dungeon_text += "Last Combat: " + last_combat.name + "\n";
            dungeon_text += "Phase: " + last_combat.phase.ToString() + "\n";
            dungeon_text += "Result: " + last_combat.result.ToString() + "\n";

            if (last_combat.rounds.Count > 0)
            {
                var last_round = last_combat.rounds[last_combat.rounds.Count - 1];
                dungeon_text += "Last Round: " + last_round.tag + "\n";
                dungeon_text += "Stage Environment: " + last_round.stage_environment + "\n";
                dungeon_text += "Round Turn: " + string.Join("-->", last_round.round_turns) + "\n";
            }
        }

        return MappingDisplayText(mapping) + "\n" + dungeon_text;
    }

    public static string GetActorLocation(string actor, Dictionary<string, List<string>> mapping)
    {
        // 在 mapping 遍历每一个 key-value 对, 在value中如果存在 actor, 则返回 key
        foreach (var kvp in mapping)
        {
            if (kvp.Value.Contains(actor))
            {
                return kvp.Key;
            }
        }
        return "";
    }

    public static List<string> RetrieveActorsForStage(string actor, Dictionary<string, List<string>> mapping)
    {
        var locateActorStageName = GetActorLocation(actor, mapping);
        if (locateActorStageName == "")
        {
            return new List<string>();
        }
        // 在 mapping 中找到 locateActorStageName 对应的 value
        if (mapping.TryGetValue(locateActorStageName, out var actors))
        {
            return actors;
        }

        return new List<string>();
    }

    public static string ActorDisplayText(EntitySnapshot actorSnapshot, HashSet<string> includedComponentNames = null)
    {
        var ret = "";
        for (int i = 0; i < actorSnapshot.components.Count; i++)
        {
            var component = actorSnapshot.components[i];
            if (includedComponentNames != null && !includedComponentNames.Contains(component.name))
            {
                continue;
            }

            if (component.name == typeof(RPGCharacterProfileComponent).Name)
            {
                var rpgCharacterProfileComponent = JsonConvert.DeserializeObject<RPGCharacterProfileComponent>(JsonConvert.SerializeObject(component.data));
                var rpgCharacterProfile = rpgCharacterProfileComponent.rpg_character_profile;
                var rpgCharacterProfileText = $"{actorSnapshot.name} = HP:{rpgCharacterProfile.hp}/{rpgCharacterProfile.max_hp}," +
                        $" Strength:{rpgCharacterProfile.strength}," +
                        $" Dexterity:{rpgCharacterProfile.dexterity}," +
                        $" Wisdom:{rpgCharacterProfile.wisdom}," +
                        $" Physical Attack:{rpgCharacterProfile.physical_attack}," +
                        $" Physical Defense:{rpgCharacterProfile.physical_defense}," +
                        $" Magic Attack:{rpgCharacterProfile.magic_attack}," +
                        $" Magic Defense:{rpgCharacterProfile.magic_defense}\n";

                rpgCharacterProfileText += "Status Effects: ";
                for (int j = 0; j < rpgCharacterProfileComponent.status_effects.Count; j++)
                {
                    var statusEffect = rpgCharacterProfileComponent.status_effects[j];
                    rpgCharacterProfileText += $"{statusEffect.name} ({statusEffect.description}, {statusEffect.rounds})";
                    if (j < rpgCharacterProfileComponent.status_effects.Count - 1)
                    {
                        rpgCharacterProfileText += ", ";
                    }
                }
                rpgCharacterProfileText += "\n";
                ret += rpgCharacterProfileText;
            }
            else if (component.name == typeof(HandComponent).Name)
            {
                var handComponent = JsonConvert.DeserializeObject<HandComponent>(JsonConvert.SerializeObject(component.data));
                var handCompText = $"{actorSnapshot.name} Hand: ";
                for (int j = 0; j < handComponent.skills.Count; j++)
                {
                    var skill = handComponent.skills[j];
                    handCompText += $"{skill.name} ({skill.description}, {skill.effect})";
                    if (j < handComponent.skills.Count - 1)
                    {
                        handCompText += ", ";
                    }
                }
                handCompText += "\n";
                for (int j = 0; j < handComponent.details.Count; j++)
                {
                    var detail = handComponent.details[j];
                    handCompText += $"Skill: {detail.skill}, Targets: {string.Join(", ", detail.targets)}\n";
                }
                ret += handCompText + "\n";
            }
        }

        return ret;
    }

    public static string AgentLogsDisplayText(List<string> agentEventLogs)
    {
        string mainTextUpdater = "";
        for (int i = 0; i < agentEventLogs.Count; i++)
        {
            mainTextUpdater += agentEventLogs[i] + "\n";
        }
        if (mainTextUpdater == "")
        {
            mainTextUpdater = "No logs";
        }
        return mainTextUpdater;
    }
}

