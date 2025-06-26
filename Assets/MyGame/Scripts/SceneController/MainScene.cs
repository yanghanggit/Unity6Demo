using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Collections.Generic;


public class MainScene : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public string _nextScene = "DungeonScene";

    public TMP_Text _mainText;

    public LogoutAction _logoutAction;

    public HomeGamePlayAction _homeGamePlayAction;

    public ViewHomeAction _viewHomeAction;

    public ViewDungeonAction _viewDungeonAction;

    public ViewDungeon _viewDungeonController;

    public ViewActorAction _viewActorAction;

    public HomePlayerInput _homePlayerInput;


    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_logoutAction != null, "_logoutAction is null");
        Debug.Assert(_homeGamePlayAction != null, "_homeAction is null");
        Debug.Assert(_viewHomeAction != null, "_viewHomeAction is null");
        Debug.Assert(_viewDungeonAction != null, "_viewDungeonAction is null");
        Debug.Assert(_viewDungeonController != null, "_viewDungeonController is null");
        Debug.Assert(_viewActorAction != null, "_viewActorAction is null");
        Debug.Assert(_homePlayerInput != null, "_homePlayerInput is null");


        _viewDungeonController.gameObject.SetActive(false);
        _homePlayerInput.gameObject.SetActive(false);


        StartCoroutine(ExecuteViewHomeAndActors());
    }

    public void UpdateTextFromAgentLogs()
    {
        _mainText.text = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
    }

    public void OnClickBack()
    {
        Debug.Log("OnGoBack");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickHomeRun()
    {
        Debug.Log("OnClickHomeRun");
        _mainText.text = "服务器正在运行，请等待！";
        StartCoroutine(ExecuteHomeGameplayAdvancing());
    }

    public void OnClickViewDungeon()
    {
        Debug.Log("OnClickViewDungeon");
        StartCoroutine(ExecuteViewDungeon());
    }

    public void OnClickViewHome()
    {
        Debug.Log("OnClickViewHome");
        StartCoroutine(ExecuteViewHomeAndActors());
    }

    public void OnClickOpenHomePlayerInput()
    {
        Debug.Log("OnClickOpenHomePlayerInput");
        _homePlayerInput.OnClickOpen();
    }

    IEnumerator ReturnToLoginScene()
    {
        yield return _logoutAction.Call();

        if (!_logoutAction.LastRequestSuccess)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        SceneManager.LoadScene(_preScene);
    }

    IEnumerator ExecuteHomeGameplayAdvancing()
    {
        yield return _homeGamePlayAction.Call("/advancing");
        if (!_homeGamePlayAction.LastRequestSuccess)
        {
            yield break;
        }
        //
        UpdateTextFromAgentLogs();
    }

    IEnumerator ExecuteViewDungeon()
    {
        yield return _viewDungeonAction.Call();
        if (!_viewDungeonAction.LastRequestSuccess)
        {
            yield break;
        }
        _viewDungeonController.gameObject.SetActive(true);
        _viewDungeonController.UpdateDungeonDisplay();
    }

    private IEnumerator ExecuteViewHomeAndActors()
    {
        yield return _viewHomeAction.Call();
        if (!_viewHomeAction.LastRequestSuccess)
        {
            yield break;
        }

        yield return _viewActorAction.Call(
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (!_viewActorAction.LastRequestSuccess)
        {
            yield break;
        }

        var text0 = "你 = " + GameContext.Instance.ActorName;
        var text1 = MyUtils.MappingDisplayText(GameContext.Instance.Mapping);
        var text2 = ComposeActorInfoString(new HashSet<string> { typeof(RPGCharacterProfileComponent).Name });
        _mainText.text = text0 + "\n" + text1 + "\n" + text2;
    }

    private string ComposeActorInfoString(HashSet<string> includedComponentNames = null)
    {
        var text = "";

        var actorSnapshots = GameContext.Instance.ActorSnapshots;
        for (int i = 0; i < actorSnapshots.Count; i++)
        {
            var actorSnapshot = actorSnapshots[i];
            text += MyUtils.ActorDisplayText(actorSnapshot, includedComponentNames);
            text += "\n";
        }
        return text;
    }
}
