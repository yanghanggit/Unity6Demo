using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _userNameText;
    [SerializeField] private TMP_Text _gameNameText;

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "MainScene";
    [SerializeField] private string _gameName = "Game1";

    /// 加一个Header 叫测试测试，内加一个变量叫固定玩家ID，用于测试用途
    [Header("测试测试")]
    [SerializeField] private string _fixedPlayerId;

    // 内部使用的玩家标识符
    private string _playerIdentifier;

    void Start()
    {
        Debug.Assert(_userNameText != null, "_userNameText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
        Debug.Assert(!string.IsNullOrEmpty(_gameName), "_gameName is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");

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

        // 3. 切换场景
        SceneManager.LoadScene(_nextSceneName);
    }
}
