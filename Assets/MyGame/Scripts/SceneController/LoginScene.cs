using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public TMP_Text _textUserName;

    public TMP_Text _textGameName;

    public TMP_Text _textActorName;

    public string _nextScene = "MainScene";

    public LoginAction _loginAction;

    public StartAction _startAction;

    public GameConfig _gameConfig;

    private string _playerIdentifier;


    void Start()
    {
        Debug.Assert(_textUserName != null, "_textUserName is null");
        Debug.Assert(_textGameName != null, "_textGameName is null");
        Debug.Assert(_textActorName != null, "_textActorName is null");
        Debug.Assert(_loginAction != null, "_loginAction is null");
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
        yield return _loginAction.Call(userName, gameName);
        if (!_loginAction.LastRequestSuccess)
        {
            Debug.LogError("Login failed");
            yield break;
        }

        yield return _startAction.Call(actorName);
        if (!_startAction.LastRequestSuccess)
        {
            Debug.LogError("Start new game failed");
            yield break;
        }

        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
