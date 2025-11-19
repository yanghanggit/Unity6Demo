using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
//using System;

public class DungeonScene : MonoBehaviour
{
    public string _preScene = "MainScene2";

    public TMP_Text _mainText;

    public DungeonGamePlayAction _dungeonGamePlayAction;

    public DungeonStateAction _viewDungeonAction;

    public ActorDetailsAction _viewActorAction;

    public TransHomeAction _transHomeAction;

    public XCardPlayer _XCardPlayer;

    public XCardEditor _XCardEditor;

    public SessionMessagesAction _sessionMessagesAction;


    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_dungeonGamePlayAction != null, "_dungeonAction is null");
        Debug.Assert(_viewDungeonAction != null, "_viewDungeonAction is null");
        Debug.Assert(_viewActorAction != null, "_viewActorAction is null");
        Debug.Assert(_transHomeAction != null, "_transHomeAction is null");
        Debug.Assert(_XCardPlayer != null, "_XCardPlayer is null");
        Debug.Assert(_XCardEditor != null, "_XCardEditor is null");
        Debug.Assert(_sessionMessagesAction != null, "_sessionMessagesAction is null");

        _XCardEditor.gameObject.SetActive(false);

        StartCoroutine(ExecuteViewDungeon());
    }

    public void OnClickDungeonCombatKickOff()
    {
        Debug.Log("OnClickDungeonCombatKickOff");
        StartCoroutine(ExecuteDungeonCombatKickOff());
    }

    public void OnClickViewDungeon()
    {
        Debug.Log("OnClickViewDungeon");
        StartCoroutine(ExecuteViewDungeon());
    }

    public void OnClickViewActor()
    {
        Debug.Log("OnClickViewActor");
        StartCoroutine(ExecuteViewActor());
    }

    public void OnClickDrawCards()
    {
        Debug.Log("OnClickDrawCards");
        StartCoroutine(ExecuteDrawCards());
    }

    public void OnClickPlayCards()
    {
        Debug.Log("OnClickPlayCards");
        StartCoroutine(ExecutePlayCards());
    }

    public void OnClickAdvanceNextDungeon()
    {
        Debug.Log("OnClickAdvanceNextDungeon");
        StartCoroutine(ExecuteAdvanceNextDungeon());
    }

    public void OnClickBackHome()
    {
        Debug.Log("OnClickBackHome");
        StartCoroutine(ExecuteBackHome());
    }


    public void OnClickXCard()
    {
        Debug.Log("OnClickXCard");
        _XCardEditor.gameObject.SetActive(true);
    }

    // public void OnClickCombatComplete()
    // {
    //     Debug.Log("OnClickCombatComplete");
    //     StartCoroutine(ExecuteCombatComplete());
    // }

    private IEnumerator ExecuteDungeonCombatKickOff()
    {
        yield return _dungeonGamePlayAction.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "dungeon_combat_kick_off");
        if (_dungeonGamePlayAction.ResponseData == null)
        {
            yield break;
        }

        //GameContext.Instance.ProcessClientMessages(_dungeonGamePlayAction.ResponseData.client_messages);
        yield return FetchAndProcessSessionMessages();
        yield return ExecuteViewDungeon();
    }

    private IEnumerator ExecuteDrawCards()
    {
        yield return _dungeonGamePlayAction.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "draw_cards");
        if (_dungeonGamePlayAction.ResponseData == null)
        {
            Debug.LogError("ExecuteDrawCards request failed");
            yield break;
        }

        //GameContext.Instance.ProcessClientMessages(_dungeonGamePlayAction.ResponseData.client_messages);
        yield return FetchAndProcessSessionMessages();

        yield return _viewActorAction.Call(
            GameContext.Instance.ActorDetailsUrl,
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (_viewActorAction.ResponseData == null)
        {
            Debug.LogError("ViewActorAction request failed");
            yield break;
        }

        GameContext.Instance.ActorEntitiesSerialization = _viewActorAction.ResponseData.actor_entities_serialization;

        Debug.Log("ExecuteDrawCards request success");
        UpdateActorDisplay(new HashSet<string> { typeof(HandComponent).Name });
    }

    private IEnumerator ExecutePlayCards()
    {
        yield return _dungeonGamePlayAction.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "play_cards");

        if (_dungeonGamePlayAction.ResponseData == null)
        {
            Debug.LogError("ExecutePlayCards request failed");
            yield break;
        }

        //GameContext.Instance.ProcessClientMessages(_dungeonGamePlayAction.ResponseData.client_messages);
        yield return FetchAndProcessSessionMessages();
        Debug.Log("ExecutePlayCards request success");
        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteViewDungeon()
    {
        yield return _viewDungeonAction.Call(GameContext.Instance.DungeonStateUrl);
        if (_viewDungeonAction.ResponseData == null)
        {
            yield break;
        }

        GameContext.Instance.Mapping = _viewDungeonAction.ResponseData.mapping;
        GameContext.Instance.Dungeon = _viewDungeonAction.ResponseData.dungeon;

        yield return _viewActorAction.Call(
            GameContext.Instance.ActorDetailsUrl,
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (_viewActorAction.ResponseData == null)
        {
            yield break;
        }

        GameContext.Instance.ActorEntitiesSerialization = _viewActorAction.ResponseData.actor_entities_serialization;

        UpdateDungeonDisplay();
    }

    private IEnumerator ExecuteViewActor()
    {
        yield return _viewActorAction.Call(
            GameContext.Instance.ActorDetailsUrl,
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (_viewActorAction.ResponseData == null)
        {
            yield break;
        }

        GameContext.Instance.ActorEntitiesSerialization = _viewActorAction.ResponseData.actor_entities_serialization;

        UpdateActorDisplay(new HashSet<string> { typeof(CombatStatsComponent).Name });
    }

    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        yield return _dungeonGamePlayAction.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "advance_next_dungeon");
        if (_dungeonGamePlayAction.ResponseData == null)
        {
            _mainText.text = _dungeonGamePlayAction.ReqResult.responseText;
            yield break;
        }

        _mainText.text = "已进入下一个地下城";
        yield return ExecuteViewDungeon();
    }

    private IEnumerator ExecuteBackHome()
    {
        Debug.Log("ExecuteBackHome");
        yield return _transHomeAction.Call(GameContext.Instance.DungeonTransHomeUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        //if (!_transHomeAction.LastRequestSuccess)
        if (_transHomeAction.ResponseData == null)
        {
            Debug.LogError("TransHomeAction request failed");
            yield break;
        }
        Debug.Log("TransHomeAction request success");
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_preScene);
    }

    // private IEnumerator ExecuteCombatComplete()
    // {
    //     yield return _dungeonGamePlayAction.Call("dungeon_combat_complete");
    //     if (!_dungeonGamePlayAction.LastRequestSuccess)
    //     {
    //         yield break;
    //     }

    //     UpdateTextFromAgentLogs();
    // }

    private void UpdateActorDisplay(HashSet<string> includedComponentNames = null)
    {
        var text = "";

        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var actorEntitySerialization = actorEntitiesSerialization[i];
            text += MyUtils.ActorDisplayText(actorEntitySerialization, includedComponentNames);
            text += "\n";
        }
        _mainText.text = text;
    }

    private void UpdateTextFromAgentLogs()
    {
        _mainText.text = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
    }

    private void UpdateDungeonDisplay()
    {
        _mainText.text = MyUtils.MappingDisplayText(CurrentMapping()) + "\n" + MyUtils.DungeonCombatDisplayText(GameContext.Instance.Dungeon);
    }

    private Dictionary<string, List<string>> CurrentMapping()
    {
        Dictionary<string, List<string>> currentMapping = new Dictionary<string, List<string>>();

        var currentActorStage = MyUtils.GetActorLocation(GameContext.Instance.ActorName, GameContext.Instance.Mapping);
        currentMapping[currentActorStage] = GameContext.Instance.Mapping[currentActorStage];

        return currentMapping;
    }

    private IEnumerator FetchAndProcessSessionMessages()
    {
        // 获取会话消息
        _sessionMessagesAction.Setup(
            GameContext.Instance.SessionMessagesUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            GameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        //
        GameContext.Instance.ProcessClientMessages(_sessionMessagesAction.ResponseData.session_messages);
    }

    private void UpdateLastSequenceIdFromResponse()
    {
        if (_sessionMessagesAction.ResponseData == null)
        {
            Debug.LogWarning("SessionMessagesAction ResponseData is null");
            Debug.Assert(false, "SessionMessagesAction ResponseData is null");
            return;
        }

        if (_sessionMessagesAction.ResponseLastSequenceId < 0)
        {
            Debug.LogWarning("Invalid last sequence ID");
            return;
        }

        // 设置 LastSequenceId
        GameContext.Instance.LastSequenceId = _sessionMessagesAction.ResponseLastSequenceId;
    }
}
