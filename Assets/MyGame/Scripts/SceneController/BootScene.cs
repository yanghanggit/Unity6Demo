using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BootScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _baseUrl = "http://192.168.2.121:8000/";

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "LoginScene";

    [Header("API Components")]
    [SerializeField] private RootApi _rootApi;
    [SerializeField] private TaskTriggerApi _taskTriggerApi;
    [SerializeField] private TasksStatusApi _tasksStatusApi;

    [Header("UI Components")]
    [SerializeField] private Button _loginButton;

    /// <summary>
    /// Unity生命周期方法：初始化启动场景
    /// 验证必要组件的有效性，隐藏登录按钮并开始API端点初始化
    /// </summary>
    void Start()
    {
        Debug.Assert(_rootApi != null, "_rootApi is null");
        Debug.Assert(_loginButton != null, "_loginButton is null");
        Debug.Assert(!string.IsNullOrEmpty(_baseUrl), "_baseUrl is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");
        Debug.Assert(_taskTriggerApi != null, "_taskTriggerApi is null");
        Debug.Assert(_tasksStatusApi != null, "_tasksStatusApi is null");

        _loginButton.gameObject.SetActive(false);
        StartCoroutine(InitializeApiEndpoints());
    }

    /// <summary>
    /// 处理登录按钮点击事件
    /// 启动登录场景加载流程
    /// </summary>
    public void OnLoginButtonClick()
    {
        StartCoroutine(LoadLoginScene());
        //StartCoroutine(TriggerTask());
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator InitializeApiEndpoints()
    {
        yield return _rootApi.Call(_baseUrl);

        if (_rootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: request result is null");
            yield break;
        }

        if (!_rootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: {_rootApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_rootApi.RespData != null, "RootApi response data is null");

        _loginButton.gameObject.SetActive(true);

        RootResp.Set(_rootApi.RespData);
    }

    /// <summary>
    /// 异步加载登录场景
    /// 使用协程实现场景切换，确保流畅的用户体验
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator LoadLoginScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextSceneName);
    }

    /// <summary>
    /// 触发任务API调用
    /// 调用任务触发端点，发起异步任务执行
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator TriggerTask()
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

