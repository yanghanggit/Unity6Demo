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
    /// Dungeon游戏玩法API接口
    /// </summary>
    [SerializeField] private DungeonGamePlayApi _dungeonGamePlayApi;

    /// <summary>
    /// 传送回家API接口
    /// </summary>
    [SerializeField] private TransHomeApi _transHomeApi;

    private void Awake()
    {
        // 单例模式处理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Debug.Assert(_dungeonGamePlayApi != null, "_dungeonGamePlayApi is null");
        Debug.Assert(_transHomeApi != null, "_transHomeApi is null");
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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "combat_init");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonGamePlayApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonGamePlayApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonGamePlayApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonGamePlayApi.RespData != null, "DungeonGamePlayApi response data is null");

        Debug.Log("[DungeonGamePlayManager] CombatInit completed successfully");
        onComplete?.Invoke(true, "Combat init completed successfully", _dungeonGamePlayApi.RespData.session_messages);
    }

    /// <summary>
    /// 抽卡
    /// 调用 draw_cards 端点执行抽卡操作
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功、消息和会话消息列表</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator DrawCards(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 draw_cards 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "draw_cards");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonGamePlayApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonGamePlayApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonGamePlayApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonGamePlayApi.RespData != null, "DungeonGamePlayApi response data is null");

        //Debug.Log("[DungeonGamePlayManager] DrawCards completed successfully");
        onComplete?.Invoke(true, "Draw cards completed successfully", _dungeonGamePlayApi.RespData.session_messages);
    }

    /// <summary>
    /// 打牌
    /// 调用 play_cards 端点执行打牌操作
    /// </summary>
    /// <param name="onComplete">完成回调，参数为是否成功和消息</param>
    /// <returns>协程迭代器</returns>
    public IEnumerator PlayCards(Action<bool, string, List<SessionMessage>> onComplete = null)
    {
        // 调用 play_cards 端点
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "play_cards");

        // 检查API调用是否成功,
        if (_dungeonGamePlayApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonGamePlayApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonGamePlayApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonGamePlayApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonGamePlayApi.RespData != null, "DungeonGamePlayApi response data is null");

        Debug.Log("[DungeonGamePlayManager] PlayCards completed successfully");
        onComplete?.Invoke(true, "Play cards completed successfully", _dungeonGamePlayApi.RespData.session_messages);
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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGamePlayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "advance_next_dungeon");

        // 检查API调用是否成功
        if (_dungeonGamePlayApi.ReqResult == null)
        {
            // 没有任何请求结果，这就是不需要继续的！
            string errorMsg = "DungeonGamePlayApi request result is null";
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        if (!_dungeonGamePlayApi.ReqResult.isSuccess)
        {
            string errorMsg = _dungeonGamePlayApi.ReqResult.responseText;
            Debug.LogError($"[DungeonGamePlayManager] {errorMsg}");
            onComplete?.Invoke(false, errorMsg, null);
            yield break;
        }

        // 必有响应数据，即使是[]
        Debug.Assert(_dungeonGamePlayApi.RespData != null, "DungeonGamePlayApi response data is null");

        Debug.Log("[DungeonGamePlayManager] AdvanceNextDungeon completed successfully");
        onComplete?.Invoke(true, "Advance dungeon completed successfully", _dungeonGamePlayApi.RespData.session_messages);
    }

    /// <summary>
    /// 传送回家
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
