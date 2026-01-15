using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonCombatScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene";
    [SerializeField] private string _nextScene = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;
    [SerializeField] private GameObject _backgroundImage; // (暂未使用)
    [SerializeField] private GameObject _actorAvatarPrefab;    // 角色头像预制体
    [SerializeField] private StringGameEvent _onActorAvatarsRefreshEvent; // 角色头像刷新事件

    [Header("API Components")]
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_actorAvatarPrefab != null, "_actorAvatarPrefab is null");
        Debug.Assert(_onActorAvatarsRefreshEvent != null, "_onActorAvatarsRefreshEvent is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

        // 检查是否已经连接服务器
        if (ApiEndpointsManager.GameRootResponse != null)
        {
            // 已经连接服务器，开始初始化战斗场景
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
            _mainText.text = $"{GameContext.Instance.Dungeon.name} | {stageName} : Initializing combat scene...";

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
    /// 执行抽卡操作并轮询任务状态，完成后显示手牌
    /// 调用服务器 draw_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示角色手牌信息
    /// </summary>
    private IEnumerator ExecuteDrawCardsAndShowHands()
    {
        // 先刷新数据，确保有最新的角色和地下城状态
        bool refreshSuccess = false;
        string refreshMessage = "";

        // 刷一次获取最新信息！
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshMessage}");
            _mainText.text = "刷新数据失败";
            yield break;
        }

        // 抓卡环节会判断血量从而改变战斗状态，所以这里要先判断战斗是否已经结束！
        // 判断战斗是否已经结束（胜利或失败），如果是则不允许继续抽卡
        if (GameUtils.IsLastCombatWin(GameContext.Instance.Dungeon))
        {
            Debug.Log("Combat already won, cannot draw cards");
            _mainText.text = "战斗已经胜利，无法继续抽卡！";
            yield break;
        }
        else if (GameUtils.IsLastCombatLose(GameContext.Instance.Dungeon))
        {
            Debug.Log("Combat already lost, cannot draw cards");
            _mainText.text = "战斗已经失败，无法继续抽卡！";
            yield break;
        }

        // 检查是否已有角色持有手牌
        if (AnyActorHasHandCards())
        {
            _mainText.text = "当前已有角色持有手牌，无法重复抽卡。";
            yield break;
        }

        // 正式的抽卡操作
        bool success = false;
        string taskId = null;

        // 开始发起抽卡请求
        yield return DungeonGamePlayManager.Instance.DrawCards(
            (result, message, id) =>
            {
                success = result;
                taskId = id;
                if (result)
                {
                    Debug.Log($"DrawCards initiated successfully, task ID: {taskId}");
                    _mainText.text = "请求已提交，正在处理中...";
                }
                else
                {
                    _mainText.text = message;
                }
            });

        if (!success || string.IsNullOrEmpty(taskId))
        {
            yield break;
        }

        // 因为是后台任务，轮询查询任务状态
        bool pollSuccess = false;
        yield return PollTaskStatus(taskId, (isSuccess, msg, taskRecord) =>
        {
            pollSuccess = isSuccess;
            if (!isSuccess)
            {
                _mainText.text = msg;
            }
        });

        // 轮询成功后显示抽卡结果
        if (!pollSuccess)
        {
            // 轮询失败
            _mainText.text = "任务轮询失败";
            yield break;
        }

        // 抽卡任务完成，刷新数据并显示手牌
        _mainText.text = "处理完成，正在加载结果...";

        // 再次刷新，因为抽卡会改变战斗状态！
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshMessage}");
            _mainText.text = "刷新数据失败";
            yield break;
        }

        // 判断战斗是否已经结束（胜利或失败）
        // isLastCombatWin = GameUtils.IsLastCombatWin(GameContext.Instance.Dungeon);
        // isLastCombatLose = GameUtils.IsLastCombatLose(GameContext.Instance.Dungeon);
        if (GameUtils.IsLastCombatWin(GameContext.Instance.Dungeon))
        {
            Debug.Log("Last combat was a WIN");
            _mainText.text = "战斗胜利！";
            yield break;
        }
        else if (GameUtils.IsLastCombatLose(GameContext.Instance.Dungeon))
        {
            Debug.Log("Last combat was a LOSE");
            _mainText.text = "战斗失败！";
            yield break;
        }
        else
        {
            Debug.Log("Last combat result is NONE");
            // 显示所有角色的手牌信息
            DisplayAllActorsHands();
        }
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
    /// 执行打牌操作并轮询任务状态，完成后显示结果
    /// 调用服务器 play_cards 接口，获取任务ID后轮询查询任务状态
    /// 当任务完成时，刷新数据并显示战斗仲裁结果
    /// </summary>
    private IEnumerator ExecutePlayCardsAndShowResult()
    {
        bool success = false;
        string taskId = null;

        yield return DungeonGamePlayManager.Instance.PlayCards(
            (result, message, id) =>
            {
                success = result;
                taskId = id;
                if (result)
                {
                    Debug.Log($"PlayCards initiated successfully, task ID: {taskId}");
                    _mainText.text = "打牌请求已提交，正在处理中...";
                }
                else
                {
                    _mainText.text = message;
                }
            });

        if (!success || string.IsNullOrEmpty(taskId))
        {
            yield break;
        }

        // 轮询查询任务状态
        bool pollSuccess = false;
        yield return PollTaskStatus(taskId, (isSuccess, msg, taskRecord) =>
        {
            pollSuccess = isSuccess;
            if (!isSuccess)
            {
                _mainText.text = msg;
            }
        });

        // 轮询成功后显示打牌结果
        if (!pollSuccess)
        {
            yield break;
        }

        _mainText.text = "打牌处理完成，正在加载结果...";

        // 刷新地下城数据
        bool refreshSuccess = false;
        string refreshMessage = "";

        // 只刷新地下城数据（不需要角色详情数据）
        yield return GameStateSync.Instance.RefreshDungeonFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon data: {refreshMessage}");
            _mainText.text = "刷新地下城数据失败";
            yield break;
        }

        // 显示最新回合信息
        DisplayLastRoundInfo();
    }

    /// <summary>
    /// 轮询查询任务状态直到完成或失败
    /// 委托 TasksStatusApi 执行轮询逻辑，完成后通过回调函数返回结果
    /// </summary>
    /// <param name="taskId">要查询的任务ID</param>
    /// <param name="onComplete">轮询完成后的回调函数，参数为(成功标志, 消息, 任务记录)</param>
    private IEnumerator PollTaskStatus(string taskId, System.Action<bool, string, TaskRecord> onComplete)
    {
        bool isSuccess = false;
        string message = "";
        TaskRecord taskRecord = null;

        // 调用 TasksStatusApi 的轮询方法，将轮询逻辑委托给API层处理
        yield return _tasksStatusApi.PollTaskStatus(
            GameContext.Instance.TasksStatusUrl,
            taskId,
            (success, msg, record) =>
            {
                isSuccess = success;
                message = msg;
                taskRecord = record;
            }
        );

        // 通过回调函数返回轮询结果
        onComplete?.Invoke(isSuccess, message, taskRecord);
    }

    /// <summary>
    /// 显示最新的地下城回合信息
    /// 获取最后一个回合并格式化显示战斗仲裁信息，如果没有回合或信息则显示相应提示
    /// </summary>
    private void DisplayLastRoundInfo()
    {
        // 获取最新的地下城回合信息
        Round round = GameUtils.GetLastRound(GameContext.Instance.Dungeon);
        if (round == null)
        {
            Debug.LogWarning("No rounds found in dungeon after playing cards");
            _mainText.text = "打牌完成，但未找到回合信息";
            return;
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
            _mainText.text = "打牌完成，但未找到战斗仲裁信息";
        }
    }

    /// <summary>
    /// 刷新并显示地下城状态
    /// 从服务器获取最新的地下城和角色数据，然后更新UI显示当前场景的角色分布和战斗信息
    /// </summary>
    private IEnumerator RefreshDungeonStateDisplay()
    {
        bool refreshSuccess = false;
        string refreshMessage = "";

        // 从服务器刷新地下城和角色数据
        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshMessage}");
            _mainText.text = "刷新地下城状态失败";
            yield break;
        }

        // 获取当前角色所在场景及该场景中的所有角色
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
        Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");

        // 需要所有的角色名称列表！
        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);

        // 格式化并显示地下城状态（包括场景-角色映射和战斗序列信息）
        _mainText.text = GameUtils.FormatDungeonStateDisplay(GameContext.Instance.Dungeon, new Dictionary<string, List<string>> { { stageName, actorsInStage } });

        // 更新背景图片
        UpdateBackgroundImage();
    }

    /// <summary>
    /// 更新场景背景图片
    /// 根据当前角色所在场景从缓存中获取并更新背景图片，如果未找到则清空背景
    /// </summary>
    private void UpdateBackgroundImage()
    {
        // 获取当前角色所在场景
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActor);
        Debug.Assert(stageName != "", "[DungeonCombatScene] Current actor's stage name is empty");

        var cachedSprite = SpriteCacheManager.Instance.GetSprite(stageName);
        if (cachedSprite != null)
        {
            _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {stageName}");
            _backgroundImage.GetComponent<Image>().sprite = null;
        }
    }

    /// <summary>
    /// 查看并显示所有角色的战斗属性
    /// 从服务器刷新数据后，获取所有角色的战斗属性组件并格式化显示
    /// </summary>
    private IEnumerator ExecuteViewActorStats()
    {
        bool refreshSuccess = false;
        string refreshMessage = "";

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshMessage}");
            _mainText.text = "刷新角色数据失败";
            yield break;
        }

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
        bool refreshSuccess = false;
        string refreshMessage = "";

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer((success, msg) =>
        {
            refreshSuccess = success;
            refreshMessage = msg;
        });

        // 检查刷新是否成功
        if (!refreshSuccess)
        {
            Debug.LogError($"Failed to refresh dungeon and actors data: {refreshMessage}");
            _mainText.text = "刷新数据失败";
            yield break;
        }

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
            yield return new WaitForSeconds(0);
            SceneManager.LoadScene(_preScene);
        }
    }

    /// <summary>
    /// 检查是否有任意角色持有手牌
    /// 遍历所有角色实体，检查其手牌组件是否包含卡牌
    /// 用于在抽卡操作前判断是否需要跳过抽卡（避免重复抽卡）
    /// </summary>
    /// <returns>如果至少有一个角色持有手牌则返回 true，否则返回 false</returns>
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
}

