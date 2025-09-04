using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Linq;

public class ViewDungeonScene : MonoBehaviour
{
    public string _preScene = "MainScene2";

    public string _nextScene = "DungeonScene";

    public TMP_Text _mainText;

    public ViewDungeonAction _viewDungeonAction;

    public TransDungeonAction _transDungeonAction;


    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_viewDungeonAction != null, "_viewDungeonAction is null");
        Debug.Assert(_transDungeonAction != null, "_transDungeonAction is null");

        // Start the coroutine to view the dungeon
        StartCoroutine(ExecuteViewDungeon());
    }

    public void OnClickTransDungeon()
    {
        Debug.Log("OnClickTransDungeon");
        StartCoroutine(ExecuteTransDungeon());
    }

    IEnumerator ExecuteTransDungeon()
    {
        if (_transDungeonAction == null)
        {
            yield break;
        }
        yield return _transDungeonAction.Call();
        if (!_transDungeonAction.LastRequestSuccess)
        {
            yield break;
        }

        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }

    IEnumerator ExecuteViewDungeon()
    {
        yield return _viewDungeonAction.Call();
        if (!_viewDungeonAction.LastRequestSuccess)
        {
            yield break;
        }
        _mainText.text = DungeonOverviewDisplayText(GameContext.Instance.Dungeon);
    }

    string DungeonOverviewDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        dungeon_text += "地下城 = " + dungeon.name + "\n";
        for (int i = 0; i < dungeon.levels.Count; i++)
        {
            dungeon_text += "第" + (i + 1) + "关 = " + dungeon.levels[i].name + "\n";
            dungeon_text += "怪物 = " + string.Join(", ", dungeon.levels[i].actors.Select(a => a.name)) + "\n";
        }

        return dungeon_text;
    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToMainScene());
    }

    IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(0);

        if (GameContext.Instance.SetupGame)
        {
            Debug.Log("Returning to MainScene2");
            SceneManager.LoadScene(_preScene);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in CampScene.");
        }
    }
}
