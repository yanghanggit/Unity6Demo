using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class GameUtils
{
    /// <summary>
    /// 格式化地下城状态显示文本，包含场景-角色映射和战斗序列信息
    /// </summary>
    public static string FormatDungeonStateDisplay(Dungeon dungeon, Dictionary<string, List<string>> stageActorsMapping)
    {
        var parts = new List<string>();

        if (stageActorsMapping != null)
        {
            var mappingText = FormatStageActorsMapping(stageActorsMapping);
            if (!string.IsNullOrEmpty(mappingText))
            {
                parts.Add(mappingText);
            }
        }

        var combatInfo = FormatCombatSequenceDisplay(dungeon.combat_sequence);
        if (!string.IsNullOrEmpty(combatInfo))
        {
            parts.Add(combatInfo);
        }

        return string.Join("\n", parts);
    }

    /// <summary>
    /// 格式化场景-角色映射显示文本
    /// </summary>
    public static string FormatStageActorsMapping(Dictionary<string, List<string>> mapping)
    {
        if (mapping == null || mapping.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var kvp in mapping)
        {
            var displayNames = kvp.Value.Select(GetDisplayName);
            sb.AppendLine($"{kvp.Key}: {string.Join(", ", displayNames)}");
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 格式化战斗序列显示文本
    /// </summary>
    public static string FormatCombatSequenceDisplay(CombatSequence combatSequence)
    {
        if (combatSequence?.combats == null || combatSequence.combats.Count == 0)
            return string.Empty;

        var lastCombat = combatSequence.combats[combatSequence.combats.Count - 1];
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"Combat: {lastCombat.name}");
        sb.AppendLine($"State: {lastCombat.state}");
        sb.AppendLine($"Result: {lastCombat.result}");

        if (lastCombat.rounds?.Count > 0)
        {
            var lastRound = lastCombat.rounds[lastCombat.rounds.Count - 1];
            sb.AppendLine($"Round: {lastRound.tag}");
            sb.AppendLine($"Action Order: {string.Join(" --> ", lastRound.action_order.Select(GetDisplayName))}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 格式化战斗属性组件为显示文本
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <param name="combatStatsComponent">战斗属性组件</param>
    /// <returns>格式化后的战斗属性文本</returns>
    public static string FormatCombatStatsComponent(CombatStatsComponent combatStatsComponent)
    {
        if (combatStatsComponent == null)
            return string.Empty;

        var stats = combatStatsComponent.stats;
        var text = $"{GetDisplayName(combatStatsComponent.name)} = LV:{stats.level}, HP:{stats.hp}/{stats.max_hp}," +
                   $" Strength:{stats.strength}," +
                   $" Dexterity:{stats.dexterity}," +
                   $" Wisdom:{stats.wisdom}," +
                   $" Physical Attack:{stats.physical_attack}," +
                   $" Physical Defense:{stats.physical_defense}," +
                   $" Magic Attack:{stats.magic_attack}," +
                   // $" Magic Defense:{stats.magic_defense}\n";
                   "\n";

        if (combatStatsComponent.status_effects.Count > 0)
        {
            text += "Status Effects: \n";
            for (int i = 0; i < combatStatsComponent.status_effects.Count; i++)
            {
                var statusEffect = combatStatsComponent.status_effects[i];
                text += $"[{statusEffect.name}]: \n{statusEffect.description}({statusEffect.duration})";
                if (i < combatStatsComponent.status_effects.Count - 1)
                {
                    text += "\n";
                }
            }
            text += "\n";
        }

        return text;
    }

    /// <summary>
    /// 格式化手牌组件为显示文本
    /// </summary>
    /// <param name="handComponent">手牌组件</param>
    /// <returns>格式化后的手牌文本</returns>
    public static string FormatHandComponent(HandComponent handComponent)
    {
        if (handComponent == null || handComponent.cards.Count == 0)
            return string.Empty;

        var text = $"{GetDisplayName(handComponent.name)} Hand: \n";

        for (int i = 0; i < handComponent.cards.Count; i++)
        {
            var card = handComponent.cards[i];
            text += $"[{card.name}]:{card.description}";
            if (i < handComponent.cards.Count - 1)
            {
                text += "\n";
            }
        }
        text += "\n";

        for (int i = 0; i < handComponent.cards.Count; i++)
        {
            var card = handComponent.cards[i];
            text += $"Card: {card.name}, Targets: {string.Join(", ", card.targets)}\n";
        }
        text += "\n";

        return text;
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
            dungeonOverviewText += "怪物 = " + string.Join(", ", dungeon.stages[i].actors.Select(a => GetDisplayName(a.name))) + "\n";
        }

        return dungeonOverviewText;
    }

    /// <summary>
    /// 格式化战斗回合信息为显示文本
    /// 将回合的标签、行动顺序、战斗日志和叙事文本组织成便于UI显示的字符串
    /// </summary>
    /// <param name="round">要格式化的回合对象</param>
    /// <returns>格式化后的回合信息文本，包含回合标签、角色行动顺序、战斗日志和叙事描述。如果回合为 null 则返回空字符串</returns>
    public static string FormatRoundInfo(Round round)
    {
        if (round == null)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Round: {round.tag}");

        if (round.action_order != null && round.action_order.Count > 0)
        {
            sb.AppendLine($"Action Order: {string.Join(" -> ", round.action_order.Select(GetDisplayName))}");
        }

        if (!string.IsNullOrEmpty(round.combat_log))
        {
            sb.AppendLine($"Combat Log:\n{round.combat_log}");
        }

        if (!string.IsNullOrEmpty(round.narrative))
        {
            sb.AppendLine($"Narrative:\n{round.narrative}");
        }

        return sb.ToString().TrimEnd();
    }
}