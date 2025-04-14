using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public TMP_Text _textUserName;
    public TMP_Text _textGameName;
    public TMP_Text _textActorName;
    public string  _nextScene = "MainScene";
    public LoginAction _loginAction;
    public StartAction _startAction;
    public GameConfig _gameConfig;

    void Start()
    {
        _textUserName.text = _gameConfig.UserName;
        _textGameName.text = _gameConfig.GameName;
        _textActorName.text = _gameConfig.ActorName;

        Debug.Assert(_textUserName != null, "_textUserName is null");
        Debug.Assert(_textGameName != null, "_textGameName is null");
        Debug.Assert(_textActorName != null, "_textActorName is null");
        Debug.Assert(_loginAction != null, "_loginAction is null");
        Debug.Assert(_startAction != null, "_startAction is null");
    }

    void Update()
    {
        
    }

    public void OnClickLogin()
    {
        Debug.Log("OnClickLogin");
        StartCoroutine(ExecuteLoginAndStartGame());
    }

    private IEnumerator ExecuteLoginAndStartGame()
    {
        yield return StartCoroutine(_loginAction.Request(GameContext.Instance.LOGIN_URL,_gameConfig.UserName, _gameConfig.GameName, _gameConfig.ActorName));
        if (!_loginAction.Success)
        {
            Debug.LogError("LoginAction request failed");
            yield break;
        }

        yield return StartCoroutine(_startAction.Request(GameContext.Instance.START_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, GameContext.Instance.ActorName));
        if (!_startAction.Success)
        {
            Debug.LogError("StartAction request failed");
            yield break;
        }

        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);  
    }
}
