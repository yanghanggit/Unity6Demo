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


    void Start()
    {
        Debug.Assert(_textUserName != null, "_textUserName is null");
        Debug.Assert(_textGameName != null, "_textGameName is null");
        Debug.Assert(_textActorName != null, "_textActorName is null");
        Debug.Assert(_loginAction != null, "_loginAction is null");
        Debug.Assert(_startAction != null, "_startAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");


        _textUserName.text = CreateRandomPlayerIdentifier();
        _textGameName.text = _gameConfig.GameName;
        _textActorName.text = _gameConfig.ActorName;
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
        StartCoroutine(LoginThenStartNewGame(_textUserName.text, _textGameName.text, _textActorName.text));
    }

    private IEnumerator LoginThenStartNewGame(string userName, string gameName, string actorName)
    {
        yield return StartCoroutine(_loginAction.Call(userName, gameName));
        if (!_loginAction.RequestSuccess)
        {
            Debug.LogError("Login failed");
            yield break;
        }

        yield return StartCoroutine(_startAction.Call(actorName));
        if (!_startAction.RequestSuccess)
        {
            Debug.LogError("Start new game failed");
            yield break;
        }

        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
