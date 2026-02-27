using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class LaunchScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _gameApiBaseUrl = "http://192.168.2.134:8000/";

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "LoginScene";

    [Header("API Components")]
    [SerializeField] private RootApi _rootApi;

    [Header("UI Components")]
    [SerializeField] private Button _loginButton;

    [Header("Settings")]

    [Tooltip("是否在启动时清除本地存储的PlayerPrefs, 注意！仅用于测试用途！")]
    [SerializeField] private bool _clearPlayerPrefsOnStart = true;

    /// <summary>
    /// Unity生命周期方法：初始化启动场景
    /// 验证必要组件的有效性，隐藏登录按钮并开始API端点初始化
    /// </summary>
    void Start()
    {
        // 测试用！
        if (_clearPlayerPrefsOnStart)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save(); // 确保删除操作在 WebGL 中被立即保存
            Debug.Log("LaunchScene: Cleared PlayerPrefs for testing purposes.");
        }

        Debug.Assert(_rootApi != null, "_rootApi is null");
        //Debug.Assert(_imageRootApi != null, "_imageRootApi is null");
        Debug.Assert(_loginButton != null, "_loginButton is null");
        Debug.Assert(!string.IsNullOrEmpty(_gameApiBaseUrl), "_baseUrl is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");

        _loginButton.gameObject.SetActive(false);
        InitializeGameApiEndpoints().Forget();
    }

    /// <summary>
    /// 处理登录按钮点击事件
    /// 启动登录场景加载流程
    /// </summary>
    public void OnLoginButtonClick()
    {
        LoadLoginScene().Forget();
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid InitializeGameApiEndpoints()
    {
        await _rootApi.Call(_gameApiBaseUrl);

        if (_rootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_gameApiBaseUrl}: request result is null");
            return;
        }

        if (!_rootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_gameApiBaseUrl}: {_rootApi.ReqResult.responseText}");
            return;
        }

        Debug.Assert(_rootApi.RespData != null, "RootApi response data is null");

        _loginButton.gameObject.SetActive(true);

        // 设置全局游戏上下文的基础URL和根响应数据
        GameApiEndpointsManager.BaseUrl = _gameApiBaseUrl;
    }

    /// <summary>
    /// 异步加载登录场景
    /// 使用协程实现场景切换，确保流畅的用户体验
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid LoadLoginScene()
    {
        await UniTask.Yield();
        SceneManager.LoadScene(_nextSceneName);
    }
}

