using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerLobbyController : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private PanelRenderer _panelRenderer;

    // 内部数据
    private const string NextSceneName = "TestLanding";
    private const string MockNextSceneName = "TestLanding";
    private string _randomPlayerId = null;
    private const string _fixedNewGameName = "Game1";

    // OnEnable/OnDisable 用于注册 Panel 加载回调，比 Start() 更可靠
    // （UXML 是异步加载的，Start() 时刻 Panel 可能还未就绪）
    void OnEnable()
    {
        if (_panelRenderer != null)
            _panelRenderer.RegisterUIReloadCallback(OnPanelLoaded);
    }

    void OnDisable()
    {
        if (_panelRenderer != null)
            _panelRenderer.UnregisterUIReloadCallback(OnPanelLoaded);
    }

    /// <summary>
    /// Panel 加载完成回调：查询 UI 元素并初始化显示
    /// </summary>
    void OnPanelLoaded(PanelRenderer pr, VisualElement root)
    {
        var playerIdText = root.Q<Label>("player-id-text");
        var gameNameText = root.Q<Label>("game-name-text");
        var newGameButton = root.Q<Button>("btn-new-game");
        Debug.Assert(playerIdText != null, "playerIdText is null");
        Debug.Assert(gameNameText != null, "gameNameText is null");
        Debug.Assert(newGameButton != null, "newGameButton is null");

        newGameButton.clicked += OnClickNewGame;

        // 初始化玩家ID并显示在UI上
        if (string.IsNullOrEmpty(_randomPlayerId))
        {
            System.DateTime now = System.DateTime.Now;
            string timestamp = now.ToString("yyyyMMddHHmmss");
            _randomPlayerId = "unity-player-" + timestamp;
        }

        // 显示玩家ID和游戏名在UI上
        playerIdText.text = $"玩家ID: {_randomPlayerId}";
        gameNameText.text = $"游戏名: {_fixedNewGameName}";
    }

    /// <summary>
    /// 点击新游戏按钮的回调
    /// </summary>
    public void OnClickNewGame()
    {
        if (GameManager.Instance.IsServerConnected)
        {
            StartNewGameAsync().Forget(); // 正常流程：启动新游戏
        }
        else
        {
            MockStartNewGameAsync().Forget(); // 模拟新游戏创建成功（用于测试）
        }
    }


    /// <summary>
    /// 异步启动新游戏
    /// </summary>
    private async UniTaskVoid StartNewGameAsync()
    {
        // 获取取消令牌和服务器客户端
        var ct = this.GetCancellationTokenOnDestroy();
        var client = GameManager.Instance.ServerClient;
        try
        {
            // 登录
            await client.LoginAsync(_randomPlayerId, _fixedNewGameName, ct);

            // 创建新游戏
            var newGameResponse = await client.NewGameAsync(_randomPlayerId, _fixedNewGameName, ct);
            Debug.Log($"NewGameResponse blueprint: {newGameResponse.blueprint}");

            // 两步均成功后再写入 Session
            GameManager.Instance.SetSession(newGameResponse.player_session, newGameResponse.blueprint);
            Debug.Log($"新游戏创建成功，玩家ID: {GameManager.Instance.Session.UserName}, 角色: {GameManager.Instance.Session.ActorName}, 游戏名: {GameManager.Instance.Session.GameName}");

            // 跳转到下一个场景
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

    /// <summary>
    /// 模拟点击新游戏按钮的回调（用于测试）
    /// </summary>
    private async UniTaskVoid MockStartNewGameAsync()
    {
        // 模拟新游戏创建成功
        await UniTask.Delay(0); // 模拟网络延迟

        // 创建一个模拟的 PlayerSession
        var mockPlayerSession = new PlayerSession
        {
            name = _randomPlayerId,
            actor = "mock_actor",
            game = _fixedNewGameName,
        };

        GameManager.Instance.SetSession(mockPlayerSession, new Blueprint());
        Debug.Log($"模拟新游戏创建成功，玩家ID: {GameManager.Instance.Session.UserName}, 角色: {GameManager.Instance.Session.ActorName}, 游戏名: {GameManager.Instance.Session.GameName}");

        // 跳转到下一个场景
        await SceneManager.LoadSceneAsync(MockNextSceneName);
    }
}

