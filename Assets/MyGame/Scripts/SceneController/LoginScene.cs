using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public TMP_Text _textUserName;

    public TMP_Text _textGameName;

    public TMP_Text _textActorName;

    public string _nextScene = "MainScene2";

    public LoginApi _loginApi;

    public StartAction _startAction;

    public GameConfig _gameConfig;

    private string _playerIdentifier;


    void Start()
    {
        Debug.Assert(_textUserName != null, "_textUserName is null");
        Debug.Assert(_textGameName != null, "_textGameName is null");
        Debug.Assert(_textActorName != null, "_textActorName is null");
        Debug.Assert(_loginApi != null, "_loginAction is null");
        Debug.Assert(_startAction != null, "_startAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");

        _playerIdentifier = CreateRandomPlayerIdentifier();
        _textUserName.text = "ID = " + _playerIdentifier;
        _textGameName.text = "测试的游戏 = " + _gameConfig.GameName;
        _textActorName.text = "扮演角色 = " + _gameConfig.ActorName;
    }

    private string CreateRandomPlayerIdentifier()
    {
        System.DateTime now = System.DateTime.Now;
        string timestamp = now.ToString("yyyyMMddHHmmss");
        string randomUserName = "Player" + timestamp + Random.Range(100, 999).ToString();
        return randomUserName;
    }

    public void OnClickLoginThenStartNewGame()
    {
        Debug.Log("OnClickLoginThenStartNewGame");
        StartCoroutine(LoginThenStartNewGame(_playerIdentifier, _gameConfig.GameName, _gameConfig.ActorName));
    }

    private IEnumerator LoginThenStartNewGame(string userName, string gameName, string actorName)
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

        yield return _startAction.Call(GameContext.Instance.StartUrl, userName, gameName, actorName);
        if (_startAction.ReqResult == null || !_startAction.ReqResult.isSuccess)
        {
            Debug.LogError("Start new game failed");
            yield break;
        }

        GameContext.Instance.ActorName = actorName;

        yield return new WaitForSeconds(0.0f);
        //_nextScene = "MainScene2";
        SceneManager.LoadScene(_nextScene);
    }
}
