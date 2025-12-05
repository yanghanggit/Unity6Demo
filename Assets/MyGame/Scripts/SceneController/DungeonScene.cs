using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DungeonScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene2";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        StartCoroutine(RefreshDungeonStateDisplay());
    }

    public void OnClickCombatInit()
    {
        Debug.Log("OnClickCombatInit");
        StartCoroutine(ExecuteCombatInit());
    }

    public void OnClickViewDungeon()
    {
        Debug.Log("OnClickViewDungeon");
        StartCoroutine(RefreshDungeonStateDisplay());
    }

    public void OnClickViewActor()
    {
        Debug.Log("OnClickViewActor");
        StartCoroutine(ExecuteViewActorStats());
    }

    public void OnClickDrawCards()
    {
        Debug.Log("OnClickDrawCards");
        StartCoroutine(ExecuteDrawCardsAndShowHands());
    }

    public void OnClickPlayCards()
    {
        Debug.Log("OnClickPlayCards");
        StartCoroutine(ExecutePlayCardsAndShowResult());
    }

    public void OnClickAdvanceNextDungeon()
    {
        Debug.Log("OnClickAdvanceNextDungeon");
        StartCoroutine(ExecuteAdvanceNextDungeon());
    }

    public void OnClickBackHome()
    {
        Debug.Log("OnClickBackHome");
        StartCoroutine(ExecuteBackHome());
    }

    /// <summary>
    /// 初始化战斗并刷新地下城状态
    /// 调用服务器 combat_init 接口开始战斗，成功后刷新并显示当前地下城状态
    /// </summary>
    private IEnumerator ExecuteCombatInit()
    {
        bool success = false;
        yield return DungeonGamePlayManager.Instance.CombatInit(
            (result, message, sessionMessages) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (success)
        {
            yield return RefreshDungeonStateDisplay();
        }
    }

    /// <summary>
    /// 执行抽卡操作并显示所有角色的手牌
    /// 调用服务器 draw_cards 接口，刷新角色数据后显示每个角色的手牌信息
    /// </summary>
    private IEnumerator ExecuteDrawCardsAndShowHands()
    {
        bool success = false;
        List<SessionMessage> sessionMessages = null;
        yield return DungeonGamePlayManager.Instance.DrawCards(
            (result, message, messages) =>
            {
                success = result;
                sessionMessages = messages;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        // Early return: 抽卡失败
        if (!success)
        {
            yield break;
        }

        // 刷新角色数据并显示手牌信息
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 检查是否有战斗完成事件
        if (sessionMessages != null && DisplayCombatCompleteResult(sessionMessages))
        {
            yield break;
        }

        DisplayAllActorsHands();
    }

    /// <summary>
    /// 显示所有角色的手牌信息
    /// 遍历所有角色实体，提取手牌组件并格式化显示
    /// </summary>
    private void DisplayAllActorsHands()
    {
        var text = "";
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;

        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent == null)
            {
                Debug.LogWarning($"HandComponent is null for actor: {actorEntity.name}");
                continue;
            }

            text += GameUtils.FormatHandComponent(handComponent);
            text += "\n";
        }

        _mainText.text = text;
    }

    /// <summary>
    /// 执行打牌操作并显示战斗仲裁结果
    /// 调用服务器 play_cards 接口，获取战斗事件并显示战斗日志和叙述文本
    /// </summary>
    private IEnumerator ExecutePlayCardsAndShowResult()
    {
        // bool success = false;
        yield return DungeonGamePlayManager.Instance.PlayCards(
            (result, message, sessionMessages) =>
            {
                // success = result;
                if (result)
                {
                    // 显示战斗仲裁结果
                    DisplayCombatArbitrationResult(sessionMessages);
                }
                else
                {
                    // 显示错误消息
                    _mainText.text = message;
                }
            });
    }

    /// <summary>
    /// 从会话消息中提取并显示战斗仲裁结果
    /// 查找最后一个战斗仲裁事件并显示其战斗日志和叙述内容
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    private void DisplayCombatArbitrationResult(List<SessionMessage> sessionMessages)
    {
        if (sessionMessages == null || sessionMessages.Count == 0)
        {
            Debug.LogWarning("No session messages to display combat arbitration result");
            _mainText.text = "No session messages available.";
            return;
        }

        foreach (var msg in sessionMessages)
        {
            var agentEvent = GameUtils.ParseAgentEvent(msg);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse AgentEvent from session message");
                continue;
            }

            if (agentEvent is CombatArbitrationEvent combatEvent)
            {
                Debug.Log($"Found CombatArbitrationEvent: combat_log={combatEvent.combat_log}, narrative={combatEvent.narrative}");
                _mainText.text = $"[combat_log]\n{combatEvent.combat_log}\n[narrative]\n{combatEvent.narrative}";
                break;
            }
            else
            {
                Debug.Log($"Skipping non-combat arbitration event of type: {agentEvent.GetType().Name}");
            }
        }
    }

    /// <summary>
    /// 从会话消息中提取并显示战斗完成结果
    /// 查找所有战斗完成事件并显示每个角色的战斗总结
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    /// <returns>是否成功找到并显示战斗完成事件</returns>
    private bool DisplayCombatCompleteResult(List<SessionMessage> sessionMessages)
    {
        if (sessionMessages == null || sessionMessages.Count == 0)
        {
            return false;
        }

        var completeEvents = new List<CombatCompleteEvent>();

        foreach (var msg in sessionMessages)
        {
            var agentEvent = GameUtils.ParseAgentEvent(msg);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse AgentEvent from session message");
                continue;
            }

            if (agentEvent is CombatCompleteEvent completeEvent)
            {
                Debug.Log($"Found CombatCompleteEvent: actor={completeEvent.actor}, summary={completeEvent.summary}");
                completeEvents.Add(completeEvent);
            }
            else
            {
                Debug.Log($"Skipping non-combat complete event of type: {agentEvent.GetType().Name}");
            }
        }

        if (completeEvents.Count == 0)
        {
            return false;
        }

        var text = string.Empty;
        foreach (var evt in completeEvents)
        {
            text += $"Actor: {evt.actor}\nSummary: {evt.summary}\n\n";
        }

        _mainText.text = "战斗完成！\n\n" + text;
        return true;
    }


    /// <summary>
    /// 刷新并显示地下城状态
    /// 从服务器获取最新的地下城和角色数据，然后更新UI显示当前场景的角色分布和战斗信息
    /// </summary>
    private IEnumerator RefreshDungeonStateDisplay()
    {
        // 从服务器刷新地下城和角色数据
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 获取当前角色所在场景及该场景中的所有角色
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");

        // 需要所有的角色名称列表！
        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);

        // 格式化并显示地下城状态（包括场景-角色映射和战斗序列信息）
        _mainText.text = GameUtils.FormatDungeonStateDisplay(GameContext.Instance.Dungeon, new Dictionary<string, List<string>> { { stageName, actorsInStage } });
    }

    /// <summary>
    /// 查看并显示所有角色的战斗属性
    /// 从服务器刷新数据后，获取所有角色的战斗属性组件并格式化显示
    /// </summary>
    private IEnumerator ExecuteViewActorStats()
    {
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        var text = "";
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var combatStatsComponent = GameUtils.GetComponent<CombatStatsComponent>(actorEntitiesSerialization[i]);
            if (combatStatsComponent == null)
            {
                Debug.Assert(false, "combatStatsComponent is null");
                continue;
            }
            text += GameUtils.FormatCombatStatsComponent(combatStatsComponent);
            text += "\n";
        }
        _mainText.text = text;
    }

    /// <summary>
    /// 前进到下一个地下城关卡
    /// 调用服务器 advance_next_dungeon 接口，成功后刷新并显示新的地下城状态
    /// </summary>
    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        bool success = false;
        yield return DungeonGamePlayManager.Instance.AdvanceNextDungeon(
            (result, message, sessionMessages) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (success)
        {
            yield return RefreshDungeonStateDisplay();
        }
    }

    /// <summary>
    /// 返回主场景
    /// 调用服务器传送回家接口，成功后切换到主场景
    /// </summary>
    private IEnumerator ExecuteBackHome()
    {
        //Debug.Log("ExecuteBackHome");
        bool success = false;
        yield return DungeonGamePlayManager.Instance.TransHome(
            (result, message) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            });

        if (success)
        {
            //Debug.Log("TransHomeAction request success");
            yield return new WaitForSeconds(0);
            SceneManager.LoadScene(_preScene);
        }
    }
}

