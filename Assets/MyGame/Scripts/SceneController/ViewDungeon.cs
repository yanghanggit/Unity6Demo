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

    void Update()
    {

    }

    public void OnClickOpen()
    {
        Debug.Log("OnClickOpen");
        this.gameObject.SetActive(true);
        UpdateDungeonDisplay();
    }

    public void OnClickClose()
    {
        Debug.Log("OnClickClose");
        this.gameObject.SetActive(false);
    }

    void UpdateDungeonDisplay()
    {
        _mainText.text = MyUtils.DungeonDisplayText(GameContext.Instance.Dungeon, GameContext.Instance.Mapping);
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
            Debug.LogError("TransDungeonAction is null");
            yield break;
        }
        yield return StartCoroutine(_transDungeonAction.Call());
        if (!_transDungeonAction.RequestSuccess)
        {
            Debug.LogError("TransDungeonAction request failed");
            yield break;
        }
        
        Debug.Log("TransDungeonAction request success!!!!!! = ");
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
     }
}
