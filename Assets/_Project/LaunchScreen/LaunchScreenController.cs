using UnityEngine;
// using UnityEngine.SceneManagement;
// using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class LaunchScreenController : MonoBehaviour
{
    // public static readonly string NextSceneName = "LoginScene";

    // [Header("Network Settings")]
    // [SerializeField] private string _baseUrl = "http://192.168.2.134:8000/";

    // [Header("API Components")]
    // [SerializeField] private RootApi _rootApi;

    [Header("UI Components")]
    [SerializeField] private Button _button;

    void Start()
    {
        Debug.Assert(_button != null, "_button is null");
        // Debug.Assert(_loginButton != null, "_loginButton is null");
        // Debug.Assert(!string.IsNullOrEmpty(_baseUrl), "_baseUrl is null");

        // 先隐藏。
        //_button.gameObject.SetActive(false);

        // 异步初始化API端点配置，确保在完成后才激活登录按钮
        //InitializeAsync().Forget();

    }

    /// <summary>
    /// 点击事件处理函数
    /// </summary>
    public void OnClick()
    {
        //LoadLoginScene().Forget();
        Debug.Log("Button clicked, proceeding to the next scene...");
    }

    // /// <summary>
    // /// 异步初始化API端点配置
    // /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    // /// </summary>
    // /// <returns>协程迭代器</returns>
    // private async UniTaskVoid InitializeAsync()
    // {
    //     var rootApiResponse = await _rootApi.Call(_baseUrl);
    //     if (rootApiResponse == null)
    //     {
    //         Debug.LogError($"Failed to initialize API endpoints from {_baseUrl}: rootApiResponse is null");

    //         // 尝试获取更详细的错误信息，如果可用的话
    //         if (_rootApi.TryGetErrorDetail(out var httpStatusCode, out var errorDetail))
    //         {
    //             Debug.LogError($"Root API call error details - HTTP Status Code: {httpStatusCode}, Detail: {errorDetail}");
    //         }
    //         return;
    //     }

    //     Debug.Log("API endpoints initialized successfully from root data");
    //     Debug.Assert(_rootApi.RespData != null, "RootApi response data is null");

    //     // 激活登录按钮，允许用户进入下一步
    //     _loginButton.gameObject.SetActive(true);

    //     // 设置全局游戏上下文的基础URL和根响应数据
    //     GameContext.BaseUrl = _baseUrl;
    // }

    // /// <summary>
    // /// 异步加载登录场景
    // /// 使用协程实现场景切换，确保流畅的用户体验
    // /// </summary>
    // /// <returns>协程迭代器</returns>
    // private async UniTaskVoid LoadLoginScene()
    // {
    //     await UniTask.Yield();
    //     SceneManager.LoadScene(NextSceneName);
    // }
}

