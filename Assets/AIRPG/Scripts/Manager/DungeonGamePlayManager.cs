using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Dungeon游戏玩法管理器
/// 单例模式，封装所有Dungeon相关的游戏操作（POST请求）
/// 负责战斗初始化、抽卡、打牌、地下城推进、传送回家等写操作
/// 不负责会话消息同步，由调用方根据需要自行处理
/// </summary>
public class DungeonGamePlayManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static DungeonGamePlayManager Instance { get; private set; }

    /// <summary>
    /// Dungeon进度API接口
    /// </summary>
    [SerializeField] private DungeonProgressApi _dungeonProgressApi;

    /// <summary>
    /// 传送回家API接口
    /// </summary>
    [SerializeField] private TransHomeApi _transHomeApi;

    /// <summary>
    /// Dungeon战斗打牌API接口
    /// </summary>
    [SerializeField] private DungeonCombatPlayCardsApi _dungeonCombatPlayCardsApi;

    /// <summary>
    /// Dungeon战斗抽牌API接口
    /// </summary>
    [SerializeField] private DungeonCombatDrawCardsApi _dungeonCombatDrawCardsApi;

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("[DungeonGamePlayManager] Duplicate instance detected, destroying the new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_dungeonProgressApi != null, "_dungeonProgressApi is null");
        Debug.Assert(_transHomeApi != null, "_transHomeApi is null");
        Debug.Assert(_dungeonCombatPlayCardsApi != null, "_dungeonCombatPlayCardsApi is null");
        Debug.Assert(_dungeonCombatDrawCardsApi != null, "_dungeonCombatDrawCardsApi is null");
    }

    /// <summary>
    /// 初始化战斗
    /// 调用 combat_init 端点开始战斗
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> CombatInit()
    {
        await _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.INIT_COMBAT);

        if (_dungeonProgressApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] CombatInit: request result is null");
            return null;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] CombatInit failed: {_dungeonProgressApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");
        Debug.Log("[DungeonGamePlayManager] CombatInit completed successfully");
        return _dungeonProgressApi.RespData.session_messages;
    }

    /// <summary>
    /// 状态评估
    /// 调用 status_evaluation 端点进行状态评估
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> CombatStatusEvaluation()
    {
        await _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.COMBAT_STATUS_EVALUATION);

        if (_dungeonProgressApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] CombatStatusEvaluation: request result is null");
            return null;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] CombatStatusEvaluation failed: {_dungeonProgressApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");
        Debug.Log("[DungeonGamePlayManager] CombatStatusEvaluation completed successfully");
        return _dungeonProgressApi.RespData.session_messages;
    }

    /// <summary>
    /// 抽卡
    /// 调用地下城战斗抽牌端点执行抽卡操作
    /// </summary>
    /// <returns>任务 ID 字符串，失败时返回 null</returns>
    public async UniTask<string> DrawCards(List<AllyDrawCardAction> specifiedActions, bool enable_enemy_draw)
    {
        await _dungeonCombatDrawCardsApi.Call(
            GameContext.Instance.DungeonCombatDrawCardsUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            specifiedActions,
            enable_enemy_draw);

        if (_dungeonCombatDrawCardsApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] DrawCards: request result is null");
            return null;
        }

        if (!_dungeonCombatDrawCardsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] DrawCards failed: {_dungeonCombatDrawCardsApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonCombatDrawCardsApi.RespData != null, "DungeonCombatDrawCardsApi response data is null");
        if (string.IsNullOrEmpty(_dungeonCombatDrawCardsApi.RespData.task_id) ||
            _dungeonCombatDrawCardsApi.RespData.status != TaskStatus.RUNNING)
        {
            Debug.LogError("[DungeonGamePlayManager] DrawCards: response data is invalid");
            return null;
        }

        Debug.Log("[DungeonGamePlayManager] DrawCards completed successfully");
        return _dungeonCombatDrawCardsApi.RespData.task_id;
    }

    /// <summary>
    /// 打牌
    /// 调用 play_cards 端点执行打牌操作
    /// </summary>
    /// <returns>任务 ID 字符串，失败时返回 null</returns>
    public async UniTask<string> PlayCards()
    {
        await _dungeonCombatPlayCardsApi.Call(
            GameContext.Instance.DungeonCombatPlayCardsUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        if (_dungeonCombatPlayCardsApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] PlayCards: request result is null");
            return null;
        }

        if (!_dungeonCombatPlayCardsApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] PlayCards failed: {_dungeonCombatPlayCardsApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonCombatPlayCardsApi.RespData != null, "DungeonCombatPlayCardsApi response data is null");
        if (string.IsNullOrEmpty(_dungeonCombatPlayCardsApi.RespData.task_id) ||
            _dungeonCombatPlayCardsApi.RespData.status != TaskStatus.RUNNING)
        {
            Debug.LogError("[DungeonGamePlayManager] PlayCards: response data is invalid");
            return null;
        }

        Debug.Log("[DungeonGamePlayManager] PlayCards completed successfully");
        return _dungeonCombatPlayCardsApi.RespData.task_id;
    }

    /// <summary>
    /// 前进到下一个地下城
    /// 调用 advance_next_dungeon 端点推进地下城进度
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> AdvanceNextDungeon()
    {
        await _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.ADVANCE_STAGE);

        if (_dungeonProgressApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] AdvanceNextDungeon: request result is null");
            return null;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] AdvanceNextDungeon failed: {_dungeonProgressApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");
        Debug.Log("[DungeonGamePlayManager] AdvanceNextDungeon completed successfully");
        return _dungeonProgressApi.RespData.session_messages;
    }

    /// <summary>
    /// 从地下城撤退
    /// 调用 retreat 端点从地下城撤退
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> RetreatFromDungeon()
    {
        await _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.RETREAT);

        if (_dungeonProgressApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] RetreatFromDungeon: request result is null");
            return null;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] RetreatFromDungeon failed: {_dungeonProgressApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");
        Debug.Log("[DungeonGamePlayManager] RetreatFromDungeon completed successfully");
        return _dungeonProgressApi.RespData.session_messages;
    }

    /// <summary>
    /// 战斗后处理
    /// 调用 post_combat 端点进行战斗后处理
    /// </summary>
    /// <returns>会话消息列表，失败时返回 null</returns>
    public async UniTask<List<SessionMessage>> PostCombat()
    {
        await _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.POST_COMBAT);

        if (_dungeonProgressApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] PostCombat: request result is null");
            return null;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] PostCombat failed: {_dungeonProgressApi.ReqResult.responseText}");
            return null;
        }

        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");
        Debug.Log("[DungeonGamePlayManager] PostCombat completed successfully");
        return _dungeonProgressApi.RespData.session_messages;
    }

    /// <summary>
    /// 传送回家
    /// 调用传送回家端点，返回主场景
    /// </summary>
    /// <returns>成功返回 true，失败返回 false</returns>
    public async UniTask<bool> TransHome()
    {
        await _transHomeApi.Call(
            GameContext.Instance.DungeonTransHomeUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        if (_transHomeApi.ReqResult == null)
        {
            Debug.LogError("[DungeonGamePlayManager] TransHome: request result is null");
            return false;
        }

        if (!_transHomeApi.ReqResult.isSuccess)
        {
            Debug.LogError($"[DungeonGamePlayManager] TransHome failed: {_transHomeApi.ReqResult.responseText}");
            return false;
        }

        Debug.Assert(_transHomeApi.RespData != null, "TransHomeApi response data is null");
        Debug.Log("[DungeonGamePlayManager] TransHome completed successfully");
        return true;
    }
}
