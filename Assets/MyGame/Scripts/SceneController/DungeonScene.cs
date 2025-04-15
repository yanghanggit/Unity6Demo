using UnityEngine;
using TMPro;
using System.Collections;
//using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Android.Gradle.Manifest;

public class DungeonScene : MonoBehaviour
{
    public string _preScene = "MainScene";

    public TMP_Text _mainText;

    public DungeonRunAction _dungeonRunAction;

    public ViewDungeonAction _viewDungeonAction;

    public ViewActorAction _viewActorAction;

    public TransHomeAction _transHomeAction;

    public XCardPlayer _XCardPlayer;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_dungeonRunAction != null, "_dungeonAction is null");
        Debug.Assert(_viewDungeonAction != null, "_viewDungeonAction is null");
        Debug.Assert(_viewActorAction != null, "_viewActorAction is null");
        Debug.Assert(_transHomeAction != null, "_transHomeAction is null");
        Debug.Assert(_XCardPlayer != null, "_XCardPlayer is null");
        StartCoroutine(ExecuteViewDungeon());
    }


    void Update()
    {

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

    public void OnClickNewRound()
    {
        Debug.Log("OnClickNewRound");
        StartCoroutine(ExecuteDungeonCombatNewRound());
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
        StartCoroutine(ExecuteXCard());
    }

    private IEnumerator ExecuteDungeonCombatKickOff()
    {
        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "dungeon_combat_kick_off", new Dictionary<string, string>()));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("DungeonAction request failed");
            yield break;
        }

        Debug.Log("DungeonAction request success");
        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteDungeonCombatNewRound()
    {
        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "new_round", new Dictionary<string, string>()));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("ExecuteDungeonCombatNewRound request failed");
            yield break;
        }

        Debug.Log("ExecuteDungeonCombatNewRound request success");
        _mainText.text = _dungeonRunAction._message;
    }

    private IEnumerator ExecuteDrawCards()
    {
        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "draw_cards", new Dictionary<string, string>()));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("ExecuteDrawCards request failed");
            yield break;
        }

        yield return StartCoroutine(_viewActorAction.Request(GameContext.Instance.VIEW_ACTOR_URL,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping)));

        if (!_viewActorAction.Success)
        {
            Debug.LogError("ViewActorAction request failed");
            yield break;
        }

        Debug.Log("ExecuteDrawCards request success");
        UpdateActorDisplay(new HashSet<string> { typeof(HandComponent).Name });
    }

    private IEnumerator ExecutePlayCards()
    {
        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "play_cards", new Dictionary<string, string>()));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("ExecutePlayCards request failed");
            yield break;
        }

        Debug.Log("ExecutePlayCards request success");
        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteXCard()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["name"] = _XCardPlayer.Name;
        data["description"] = _XCardPlayer.Description;
        data["effect"] = _XCardPlayer.Effect;


        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "x_card", data));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("ExecuteXCard request failed");
            yield break;
        }

        Debug.Log("ExecuteXCard request success");
    }

    private IEnumerator ExecuteViewDungeon()
    {
        yield return StartCoroutine(_viewDungeonAction.Request(GameContext.Instance.VIEW_DUNGEON_URL, GameContext.Instance.UserName, GameContext.Instance.GameName));
        if (!_viewDungeonAction.Success)
        {
            Debug.LogError("ViewDungeonAction request failed");
            yield break;
        }

        Debug.Log("ExecuteViewDungeon request success!!!!!!");
        UpdateDungeonDisplay();
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

    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        yield return StartCoroutine(_dungeonRunAction.Request(GameContext.Instance.DUNGEON_GAMEPLAY_URL, GameContext.Instance.UserName, GameContext.Instance.GameName, "advance_next_dungeon", new Dictionary<string, string>()));
        if (!_dungeonRunAction.Success)
        {
            Debug.LogError("ExecuteAdvanceNextDungeon request failed");
            yield break;
        }

        Debug.Log("ExecuteAdvanceNextDungeon request success");
        UpdateTextFromAgentLogs();
        _mainText.text = _mainText.text + "\n" + _dungeonRunAction._message;
    }

    private IEnumerator ExecuteBackHome()
    {
        Debug.Log("ExecuteBackHome");
        yield return StartCoroutine(_transHomeAction.Request(GameContext.Instance.DUNGEON_TRANS_HOME_URL, GameContext.Instance.UserName, GameContext.Instance.GameName));
        if (!_transHomeAction.Success)
        {
            Debug.LogError("TransHomeAction request failed");
            yield break;
        }
        Debug.Log("TransHomeAction request success");
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_preScene);
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

    private void UpdateTextFromAgentLogs()
    {
        _mainText.text = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
    }

    private void UpdateDungeonDisplay()
    {
        _mainText.text = MyUtils.DungeonDisplayText(GameContext.Instance.Dungeon, GameContext.Instance.Mapping); ;
    }

}
