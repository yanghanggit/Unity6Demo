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

    // [Header("Sync Components")]
    // [SerializeField] private GameStateSync _gameStateSync;

    [Header("Game Data")]
    //[SerializeField] private string _actorName;
    [SerializeField] private string _gameName;

    private string _playerIdentifier;

    void Start()
    {
        Debug.Assert(_userNameText != null, "_userNameText is null");
        Debug.Assert(_gameNameText != null, "_gameNameText is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        //Debug.Assert(_gameStateSync != null, "_gameStateSync is null");
        //Debug.Assert(!string.IsNullOrEmpty(_actorName), "_actorName is null");
        Debug.Assert(!string.IsNullOrEmpty(_gameName), "_gameName is null");
        Debug.Assert(!string.IsNullOrEmpty(_nextSceneName), "_nextSceneName is null");

        _playerIdentifier = GeneratePlayerId();
        _userNameText.text = "临时ID = " + _playerIdentifier;
        _gameNameText.text = "测试游戏 = " + _gameName;
        //_actorNameText.text = "扮演角色 = " + _actorName;
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

        // 3. 验证所有 Actor 的精灵资源
        ValidateActorSprites();

        // 4. 切换场景
        SceneManager.LoadScene(_nextSceneName);
    }

    /// <summary>
    /// 验证所有 Actor 实体的精灵资源是否可用
    /// </summary>
    private void ValidateActorSprites()
    {
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var entity = actorEntitiesSerialization[i];
            //Debug.Log($"[LoginScene] Actor Entity {i}: {entity.ToString()}");
            var actorSprite = TextureManager.Instance.GetSprite(entity.name);
            Debug.Assert(actorSprite != null, $"Actor sprite is null for entity: {entity.name}");
        }
    }
}
