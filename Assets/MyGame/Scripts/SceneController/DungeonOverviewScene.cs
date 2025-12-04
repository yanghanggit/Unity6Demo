using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Linq;

public class DungeonOverviewScene : MonoBehaviour
{
    public string _preScene = "MainScene2";

    public string _nextScene = "DungeonScene";

    public TMP_Text _mainText;

    public TransDungeonApi _transDungeonApi;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        //Debug.Assert(_dungeonStateApi != null, "_viewDungeonAction is null");
        Debug.Assert(_transDungeonApi != null, "_transDungeonAction is null");

        // Start the coroutine to view the dungeon
        _mainText.text = "Loading dungeon data...";
        StartCoroutine(ExecuteViewDungeon());
    }

    public void OnClickTransDungeon()
    {
        Debug.Log("OnClickTransDungeon");
        StartCoroutine(ExecuteTransDungeon());
    }

    IEnumerator ExecuteTransDungeon()
    {
        if (_transDungeonApi == null)
        {
            yield break;
        }
        yield return _transDungeonApi.Call(GameContext.Instance.HomeTransDungeonUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        if (_transDungeonApi.RespData == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }

    IEnumerator ExecuteViewDungeon()
    {
        yield return GameStateSync.Instance.RefreshDungeonFromServer();

        _mainText.text = DungeonOverviewDisplayText(GameContext.Instance.Dungeon);
    }

    string DungeonOverviewDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        dungeon_text += "地下城 = " + dungeon.name + "\n";
        for (int i = 0; i < dungeon.stages.Count; i++)
        {
            dungeon_text += "第" + (i + 1) + "关 = " + dungeon.stages[i].name + "\n";
            dungeon_text += "怪物 = " + string.Join(", ", dungeon.stages[i].actors.Select(a => a.name)) + "\n";
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

        if (RootResp.Get() != null)
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
