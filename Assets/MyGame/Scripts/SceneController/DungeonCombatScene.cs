using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DungeonCombatScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene2";
    [SerializeField] private string _nextScene = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");

        // 初始化UI文本
        _mainText.text = "Initializing Dungeon Combat Scene...";

        // 检查是否已经连接服务器
        if (RootResp.Get() != null)
        {
            // 已经连接服务器，开始初始化战斗场景
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
            _mainText.text = $"{GameContext.Instance.Dungeon.name} | {stageName} : Initializing combat scene...";

            //Debug.Log("DungeonCombatScene Start: RootResp is already set");
            StartCoroutine(ExecuteCombatInit());
        }
        else
        {
            // 没有连接服务器，基本是本地测试模式
            Debug.Log("DungeonCombatScene Start: RootResp is null, running in local test mode");
        }
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

    public void OnClickViewCards()
    {
        Debug.Log("OnClickViewCards");
        StartCoroutine(ExecuteViewActorCards());
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
        // 先刷新数据，检查是否有角色已经持有手牌
        bool anyActorHasHandCards = false;
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer(
            (result, message) =>
            {
                if (result)
                {
                    anyActorHasHandCards = AnyActorHasHandCards();
                }
            }
        );

        if (anyActorHasHandCards)
        {
            _mainText.text = "角色已有手牌，跳过抽卡操作。";
            yield break;
        }

        // 正式的抽卡操作
        // 步骤函数定义
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
            Debug.LogWarning("DrawCards action failed");
            yield break;
        }

        // 检查是否有战斗完成事件
        if (sessionMessages != null && DisplayCombatCompleteResult(sessionMessages))
        {
            // 已经显示战斗完成结果，直接返回，不要再显示手牌信息！
            yield break;
        }

        // 刷新角色数据并显示手牌信息
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 显示所有角色的手牌信息
        DisplayAllActorsHands();
    }

    /// <summary>
    /// 检查是否有任意角色持有手牌
    /// 遍历所有角色实体,检查其手牌组件是否包含卡牌
    /// 用于在抽卡操作前判断是否需要跳过抽卡(避免重复抽卡)
    /// </summary>
    /// <returns>如果至少有一个角色持有手牌则返回 true,否则返回 false</returns>
    private bool AnyActorHasHandCards()
    {
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;

        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent != null && handComponent.cards.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 显示所有角色的手牌信息
    /// 遍历所有角色实体，提取手牌组件并格式化显示
    /// </summary>
    private void DisplayAllActorsHands()
    {
        var text = string.Empty;

        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        foreach (var actorEntity in actorEntitiesSerialization)
        {
            var handComponent = GameUtils.GetComponent<HandComponent>(actorEntity);
            if (handComponent == null)
            {
                //Debug.Log($"HandComponent is null for actor: {actorEntity.name}");
                continue;
            }

            text += GameUtils.FormatHandComponent(handComponent);
            text += "\n";
        }

        if (string.IsNullOrEmpty(text))
        {
            _mainText.text = "当前没有角色持有手牌信息。";
        }
        else
        {
            _mainText.text = "当前角色手牌信息：\n\n" + text;
        }
    }

    /// <summary>
    /// 执行打牌操作并显示战斗仲裁结果
    /// 调用服务器 play_cards 接口，获取战斗事件并显示战斗日志和叙述文本
    /// </summary>
    private IEnumerator ExecutePlayCardsAndShowResult()
    {
        bool success = false;
        yield return DungeonGamePlayManager.Instance.PlayCards(
            (result, message, sessionMessages) =>
            {
                success = result;
                if (result)
                {
                    // 显示战斗仲裁结果
                    Debug.Log("PlayCards action succeeded, displaying combat arbitration results");
                }
                else
                {
                    // 显示错误消息
                    _mainText.text = message;
                }
            });

        if (!success)
        {
            yield break;
        }

        // 刷新地下城和角色数据
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        // 获取最新的地下城回合信息
        Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        if (round == null)
        {
            Debug.LogWarning("No rounds found in dungeon after playing cards");
            _mainText.text = "No round information available after playing cards.";
            yield break;
        }

        // 显示最新的地下城战斗仲裁信息
        var formattedRoundInfo = GameUtils.FormatRoundInfo(round);
        if (!string.IsNullOrEmpty(formattedRoundInfo))
        {
            _mainText.text = formattedRoundInfo;
        }
        else
        {
            Debug.LogWarning("No combat arbitration info available in dungeon state");
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
//                Debug.Log($"Found CombatCompleteEvent: actor={completeEvent.actor}, summary={completeEvent.summary}");
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
    /// 查看并显示所有角色的手牌信息
    /// 从服务器刷新数据后,直接显示所有角色当前持有的手牌
    /// 用于在游戏过程中随时查看角色的手牌状态
    /// </summary>
    private IEnumerator ExecuteViewActorCards()
    {
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        DisplayAllActorsHands();
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

        if (!success)
        {
            yield break;
        }

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer(
            (result, message) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = message;
                }
            }
        );

        if (!success)
        {
            yield break;
        }

        // 3. 切换到地下城场景
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
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

