using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class BootScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _baseUrl = "http://192.168.2.121:8000/";

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "LoginScene";

    [Header("API Components")]
    [SerializeField] private RootApi _rootApi;

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
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator InitializeApiEndpoints()
    {
        yield return _rootApi.Call(_baseUrl);
        if (_rootApi.RespData == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}");
            yield break;
        }

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
}

