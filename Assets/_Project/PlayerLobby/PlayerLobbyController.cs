using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerLobbyController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _playerIdText;
    [SerializeField] private TMP_Text _gameNameText;

    private const string NextSceneName = "HomeScene"; // TODO: 待确认目标场景
    private string _randomPlayerId = null;
    private const string _gameName = "Game1";

    void Awake()
    {
        Debug.Assert(_playerIdText != null, "_playerIdText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
    }

    void Start()
    {
        // 初始化玩家ID并显示在UI上
        if (string.IsNullOrEmpty(_randomPlayerId))
        {
            System.DateTime now = System.DateTime.Now;
            string timestamp = now.ToString("yyyyMMddHHmmss");
            _randomPlayerId = "unity-player-" + timestamp;
        }

        // 显示玩家ID和游戏名在UI上
        _playerIdText.text = $"玩家ID: {_randomPlayerId}";
        _gameNameText.text = $"游戏名: {_gameName}";
    }

    /// <summary>
    /// 点击新游戏按钮的回调
    /// </summary>
    public void OnClickNewGame()
    {
        StartNewGameAsync().Forget();
    }

    /// <summary>
    /// 异步启动新游戏
    /// </summary>
    private async UniTaskVoid StartNewGameAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        var client = GameManager.Instance.ServerClient;
        try
        {
            // 登录并创建新游戏
            await client.LoginAsync(_randomPlayerId, _gameName, ct);
            await client.NewGameAsync(_randomPlayerId, _gameName, ct);

            GameManager.Instance.UserName = _randomPlayerId;
            GameManager.Instance.GameName = _gameName;

            await SceneManager.LoadSceneAsync(NextSceneName);
        }
        catch (GameServerClient.ServerException ex)
        {
            Debug.LogError($"新游戏失败 [{ex.StatusCode}]: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"新游戏未知错误: {ex.Message}");
        }
    }
}

