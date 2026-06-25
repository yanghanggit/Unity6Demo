using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaunchScreenController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;

    void Start()
    {
        Debug.Assert(_button != null, "_button is null");
    }

    /// <summary>
    /// 点击事件处理函数
    /// </summary>
    public void OnClick()
    {
        LoadAsync().Forget();
    }

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
}

