using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public static readonly string NextSceneName = "MainScene";

    public static readonly string GameName = "Game1";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _userNameText;
    [SerializeField] private TMP_Text _gameNameText;


    /// 加一个Header 叫测试测试，内加一个变量叫固定玩家ID，用于测试用途
    [Header("测试测试")]
    [SerializeField] private string _fixedPlayerId;

    // 内部使用的玩家标识符
    private string _playerIdentifier;

    void Start()
    {
        Debug.Assert(_userNameText != null, "_userNameText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
        Debug.Assert(!string.IsNullOrEmpty(GameName), "_gameName is null");
        //Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");

        // 如果固定就使用固定的玩家ID，否则生成一个临时ID
        if (!string.IsNullOrEmpty(_fixedPlayerId))
        {
            _playerIdentifier = _fixedPlayerId;
        }
        else
        {
            _playerIdentifier = GeneratePlayerId();
        }

        //_playerIdentifier = GeneratePlayerId();
        _userNameText.text = "临时ID = " + _playerIdentifier;
        _gameNameText.text = "测试游戏 = " + GameName;
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
        StartGameFlow(_playerIdentifier, GameName).Forget();
    }

    /// <summary>
    /// 执行登录并开始游戏的完整流程：登录 -> 开始游戎 -> 同步状态 -> 加载场景
    /// </summary>
    private async UniTaskVoid StartGameFlow(string userName, string gameName)
    {
        // 1. 使用 SessionManager 执行登录和开始游戏
        bool apiSuccess = await SessionManager.Instance.LoginAndStart(userName, gameName);
        if (!apiSuccess)
        {
            Debug.LogError("[LoginScene] LoginAndStart failed");
            return;
        }

        // 2. 刷新全局游戏状态
        // var syncErr = await GameStateSync.Instance.RefreshMappingAndEntitiesFromServer();
        // if (syncErr != GameSyncError.None)
        // {
        //     Debug.LogError($"[LoginScene] RefreshMappingAndEntitiesFromServer failed: {syncErr}");
        //     return;
        // }

        await UniTask.Yield();

        // 3. 切换场景
        SceneManager.LoadScene(NextSceneName);
    }
}
