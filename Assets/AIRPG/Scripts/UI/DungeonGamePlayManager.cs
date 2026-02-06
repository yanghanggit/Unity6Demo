using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

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
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator CombatInit(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 combat_init 端点
        yield return _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.INIT_COMBAT);

        // 检查API调用是否成功
        if (_dungeonProgressApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonProgressApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonProgressApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");

        Debug.Log("[DungeonGamePlayManager] CombatInit completed successfully");
        onComplete?.Invoke(true, "Combat init completed successfully", _dungeonProgressApi.RespData.session_messages);
    }

    /// <summary>    
    /// 状态评估
    /// 调用 status_evaluation 端点进行状态评估
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator CombatStatusEvaluation(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 status_evaluation 端点
        yield return _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.COMBAT_STATUS_EVALUATION);

        // 检查API调用是否成功
        if (_dungeonProgressApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonProgressApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonProgressApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");

        Debug.Log("[DungeonGamePlayManager] StatusEvaluation completed successfully");
        onComplete?.Invoke(true, "Status evaluation completed successfully", _dungeonProgressApi.RespData.session_messages);
    }

    /// <summary>    /// 抽卡
    /// 调用地下城战斗抽牌端点执行抽卡操作
    /// </summary>
    /// <param name="specifiedActions">指定的盟友抽卡行动列表</param>
    /// <param name="enable_enemy_draw">是否启用敌人抽牌</param>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和任务ID</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator DrawCards(List<AllyDrawCardAction> specifiedActions, bool enable_enemy_draw, Action<bool, string, string> onComplete = null)
    {
        // 调用地下城战斗抽牌端点
        yield return _dungeonCombatDrawCardsApi.Call(
            GameContext.Instance.DungeonCombatDrawCardsUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            specifiedActions,
            enable_enemy_draw);

        // 检查API调用是否成功
        if (_dungeonCombatDrawCardsApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonCombatDrawCardsApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonCombatDrawCardsApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonCombatDrawCardsApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonCombatDrawCardsApi.RespData != null, "DungeonCombatDrawCardsApi response data is null");
        if (
            string.IsNullOrEmpty(_dungeonCombatDrawCardsApi.RespData.task_id) ||
            _dungeonCombatDrawCardsApi.RespData.status != TaskStatus.RUNNING)
        {
            string errorMsg = "DungeonCombatDrawCardsApi response data is invalid";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] DrawCards completed successfully");
        onComplete?.Invoke(true, "Draw cards completed successfully", _dungeonCombatDrawCardsApi.RespData.task_id);
    }

    /// <summary>
    /// 打牌
    /// 调用 play_cards 端点执行打牌操作
    /// </summary>
    /// <returns>协程迭代器</returns>
    public IEnumerator PlayCards(Action<bool, string, string> onComplete = null)
    {
        // 调用地下城战斗打牌端点
        yield return _dungeonCombatPlayCardsApi.Call(
            GameContext.Instance.DungeonCombatPlayCardsUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        // 检查API调用是否成功,
        if (_dungeonCombatPlayCardsApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonCombatPlayCardsApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonCombatPlayCardsApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonCombatPlayCardsApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonCombatPlayCardsApi.RespData != null, "DungeonCombatPlayCardsApi response data is null");
        if (
            string.IsNullOrEmpty(_dungeonCombatPlayCardsApi.RespData.task_id) ||
            _dungeonCombatPlayCardsApi.RespData.status != TaskStatus.RUNNING)
        {
            string errorMsg = "DungeonCombatPlayCardsApi response data is invalid";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        Debug.Log("[DungeonGamePlayManager] PlayCards completed successfully");
        onComplete?.Invoke(true, "Play cards completed successfully", _dungeonCombatPlayCardsApi.RespData.task_id);
    }

    /// <summary>
    /// 前进到下一个地下城
    /// 调用 advance_next_dungeon 端点推进地下城进度
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator AdvanceNextDungeon(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 advance_next_dungeon 端点
        yield return _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.ADVANCE_STAGE);

        // 检查API调用是否成功
        if (_dungeonProgressApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonProgressApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonProgressApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");

        Debug.Log("[DungeonGamePlayManager] AdvanceNextDungeon completed successfully");
        onComplete?.Invoke(true, "Advance dungeon completed successfully", _dungeonProgressApi.RespData.session_messages);
    }

    /// <summary>
    /// 从地下城撤退
    /// 调用 retreat 端点从地下城撤退
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator RetreatFromDungeon(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 retreat 端点
        yield return _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.RETREAT);

        // 检查API调用是否成功
        if (_dungeonProgressApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonProgressApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonProgressApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");

        Debug.Log("[DungeonGamePlayManager] RetreatFromDungeon completed successfully");
        onComplete?.Invoke(true, "Retreat from dungeon completed successfully", _dungeonProgressApi.RespData.session_messages);
    }

    /// <summary>    /// 战斗后处理
    /// 调用 post_combat 端点进行战斗后处理
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator PostCombat(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 post_combat 端点
        yield return _dungeonProgressApi.Call(
            GameContext.Instance.DungeonProgressUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            DungeonProgressType.POST_COMBAT);

        // 检查API调用是否成功
        if (_dungeonProgressApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonProgressApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonProgressApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonProgressApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonProgressApi.RespData != null, "DungeonProgressApi response data is null");

        Debug.Log("[DungeonGamePlayManager] PostCombat completed successfully");
        onComplete?.Invoke(true, "Post combat completed successfully", _dungeonProgressApi.RespData.session_messages);
    }

    /// <summary>    /// 传送回家
    /// 调用传送回家端点，返回主场景
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功和消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator TransHome(Action<bool, string> onComplete = null)
    {
        // 调用传送回家端点
        yield return _transHomeApi.Call(
            GameContext.Instance.DungeonTransHomeUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName);

        // 检查API调用是否成功
        if (_transHomeApi.ReqResult == null)
        {
            string errorMsg = "TransHomeApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        if (!_transHomeApi.ReqResult.isSuccess)
        {
            string errorMsg = _transHomeApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg);
            yield break;
        }

        Debug.Assert(_transHomeApi.RespData != null, "TransHomeApi response data is null");

        Debug.Log("[DungeonGamePlayManager] TransHome completed successfully");
        onComplete?.Invoke(true, "Trans home completed successfully");
    }
}
