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

    public DungeonGamePlayApi _dungeonGamePlayApi;

    public DungeonStateApi _dungeonStateApi;

    public ActorDetailsApi _actorDetailApi;

    public TransHomeApi _transHomeApi;

    public XCardPlayer _XCardPlayer;

    public XCardEditor _XCardEditor;

    public SessionMessagesApi _sessionMessagesApi;


    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_dungeonGamePlayApi != null, "_dungeonAction is null");
        Debug.Assert(_dungeonStateApi != null, "_viewDungeonAction is null");
        Debug.Assert(_actorDetailApi != null, "_actorDetailApi is null");
        Debug.Assert(_transHomeApi != null, "_transHomeApi is null");
        Debug.Assert(_XCardPlayer != null, "_XCardPlayer is null");
        Debug.Assert(_XCardEditor != null, "_XCardEditor is null");
        Debug.Assert(_sessionMessagesApi != null, "_sessionMessagesAction is null");

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
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "dungeon_combat_kick_off");
        if (_dungeonGamePlayApi.RespData == null)
        {
            yield break;
        }

        //GameContext.Instance.ProcessClientMessages(_dungeonGamePlayAction.ResponseData.client_messages);
        yield return FetchAndProcessSessionMessages();
        yield return ExecuteViewDungeon();
    }

    private IEnumerator ExecuteDrawCards()
    {
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "draw_cards");
        if (_dungeonGamePlayApi.RespData == null)
        {
            Debug.LogError("ExecuteDrawCards request failed");
            yield break;
        }

        //GameContext.Instance.ProcessClientMessages(_dungeonGamePlayAction.ResponseData.client_messages);
        // yield return FetchAndProcessSessionMessages();

        // yield return _actorDetailApi.Call(
        //     GameContext.Instance.ActorDetailsUrl,
        //     MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        // if (_actorDetailApi.RespData == null)
        // {
        //     Debug.LogError("ViewActorAction request failed");
        //     yield break;
        // }

        // GameContext.Instance.ActorEntitiesSerialization = _actorDetailApi.RespData.actor_entities_serialization;

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        Debug.Log("ExecuteDrawCards request success");
        UpdateActorDisplay(new HashSet<string> { typeof(HandComponent).Name });
    }

    private IEnumerator ExecutePlayCards()
    {
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "play_cards");

        if (_dungeonGamePlayApi.RespData == null)
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
        // yield return _dungeonStateApi.Call(GameContext.Instance.DungeonStateUrl);
        // if (_dungeonStateApi.RespData == null)
        // {
        //     yield break;
        // }

        // GameContext.Instance.Mapping = _dungeonStateApi.RespData.mapping;
        // GameContext.Instance.Dungeon = _dungeonStateApi.RespData.dungeon;

        // yield return _actorDetailApi.Call(
        //     GameContext.Instance.ActorDetailsUrl,
        //     MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        // if (_actorDetailApi.RespData == null)
        // {
        //     yield break;
        // }

        // GameContext.Instance.ActorEntitiesSerialization = _actorDetailApi.RespData.actor_entities_serialization;

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();

        UpdateDungeonDisplay();
    }

    private IEnumerator ExecuteViewActor()
    {
        // yield return _actorDetailApi.Call(
        //     GameContext.Instance.ActorDetailsUrl,
        //     MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        // if (_actorDetailApi.RespData == null)
        // {
        //     yield break;
        // }

        // GameContext.Instance.ActorEntitiesSerialization = _actorDetailApi.RespData.actor_entities_serialization;

        yield return GameStateSync.Instance.RefreshDungeonAndActorsFromServer();
        UpdateActorDisplay(new HashSet<string> { typeof(CombatStatsComponent).Name });
    }

    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        yield return _dungeonGamePlayApi.Call(
            GameContext.Instance.DungeonGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "advance_next_dungeon");
        if (_dungeonGamePlayApi.RespData == null)
        {
            _mainText.text = _dungeonGamePlayApi.ReqResult.responseText;
            yield break;
        }

        _mainText.text = "已进入下一个地下城";
        yield return ExecuteViewDungeon();
    }

    private IEnumerator ExecuteBackHome()
    {
        Debug.Log("ExecuteBackHome");
        yield return _transHomeApi.Call(GameContext.Instance.DungeonTransHomeUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        //if (!_transHomeAction.LastRequestSuccess)
        if (_transHomeApi.RespData == null)
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
        var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");
        var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
        var currentMapping = new Dictionary<string, List<string>> { { stageName, actorsInStage } };


        _mainText.text = MyUtils.MappingDisplayText(currentMapping) + "\n" + MyUtils.DungeonCombatDisplayText(GameContext.Instance.Dungeon);
    }

    // private Dictionary<string, List<string>> CurrentMapping()
    // {

    //     var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
    //     Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");

    //     var actorsInStage = GameContext.Instance.GetActorsInStage(stageName);
    //     // if (actorsInStage.Count == 0)
    //     // {
    //     //     Debug.LogError("[GameStateSync] No actors found in the current actor's stage");
    //     //     yield break;
    //     // }

    //     return new Dictionary<string, List<string>> { { stageName, actorsInStage } };

    //     // Dictionary<string, List<string>> currentMapping = new Dictionary<string, List<string>>();

    //     // var currentActorStage = MyUtils.GetActorLocation(GameContext.Instance.ActorName, GameContext.Instance.Mapping);
    //     // currentMapping[currentActorStage] = GameContext.Instance.Mapping[currentActorStage];

    //     // return currentMapping;
    // }

    private IEnumerator FetchAndProcessSessionMessages()
    {
        // 获取会话消息
        // _sessionMessagesApi.Initialize(
        //     GameContext.Instance.SessionMessagesUrl,
        //     GameContext.Instance.UserName,
        //     GameContext.Instance.GameName,
        //     GameContext.Instance.LastSequenceId
        // );

        yield return _sessionMessagesApi.Call(GameContext.Instance.SessionMessagesUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            GameContext.Instance.LastSequenceId);
        if (_sessionMessagesApi.RespData == null)
        {
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        //
        GameContext.Instance.ProcessClientMessages(_sessionMessagesApi.RespData.session_messages);
    }

    private void UpdateLastSequenceIdFromResponse()
    {
        if (_sessionMessagesApi.RespData == null)
        {
            Debug.LogWarning("SessionMessagesAction ResponseData is null");
            Debug.Assert(false, "SessionMessagesAction ResponseData is null");
            return;
        }

        if (_sessionMessagesApi.RespLastSequenceId < 0)
        {
            Debug.LogWarning("Invalid last sequence ID");
            return;
        }

        // 设置 LastSequenceId
        GameContext.Instance.LastSequenceId = _sessionMessagesApi.RespLastSequenceId;
    }
}
