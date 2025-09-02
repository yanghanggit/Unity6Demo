using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainScene2 : MonoBehaviour
{

    public string _preScene = "LoginScene";

    public LogoutAction _logoutAction;

    

    void Start()
    {
        Debug.Assert(_logoutAction != null, "_logoutAction is null");

    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickCamp()
    {
        Debug.Log("OnClickCamp");
    }

    public void OnClickDungeon()
    {
        Debug.Log("OnClickDungeon");
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
}
