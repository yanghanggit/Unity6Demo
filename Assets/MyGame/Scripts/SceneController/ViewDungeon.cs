using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ViewDungeon : MonoBehaviour
{

    public TMP_Text _mainText;

    public TransDungeonAction _transDungeonAction;

    public string _nextScene = "DungeonScene";

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_transDungeonAction != null, "_transDungeonAction is null");
    }

    public void OnClickClose()
    {
        Debug.Log("OnClickClose");
        this.gameObject.SetActive(false);
    }

    public void UpdateDungeonDisplay()
    {
        _mainText.text = MyUtils.DungeonDisplayText(GameContext.Instance.Dungeon);
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
        yield return StartCoroutine(_transDungeonAction.Call());
        if (!_transDungeonAction.RequestSuccess)
        {
            yield break;
        }

        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }
}
