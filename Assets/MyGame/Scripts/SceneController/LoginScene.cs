using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _userNameText;
    [SerializeField] private TMP_Text _gameNameText;
    [SerializeField] private TMP_Text _actorNameText;

    [Header("Scene Settings")]
    [SerializeField] private string _nextSceneName = "MainScene2";

    [Header("API Components")]
    [SerializeField] private LoginApi _loginApi;
    [SerializeField] private StartApi _startApi;
    [SerializeField] private GameStateSync _gameStateSync;

    [Header("Game Data")]
    [SerializeField] private string _actorName;
    [SerializeField] private string _gameName;

    private string _playerIdentifier;

    void Start()
    {
        Debug.Assert(_userNameText != null, "_userNameText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_loginApi != null, "_loginApi is null");
        Debug.Assert(_startApi != null, "_startApi is null");
        Debug.Assert(_gameStateSync != null, "_gameStateSync is null");
        Debug.Assert(!string.IsNullOrEmpty(_actorName), "_actorName is null");
        Debug.Assert(!string.IsNullOrEmpty(_gameName), "_gameName is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");

        _playerIdentifier = GeneratePlayerId();
        _userNameText.text = "临时ID = " + _playerIdentifier;
        _gameNameText.text = "测试游戏 = " + _gameName;
        _actorNameText.text = "扮演角色 = " + _actorName;
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
        StartCoroutine(StartGameFlow(_playerIdentifier, _gameName, _actorName));
    }

    /// <summary>
    /// 执行登录并开始游戏的完整流程：登录 -> 开始游戏 -> 同步状态 -> 加载场景
    /// </summary>
    private IEnumerator StartGameFlow(string userName, string gameName, string actorName)
    {
        yield return _loginApi.Call(GameContext.Instance.LoginUrl, userName, gameName);
        if (_loginApi.ReqResult == null || !_loginApi.ReqResult.isSuccess)
        {
            Debug.LogError("Login failed");
            yield break;
        }

        // 保存登录信息
        GameContext.Instance.UserName = userName;
        GameContext.Instance.GameName = gameName;
        GameContext.Instance.ActorName = "";

        yield return _startApi.Call(GameContext.Instance.StartUrl, userName, gameName, actorName);
        if (_startApi.ReqResult == null || !_startApi.ReqResult.isSuccess)
        {
            Debug.LogError("Start new game failed");
            yield break;
        }

        GameContext.Instance.ActorName = actorName;

        // 刷新全局游戏状态, 全部刷新！
        yield return _gameStateSync.RefreshMappingAndEntitiesFromServer();

        // 刷新地下城数据！
        yield return _gameStateSync.RefreshDungeonFromServer();

        //这里加一个测试,打印所有的actor entity，确保都能取到贴图
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var entity = actorEntitiesSerialization[i];
            Debug.Log("Actor Entity " + i + ": " + entity.ToString());
            var actorSprite = TextureManager.Instance.GetSprite(entity.name);
            Debug.Assert(actorSprite != null, "Actor sprite is null for entity: " + entity.name);
        }

        // 切换场景
        SceneManager.LoadScene(_nextSceneName);
    }
}
