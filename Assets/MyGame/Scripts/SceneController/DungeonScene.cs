using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DungeonScene : MonoBehaviour
{
    public string _preScene = "MainScene";

    public TMP_Text _mainText;

    public DungeonGamePlayAction _dungeonGamePlayAction;

    public ViewDungeonAction _viewDungeonAction;

    public ViewActorAction _viewActorAction;

    public TransHomeAction _transHomeAction;

    public XCardPlayer _XCardPlayer;

    public XCardEditor _XCardEditor;

    public GameObject _imageGoblin;

    public GameObject _imageOrc;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_dungeonGamePlayAction != null, "_dungeonAction is null");
        Debug.Assert(_viewDungeonAction != null, "_viewDungeonAction is null");
        Debug.Assert(_viewActorAction != null, "_viewActorAction is null");
        Debug.Assert(_transHomeAction != null, "_transHomeAction is null");
        Debug.Assert(_XCardPlayer != null, "_XCardPlayer is null");
        Debug.Assert(_XCardEditor != null, "_XCardEditor is null");
        Debug.Assert(_imageGoblin != null, "_imageGoblin is null");
        Debug.Assert(_imageOrc != null, "_imageOrc is null");

        _XCardEditor.gameObject.SetActive(false);
        _imageGoblin.SetActive(false);
        _imageOrc.SetActive(false);
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

    public void OnClickCombatComplete()
    {
        Debug.Log("OnClickCombatComplete");
        StartCoroutine(ExecuteCombatComplete());
    }

    private IEnumerator ExecuteDungeonCombatKickOff()
    {
        yield return _dungeonGamePlayAction.Call("dungeon_combat_kick_off");
        if (!_dungeonGamePlayAction.LastRequestSuccess)
        {
            yield break;
        }

        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteDrawCards()
    {
        yield return _dungeonGamePlayAction.Call("draw_cards");
        if (!_dungeonGamePlayAction.LastRequestSuccess)
        {
            Debug.LogError("ExecuteDrawCards request failed");
            yield break;
        }

        yield return _viewActorAction.Call(
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (!_viewActorAction.LastRequestSuccess)
        {
            Debug.LogError("ViewActorAction request failed");
            yield break;
        }

        Debug.Log("ExecuteDrawCards request success");
        UpdateActorDisplay(new HashSet<string> { typeof(HandComponent).Name });
    }

    private IEnumerator ExecutePlayCards()
    {
        yield return _dungeonGamePlayAction.Call("play_cards");
        if (!_dungeonGamePlayAction.LastRequestSuccess)
        {
            Debug.LogError("ExecutePlayCards request failed");
            yield break;
        }

        Debug.Log("ExecutePlayCards request success");
        UpdateTextFromAgentLogs();
    }

    private IEnumerator ExecuteViewDungeon()
    {
        yield return _viewDungeonAction.Call();
        if (!_viewDungeonAction.LastRequestSuccess)
        {
            yield break;
        }

        yield return _viewActorAction.Call(
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (!_viewActorAction.LastRequestSuccess)
        {
            yield break;
        }

        UpdateDungeonDisplay();
    }

    private IEnumerator ExecuteViewActor()
    {
        yield return _viewActorAction.Call(
            MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping));

        if (!_viewActorAction.LastRequestSuccess)
        {
            yield break;
        }

        UpdateActorDisplay(new HashSet<string> { typeof(RPGCharacterProfileComponent).Name });
    }

    private IEnumerator ExecuteAdvanceNextDungeon()
    {
        yield return _dungeonGamePlayAction.Call("advance_next_dungeon");
        if (!_dungeonGamePlayAction.LastRequestSuccess)
        {
            yield break;
        }

        UpdateTextFromAgentLogs();
        _mainText.text = _mainText.text + "\n" + _dungeonGamePlayAction.ResponseMessage;

        yield return ExecuteViewDungeon();
    }

    private IEnumerator ExecuteBackHome()
    {
        Debug.Log("ExecuteBackHome");
        yield return _transHomeAction.Call();
        if (!_transHomeAction.LastRequestSuccess)
        {
            Debug.LogError("TransHomeAction request failed");
            yield break;
        }
        Debug.Log("TransHomeAction request success");
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_preScene);
    }

    private IEnumerator ExecuteCombatComplete()
    {
        yield return _dungeonGamePlayAction.Call("dungeon_combat_complete");
        if (!_dungeonGamePlayAction.LastRequestSuccess)
        {
            yield break;
        }

        UpdateTextFromAgentLogs();
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
        _mainText.text = MyUtils.MappingDisplayText(CurrentMapping()) + "\n" + MyUtils.DungeonCombatDisplayText(GameContext.Instance.Dungeon);
        UpdateMonsterImage();
    }

    private Dictionary<string, List<string>> CurrentMapping()
    {
        Dictionary<string, List<string>> currentMapping = new Dictionary<string, List<string>>();

        var currentActorStage = MyUtils.GetActorLocation(GameContext.Instance.ActorName, GameContext.Instance.Mapping);
        currentMapping[currentActorStage] = GameContext.Instance.Mapping[currentActorStage];

        return currentMapping;
    }


    private void UpdateMonsterImage()
    {
        _imageOrc.SetActive(false);
        _imageGoblin.SetActive(false);

        var actors = MyUtils.RetrieveActorsForStage(GameContext.Instance.ActorName, GameContext.Instance.Mapping);
        for (int i = 0; i < actors.Count; i++)
        {
            // TODO, 先这么招吧，无所谓的事。
            if (actors[i].Contains("哥布林"))
            {
                _imageGoblin.SetActive(true);
                _imageOrc.SetActive(false);
                return;
            }
            else if (actors[i].Contains("兽人"))
            {
                _imageGoblin.SetActive(false);
                _imageOrc.SetActive(true);
                return;
            }
        }
    }
}
