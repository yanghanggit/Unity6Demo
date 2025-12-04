//using System;
using System.Collections.Generic;
using System.Linq;

//using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

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

    public static string DungeonCombatDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        if (dungeon.combat_sequence.combats.Count > 0)
        {
            var last_combat = dungeon.combat_sequence.combats[dungeon.combat_sequence.combats.Count - 1];
            dungeon_text += "Last Combat: " + last_combat.name + "\n";
            dungeon_text += "Phase: " + last_combat.phase.ToString() + "\n";
            dungeon_text += "Result: " + last_combat.result.ToString() + "\n";

            if (last_combat.rounds.Count > 0)
            {
                var last_round = last_combat.rounds[last_combat.rounds.Count - 1];
                dungeon_text += "Last Round: " + last_round.tag + "\n";
                //dungeon_text += "Stage Environment: " + last_round.environment + "\n";
                dungeon_text += "Action Order: " + string.Join("-->", last_round.action_order) + "\n";
            }
        }

        return dungeon_text;
    }

    public static string ActorDisplayText(EntitySerialization actorEntitySerialization, HashSet<string> includedComponentNames = null)
    {
        var ret = "";
        for (int i = 0; i < actorEntitySerialization.components.Count; i++)
        {
            var component = actorEntitySerialization.components[i];
            if (includedComponentNames != null && !includedComponentNames.Contains(component.name))
            {
                continue;
            }

            if (component.name == typeof(CombatStatsComponent).Name)
            {
                var rpgCharacterProfileComponent = JsonConvert.DeserializeObject<CombatStatsComponent>(JsonConvert.SerializeObject(component.data));
                var rpgCharacterProfile = rpgCharacterProfileComponent.stats;
                var rpgCharacterProfileText = $"{actorEntitySerialization.name} = HP:{rpgCharacterProfile.hp}/{rpgCharacterProfile.max_hp}," +
                        $" Strength:{rpgCharacterProfile.strength}," +
                        $" Dexterity:{rpgCharacterProfile.dexterity}," +
                        $" Wisdom:{rpgCharacterProfile.wisdom}," +
                        $" Physical Attack:{rpgCharacterProfile.physical_attack}," +
                        $" Physical Defense:{rpgCharacterProfile.physical_defense}," +
                        $" Magic Attack:{rpgCharacterProfile.magic_attack}," +
                        $" Magic Defense:{rpgCharacterProfile.magic_defense}\n";

                if (rpgCharacterProfileComponent.effects.Count > 0)
                {
                    rpgCharacterProfileText += "Status Effects: ";
                    for (int j = 0; j < rpgCharacterProfileComponent.effects.Count; j++)
                    {
                        var statusEffect = rpgCharacterProfileComponent.effects[j];
                        rpgCharacterProfileText += $"{statusEffect.name} ({statusEffect.description}, {statusEffect.duration})";
                        if (j < rpgCharacterProfileComponent.effects.Count - 1)
                        {
                            rpgCharacterProfileText += ", ";
                        }
                    }
                    rpgCharacterProfileText += "\n";
                }

                ret += rpgCharacterProfileText;
            }
            else if (component.name == typeof(HandComponent).Name)
            {
                var handComponent = JsonConvert.DeserializeObject<HandComponent>(JsonConvert.SerializeObject(component.data));
                var handCompText = $"{actorEntitySerialization.name} Hand: ";
                for (int j = 0; j < handComponent.cards.Count; j++)
                {
                    var card = handComponent.cards[j];
                    handCompText += $"{card.name} ({card.description})";
                    if (j < handComponent.cards.Count - 1)
                    {
                        handCompText += ", ";
                    }
                }
                handCompText += "\n";
                for (int j = 0; j < handComponent.cards.Count; j++)
                {
                    var card = handComponent.cards[j];
                    handCompText += $"Card: {card.name}, Targets: {string.Join(", ", card.targets)}\n";
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

    /// <summary>
    /// 将单个代理事件格式化为简洁的摘要字符串
    /// 根据事件类型生成不同格式的描述文本，用于UI显示或日志记录
    /// </summary>
    /// <param name="agentEvent">要格式化的代理事件</param>
    /// <returns>格式化后的事件摘要字符串，格式为 "(事件类型) 内容"。如果事件类型未处理则返回空字符串</returns>
    public static string FormatAgentEventSummary(AgentEvent agentEvent)
    {
        switch ((EventHead)agentEvent.head)
        {
            case EventHead.NONE:
                var message = agentEvent.message;
                Debug.Log("NONE: " + message);
                break;

            case EventHead.MIND_EVENT:
                MindEvent mindVoiceEvent = (MindEvent)agentEvent;
                return $"(mind) {mindVoiceEvent.content}";

            case EventHead.SPEAK_EVENT:
                SpeakEvent speakEvent = (SpeakEvent)agentEvent;
                return $"(speak) @{speakEvent.target} {speakEvent.content}";


            case EventHead.WHISPER_EVENT:
                WhisperEvent whisperEvent = (WhisperEvent)agentEvent;
                return $"(whisper) @{whisperEvent.target} {whisperEvent.content}";

            case EventHead.ANNOUNCE_EVENT:
                AnnounceEvent announceEvent = (AnnounceEvent)agentEvent;
                return $"(announce) {announceEvent.content}";


            case EventHead.TRANS_STAGE_EVENT:
                TransStageEvent transStageEvent = (TransStageEvent)agentEvent;
                return $"(trans_stage) from {transStageEvent.from_stage} to {transStageEvent.to_stage}";

            default:
                // 其他未处理的事件类型
                Debug.LogWarning($"Unhandled event type: {agentEvent.head}");
                break;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取执行了场景切换的角色集合
    /// 遍历所有角色的事件历史,查找所有发生 TransStageEvent 的角色
    /// </summary>
    /// <param name="agentEventsHistory">代理事件历史字典,键为角色名称,值为该角色的事件列表</param>
    /// <returns>返回执行了场景切换的角色名称集合,如果没有则返回空集合</returns>
    public static HashSet<string> GetActorsWithTransStageEvents(Dictionary<string, List<AgentEvent>> agentEventsHistory)
    {
        HashSet<string> switchStageActors = new();

        foreach (var kvp in agentEventsHistory)
        {
            foreach (var agentEvent in kvp.Value)
            {
                if (agentEvent.head == (int)EventHead.TRANS_STAGE_EVENT)
                {
                    TransStageEvent transStageEvent = (TransStageEvent)agentEvent;
                    switchStageActors.Add(transStageEvent.actor);
                }
            }
        }

        return switchStageActors;
    }

    /// <summary>
    /// 格式化地下城概览信息为可读文本
    /// 将地下城的名称、关卡列表和每关的怪物信息组织成便于UI显示的字符串
    /// </summary>
    /// <param name="dungeon">要格式化的地下城对象</param>
    /// <returns>格式化后的地下城概览文本，包含地下城名称、各关卡名称及对应的怪物列表</returns>
    public static string FormatDungeonOverview(Dungeon dungeon)
    {
        var dungeonOverviewText = string.Empty;
        dungeonOverviewText += "地下城 = " + dungeon.name + "\n";
        for (int i = 0; i < dungeon.stages.Count; i++)
        {
            dungeonOverviewText += "第" + (i + 1) + "关 = " + dungeon.stages[i].name + "\n";
            dungeonOverviewText += "怪物 = " + string.Join(", ", dungeon.stages[i].actors.Select(a => a.name)) + "\n";
        }

        return dungeonOverviewText;
    }
}

