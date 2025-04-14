using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HomePlayerInput : MonoBehaviour
{
    public TMP_Text _mainText;

    public TMP_InputField _inputField;

    public HomeRunAction _homeRunAction;

    public GameConfig _gameConfig;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_inputField != null, "_inputField is null");
        Debug.Assert(_homeRunAction != null, "_homeRunAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");
    }

    void Update()
    {

    }

    public void OnClickOpen()
    {
        Debug.Log("OnClickOpen");
        this.gameObject.SetActive(true);
        _inputField.text = "";
        _mainText.text = "";

    }

    public void OnClickClose()
    {
        Debug.Log("OnClickClose");
        this.gameObject.SetActive(false);
    }

    public void OnClickSend()
    {
        Debug.Log("OnClickSend");
        /////speak --target=角色.法师.奥露娜 --content=我还是需要准备一下
        if (_inputField.text == "")
        {
            Debug.LogError("Input field is empty");
            return;
        }
        StartCoroutine(ExecuteHomeGameplaySpeakAction(_gameConfig.AllyName, _inputField.text));
    }

    public void OnValueChanged()
    {
        Debug.Log("OnValueChanged: " + _inputField.text);
        _mainText.text = $"你({GameContext.Instance.ActorName})/speak = @{_gameConfig.AllyName} " + _inputField.text;
    }

    public void OnEditEnd()
    {
        Debug.Log("OnEditEnd: " + _inputField.text);
    }

    public void OnSelect()
    {
        Debug.Log("OnSelect: " + _inputField.text);
    }

    public void OnDeselect()
    {
        Debug.Log("OnDeselect: " + _inputField.text);
    }

    public void OnClickHomeGameplayAdvancing()
    {
        Debug.Log("OnClickHomeGameplayAdvancing");
        _mainText.text = "服务器正在运行，请等待！";
        GameContext.Instance.AgentEventLogs.Clear();
        StartCoroutine(ExecuteHomeGameplayAdvancing());
    }

    private IEnumerator ExecuteHomeGameplaySpeakAction(string target, string content)
    {

        var player_input_data = new Dictionary<string, string>();
        player_input_data["target"] = target;
        player_input_data["content"] = content;

        yield return StartCoroutine(_homeRunAction.Request(GameContext.Instance.HOME_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "/speak", player_input_data));
        if (!_homeRunAction.Success)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }

        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteHomeGameplayAdvancing()
    {
        yield return StartCoroutine(_homeRunAction.Request(GameContext.Instance.HOME_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "/advancing", new Dictionary<string, string>()));
        if (!_homeRunAction.Success)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }
        //
        UpdateTextFromAgentLogs();
    }

    private void UpdateTextFromAgentLogs()
    {
        _mainText.text = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
    }
}
