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

        // 启动的时候，第一次显示内容。
        UpdatePlayerControlText();

        // 先关了
        _viewDungeonController.gameObject.SetActive(false);
        _homePlayerInput.gameObject.SetActive(false);
    }

    private void UpdatePlayerControlText()
    {
        _mainText.text = "玩家控制角色: " + GameContext.Instance.ActorName + "\n";
    }

    public void UpdateTextFromAgentLogs()
    {
        _mainText.text = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
    }

    private void UpdateMappingInfoText()
    {
        _mainText.text = MyUtils.MappingDisplayText(GameContext.Instance.Mapping);
    }

    void Update()
    {

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
        StartCoroutine(ExecuteViewHome());
    }

    public void OnClickViewActor()
    {
        Debug.Log("OnClickViewActor");
        StartCoroutine(ExecuteViewActor());
    }

    public void OnClickOpenHomePlayerInput()
    {
        Debug.Log("OnClickOpenHomePlayerInput");
        _homePlayerInput.OnClickOpen();
    }

    IEnumerator ReturnToLoginScene()
    {

        if (_logoutAction == null)
        {
            Debug.LogError("LogoutAction is null");
            yield break;
        }

        yield return StartCoroutine(_logoutAction.Request(GameContext.Instance.LOGOUT_URL, GameContext.Instance.UserName, GameContext.Instance.GameName));

        if (!_logoutAction.Success)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        SceneManager.LoadScene(_preScene);
    }

    IEnumerator ExecuteHomeGameplayAdvancing()
    {
        yield return StartCoroutine(_homeGamePlayAction.Request(GameContext.Instance.HOME_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "/advancing", new Dictionary<string, string>()));
        if (!_homeGamePlayAction.Success)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }
        //
        UpdateTextFromAgentLogs();
    }

    IEnumerator ExecuteViewDungeon()
    {
        if (_viewDungeonAction == null)
        {
            Debug.LogError("ViewDungeonAction is null");
            yield break;
        }
        yield return StartCoroutine(_viewDungeonAction.Request(GameContext.Instance.VIEW_DUNGEON_URL, GameContext.Instance.UserName, GameContext.Instance.GameName));
        if (!_viewDungeonAction.Success)
        {
            Debug.LogError("ViewDungeonAction request failed");
            yield break;
        }

        Debug.Log("ExecuteViewDungeon request success!!!!!!");
        _viewDungeonController.OnClickOpen();
    }

    IEnumerator ExecuteViewHome()
    {
        if (_viewHomeAction == null)
        {
            Debug.LogError("ViewHomeAction is null");
            yield break;
        }
        yield return StartCoroutine(_viewHomeAction.Request(GameContext.Instance.VIEW_HOME_URL, GameContext.Instance.UserName, GameContext.Instance.GameName));
        if (!_viewHomeAction.Success)
        {
            Debug.LogError("ViewHomeAction request failed");
            yield break;
        }

        Debug.Log("ExecuteViewHome request success!!!!!!");
        UpdateMappingInfoText();
    }

    private IEnumerator ExecuteViewActor()
    {
        yield return StartCoroutine(_viewActorAction.Request(GameContext.Instance.VIEW_ACTOR_URL,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping)));

        if (!_viewActorAction.Success)
        {
            Debug.LogError("ViewActorAction request failed");
            yield break;
        }

        Debug.Log("ExecuteViewActor request success!!!!!!");
        UpdateActorDisplay(new HashSet<string> { typeof(RPGCharacterProfileComponent).Name });
    }

    private void UpdateActorDisplay(HashSet<string> includedComponentNames = null)
    {
        var text = "";

        var actorSnapshots = GameContext.Instance.ActorSnapshots;
        for (int i = 0; i < actorSnapshots.Count; i++)
        {
            var actorSnapshot = actorSnapshots[i];
            text += MyUtils.ActorDisplayText(actorSnapshot, includedComponentNames);
            text += "\n";
        }
        _mainText.text = text;
    }
}
