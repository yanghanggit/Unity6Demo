using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LaunchScreenController : MonoBehaviour
{
    private const string PlayerLobbySceneName = "PlayerLobby";

    [Header("UI Components")]
    [SerializeField] private Button _button;

    void Start()
    {
        Debug.Assert(_button != null, "_button is null");

        LoadAsync().Forget();
    }

    /// <summary>
    /// 点击事件处理函数：切入大厅场景
    /// </summary>
    public void OnClick()
    {
        EnterLobbyAsync().Forget();
    }

    /// <summary>
    /// 异步加载服务器信息
    /// </summary>
    private async UniTaskVoid LoadAsync()
    {
        _button.interactable = false;
        try
        {
            JObject info = await GameServerClientHolder.Instance.Client.FetchServerInfoAsync(
                this.GetCancellationTokenOnDestroy());
            Debug.Log($"Server info: {info}");
        }
        catch (GameServerClient.ServerException ex)
        {
            Debug.LogError($"无法连接服务器 [{ex.StatusCode}]: {ex.Message}");
        }
        finally
        {
            _button.interactable = true;
        }
    }

    /// <summary>
    /// 异步切入大厅场景
    /// </summary>
    private async UniTaskVoid EnterLobbyAsync()
    {
        _button.interactable = false;
        await SceneManager.LoadSceneAsync(PlayerLobbySceneName);
    }
}

