using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class GameUtils
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
            sb.AppendLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
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

        sb.AppendLine($"Last Combat: {lastCombat.name}");
        sb.AppendLine($"Phase: {lastCombat.phase}");
        sb.AppendLine($"Result: {lastCombat.result}");

        if (lastCombat.rounds?.Count > 0)
        {
            var lastRound = lastCombat.rounds[lastCombat.rounds.Count - 1];
            sb.AppendLine($"Last Round: {lastRound.tag}");
            sb.AppendLine($"Action Order: {string.Join(" --> ", lastRound.action_order)}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 从实体序列化数据中获取指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="actorEntitySerialization">实体序列化数据</param>
    /// <returns>如果找到返回组件实例，否则返回 null</returns>
    public static T GetComponent<T>(EntitySerialization actorEntitySerialization) where T : class
    {
        if (actorEntitySerialization?.components == null)
            return null;

        string componentName = typeof(T).Name;

        foreach (var component in actorEntitySerialization.components)
        {
            if (component.name == componentName)
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(component.data));
            }
        }

        return null;
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
        var text = $"{combatStatsComponent.name} = HP:{stats.hp}/{stats.max_hp}," +
                   $" Strength:{stats.strength}," +
                   $" Dexterity:{stats.dexterity}," +
                   $" Wisdom:{stats.wisdom}," +
                   $" Physical Attack:{stats.physical_attack}," +
                   $" Physical Defense:{stats.physical_defense}," +
                   $" Magic Attack:{stats.magic_attack}," +
                   $" Magic Defense:{stats.magic_defense}\n";

        if (combatStatsComponent.effects.Count > 0)
        {
            text += "Status Effects: ";
            for (int i = 0; i < combatStatsComponent.effects.Count; i++)
            {
                var statusEffect = combatStatsComponent.effects[i];
                text += $"{statusEffect.name} ({statusEffect.description}, {statusEffect.duration})";
                if (i < combatStatsComponent.effects.Count - 1)
                {
                    text += ", ";
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

        var text = $"{handComponent.name} Hand: ";

        for (int i = 0; i < handComponent.cards.Count; i++)
        {
            var card = handComponent.cards[i];
            text += $"{card.name} ({card.description})";
            if (i < handComponent.cards.Count - 1)
            {
                text += ", ";
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

