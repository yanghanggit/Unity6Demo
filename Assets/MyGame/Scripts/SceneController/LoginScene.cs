using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginScene : MonoBehaviour
{
    public TMP_Text _textUserName;
    public TMP_Text _textGameName;
    public TMP_Text _textActorName;
    public TMP_Text _textDefaultXCardName;
    public string _nextScene = "MainScene";
    public LoginAction _loginAction;
    public StartAction _startAction;
    public GameConfig _gameConfig;
    public XCardPlayer _XCardPlayer;

    void Start()
    {
        Debug.Assert(_textUserName != null, "_textUserName is null");
        Debug.Assert(_textGameName != null, "_textGameName is null");
        Debug.Assert(_textActorName != null, "_textActorName is null");
        Debug.Assert(_loginAction != null, "_loginAction is null");
        Debug.Assert(_startAction != null, "_startAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");
        Debug.Assert(_XCardPlayer != null, "_XCardPlayer is null");
        Debug.Assert(_textDefaultXCardName != null, "_XCardPlayer._defaultXCard is null");

        _textUserName.text = _gameConfig.UserName;
        _textGameName.text = _gameConfig.GameName;
        _textActorName.text = _gameConfig.ActorName;
        _textDefaultXCardName.text = $"{_XCardPlayer.Name}\n{_XCardPlayer.Description}\n{_XCardPlayer.Effect}";

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
        yield return StartCoroutine(_loginAction.Request(GameContext.Instance.LOGIN_URL, _gameConfig.UserName, _gameConfig.GameName, _gameConfig.ActorName));
        if (!_loginAction.RequestSuccess)
        {
            Debug.LogError("LoginAction request failed");
            yield break;
        }

        yield return StartCoroutine(_startAction.Request(GameContext.Instance.START_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, GameContext.Instance.ActorName));
        if (!_startAction.RequestSuccess)
        {
            Debug.LogError("StartAction request failed");
            yield break;
        }

        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }
}
