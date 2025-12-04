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

    [Header("API Components")]
    [SerializeField] private DungeonGamePlayApi _dungeonGamePlayApi;
    [SerializeField] private TransHomeApi _transHomeApi;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_dungeonGamePlayApi != null, "_dungeonAction is null");
        Debug.Assert(_transHomeApi != null, "_transHomeApi is null");

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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "combat_init");

        if (_dungeonGamePlayApi.RespData == null)
        {
            if (_dungeonGamePlayApi.ReqResult != null)
            {
                Debug.LogError("ExecuteCombatInit request failed: " + _dungeonGamePlayApi.ReqResult.responseText);
                _mainText.text = _dungeonGamePlayApi.ReqResult.responseText;
            }
            else
            {
                Debug.LogError("ExecuteCombatInit request failed: response data is null");
            }
            yield break;
        }

        //yield return FetchAndProcessSessionMessages();
        yield return RefreshDungeonStateDisplay();
    }

    /// <summary>
    /// 执行抽卡操作并显示所有角色的手牌
    /// 调用服务器 draw_cards 接口，刷新角色数据后显示每个角色的手牌信息
    /// </summary>
    private IEnumerator ExecuteDrawCardsAndShowHands()
    {
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "draw_cards");

        if (_dungeonGamePlayApi.RespData == null)
        {
            if (_dungeonGamePlayApi.ReqResult != null)
            {
                Debug.LogError("ExecuteDrawCardsAndShowHands request failed: " + _dungeonGamePlayApi.ReqResult.responseText);
                _mainText.text = _dungeonGamePlayApi.ReqResult.responseText;
            }
            else
            {
                Debug.LogError("ExecuteDrawCardsAndShowHands request failed: response data is null");
            }

            yield break;
        }

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "play_cards");

        if (_dungeonGamePlayApi.RespData == null)
        {
            if (_dungeonGamePlayApi.ReqResult != null)
            {
                Debug.LogError("ExecutePlayCardsAndShowResult request failed: " + _dungeonGamePlayApi.ReqResult.responseText);
                _mainText.text = _dungeonGamePlayApi.ReqResult.responseText;
            }
            else
            {
                Debug.LogError("ExecutePlayCardsAndShowResult request failed: response data is null");
            }
            yield break;
        }

        bool fetchSuccess = false;
        yield return SessionManager.Instance.FetchSessionMessages(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    DisplayCombatArbitrationResult(sessionMessages);
                }
            }
        );

        if (!fetchSuccess)
        {
            Debug.LogError("Failed to fetch session messages in DungeonScene");
        }
    }

    /// <summary>
    /// 从会话消息中提取并显示战斗仲裁结果
    /// 查找最后一个战斗仲裁事件并显示其战斗日志和叙述内容
    /// </summary>
    /// <param name="sessionMessages">会话消息列表</param>
    private void DisplayCombatArbitrationResult(List<SessionMessage> sessionMessages)
    {
        var agentEvents = GameContext.Instance.ExtractCombatEventsFromMessages(sessionMessages);
        var arbitrationEvents = GameUtils.FilterEventsByType<CombatArbitrationEvent>(agentEvents);

        if (arbitrationEvents.Count == 0)
        {
            Debug.LogWarning("No CombatArbitrationEvent found in session messages");
            return;
        }

        var lastArbitrationEvent = arbitrationEvents[arbitrationEvents.Count - 1];
        //_mainText.text = $"[combat_log]\n{lastArbitrationEvent.combat_log}\n\n[narrative]\n{lastArbitrationEvent.narrative}";
        _mainText.text = $"[narrative]\n{lastArbitrationEvent.narrative}";
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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "advance_next_dungeon");
        if (_dungeonGamePlayApi.RespData == null)
        {
            if (_dungeonGamePlayApi.ReqResult != null)
            {
                Debug.LogError("ExecuteAdvanceNextDungeon request failed: " + _dungeonGamePlayApi.ReqResult.responseText);
                _mainText.text = _dungeonGamePlayApi.ReqResult.responseText;
            }
            else
            {
                Debug.LogError("ExecuteAdvanceNextDungeon request failed: response data is null");
            }
            yield break;
        }

        //_mainText.text = "已进入下一个地下城";
        yield return RefreshDungeonStateDisplay();
    }

    /// <summary>
    /// 返回主场景
    /// 调用服务器传送回家接口，成功后切换到主场景
    /// </summary>
    private IEnumerator ExecuteBackHome()
    {
        Debug.Log("ExecuteBackHome");
        yield return _transHomeApi.Call(GameContext.Instance.DungeonTransHomeUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        if (_transHomeApi.RespData == null)
        {
            Debug.LogError("TransHomeAction request failed");
            if (_transHomeApi.ReqResult != null)
            {
                Debug.LogError("ExecuteBackHome request failed: " + _transHomeApi.ReqResult.responseText);
                _mainText.text = _transHomeApi.ReqResult.responseText;
            }
            else
            {
                Debug.LogError("ExecuteBackHome request failed: response data is null");
            }

            yield break;
        }
        Debug.Log("TransHomeAction request success");
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_preScene);
    }
}

