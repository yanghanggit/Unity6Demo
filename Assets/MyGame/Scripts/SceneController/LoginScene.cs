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

        _textUserName.text = "登录后显示用户名";
        _textGameName.text = "登录后显示游戏名";
        _textActorName.text = "启动游戏后显示角色名";
        _textDefaultXCardName.text = "启动游戏后显示默认X-Card名";
    }

    public void OnClickLogin()
    {
        Debug.Log("OnClickLogin");
        StartCoroutine(ExecuteLogin());
    }

    public void OnClickNewGame()
    {
        Debug.Log("OnClickNewGame");
        StartCoroutine(ExecuteNewGame(_gameConfig.ActorName));
    }

    public void OnClickContinueGame()
    {
        Debug.Log("OnClickContinueGame");
    }

    private IEnumerator ExecuteLogin()
    {
        yield return StartCoroutine(_loginAction.Call(_gameConfig.UserName, _gameConfig.GameName));
        if (!_loginAction.RequestSuccess)
        {
            yield break;
        }

        _textUserName.text = GameContext.Instance.UserName;
        _textGameName.text = GameContext.Instance.GameName;
    }

    private IEnumerator ExecuteNewGame(string actorName)
    {
        yield return StartCoroutine(_startAction.Call(actorName));
        if (!_startAction.RequestSuccess)
        {
            yield break;
        }

        //
        Debug.Assert(GameContext.Instance.ActorName == actorName, "GameContext.Instance.ActorName != actorName");
        Debug.Assert(GameContext.Instance.UserName == _gameConfig.UserName, "GameContext.Instance.UserName != _gameConfig.UserName");
        Debug.Assert(GameContext.Instance.GameName == _gameConfig.GameName, "GameContext.Instance.GameName != _gameConfig.GameName");

        //
        _textActorName.text = GameContext.Instance.ActorName;
        _textDefaultXCardName.text = $"{_XCardPlayer.Name}\n{_XCardPlayer.Description}\n{_XCardPlayer.Effect}";

        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
