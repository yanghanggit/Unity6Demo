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
    public TMP_InputField _UserNameInputField;

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
        Debug.Assert(_UserNameInputField != null, "_UserNameInputField is null");

        _textUserName.text = "登录后显示用户名";
        _textGameName.text = "登录后显示游戏名";
        _textActorName.text = "启动游戏后显示角色名";
        _textDefaultXCardName.text = "启动游戏后显示默认X-Card名";


        //结合当前的时间，生成一个随机的用户名
        System.DateTime now = System.DateTime.Now;
        string timestamp = now.ToString("yyyyMMddHHmmss");
        string randomUserName = "Player" + timestamp + Random.Range(100, 999).ToString();
        _UserNameInputField.text = randomUserName;
    }

    public void OnClickLogin()
    {
        Debug.Log("OnClickLogin");
        if (_UserNameInputField.text == "")
        {
            Debug.LogError("Input field is empty");
            return;
        }
    
        StartCoroutine(ExecuteLogin(_UserNameInputField.text, _gameConfig.GameName));
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

    private IEnumerator ExecuteLogin(string userName, string gameName)
    {
        yield return StartCoroutine(_loginAction.Call(userName, gameName));
        if (!_loginAction.RequestSuccess)
        {
            yield break;
        }

        _textUserName.text = userName;
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
        _textActorName.text = GameContext.Instance.ActorName;
        _textDefaultXCardName.text = $"{_XCardPlayer.Name}\n{_XCardPlayer.Description}\n{_XCardPlayer.Effect}";

        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
