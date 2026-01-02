using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class LoginScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _userNameText;
    [SerializeField] private TMP_Text _gameNameText;

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "MainScene";
    [SerializeField] private string _gameName = "Game1";

    [Header("API Components")]
    [SerializeField] private TaskTriggerApi _taskTriggerApi;
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    private string _playerIdentifier;

    void Start()
    {
        Debug.Assert(_userNameText != null, "_userNameText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
        Debug.Assert(!string.IsNullOrEmpty(_gameName), "_gameName is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");
        Debug.Assert(_taskTriggerApi != null, "_taskTriggerApi is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

        _playerIdentifier = GeneratePlayerId();
        _userNameText.text = "临时ID = " + _playerIdentifier;
        _gameNameText.text = "测试游戏 = " + _gameName;
    }

    /// <summary>
    /// 根据当前时间戳生成唯一的玩家ID
    /// </summary>
    private string GeneratePlayerId()
    {
        System.DateTime now = System.DateTime.Now;
        string timestamp = now.ToString("yyyyMMddHHmmss");
        string randomUserName = "unity-player-" + timestamp;
        return randomUserName;
    }

    /// <summary>
    /// 点击开始游戏按钮的回调
    /// </summary>
    public void OnStartGameClicked()
    {
        StartCoroutine(StartGameFlow(_playerIdentifier, _gameName));
    }

    /// <summary>
    /// 执行登录并开始游戏的完整流程：登录 -> 开始游戎 -> 同步状态 -> 加载场景
    /// </summary>
    private IEnumerator StartGameFlow(string userName, string gameName)
    {
        // 1. 使用 SessionManager 执行登录和开始游戏
        bool sessionSuccess = false;
        yield return SessionManager.Instance.LoginAndStart(
            userName,
            gameName,
            (success) => sessionSuccess = success
        );

        // 检查会话是否成功
        if (!sessionSuccess)
        {
            Debug.LogError("[LoginScene] LoginAndStart failed");
            yield break;
        }

        // 2. 刷新全局游戏状态
        yield return GameStateSync.Instance.RefreshMappingAndEntitiesFromServer();

        // 3. 验证所有 Actor 的精灵资源
        ValidateActorSprites();

        // 4. 触发任务 API 调用
        //yield return TriggerTask();

        // 4. 切换场景
        SceneManager.LoadScene(_nextSceneName);
    }

    /// <summary>
    /// 验证所有 Actor 实体的精灵资源是否可用
    /// </summary>
    private void ValidateActorSprites()
    {
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var entity = actorEntitiesSerialization[i];
            //Debug.Log($"[LoginScene] Actor Entity {i}: {entity.ToString()}");
            var actorSprite = TextureManager.Instance.GetSprite(entity.name);
            Debug.Assert(actorSprite != null, $"Actor sprite is null for entity: {entity.name}");
        }
    }

    /// <summary>
    /// 触发任务API调用
    /// 调用任务触发端点，发起异步任务执行
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator TestTriggerTask()
    {
        string url = GameContext.Instance.TasksTriggerUrl;
        yield return _taskTriggerApi.Call(url);

        if (_taskTriggerApi.ReqResult == null)
        {
            Debug.LogError($"Failed to trigger task from {url}: request result is null");
            yield break;
        }

        if (!_taskTriggerApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to trigger task from {url}: {_taskTriggerApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_taskTriggerApi.RespData != null, "TaskTriggerApi response data is null");
        Debug.Log($"Task triggered successfully. Task ID: {_taskTriggerApi.RespData.task_id}, Status: {_taskTriggerApi.RespData.status}");

        //等待10秒
        yield return new WaitForSeconds(10.0f);

        //调用 QueryTasksStatus 来获取 _taskTriggerApi.RespData.task_id 的状态
        List<string> taskIds = new() { _taskTriggerApi.RespData.task_id };
        yield return QueryTasksStatus(taskIds);
    }

    /// <summary>
    /// 查询多个任务的执行状态
    /// 调用任务状态查询端点，获取指定任务ID列表的当前状态信息
    /// </summary>
    /// <param name="taskIds">要查询的任务ID列表</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator QueryTasksStatus(List<string> taskIds)
    {
        string url = GameContext.Instance.TasksStatusUrl;
        yield return _tasksStatusApi.Call(url, taskIds);

        if (_tasksStatusApi.ReqResult == null)
        {
            Debug.LogError($"Failed to query task status from {url}: request result is null");
            yield break;
        }

        if (!_tasksStatusApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to query task status from {url}: {_tasksStatusApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_tasksStatusApi.RespData != null, "TasksStatusApi response data is null");
        foreach (var taskStatus in _tasksStatusApi.RespData.tasks)
        {
            Debug.Log($"Task ID: {taskStatus.task_id}, Status: {taskStatus.status}");
        }
    }
}
