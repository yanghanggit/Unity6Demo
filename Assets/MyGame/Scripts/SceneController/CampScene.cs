using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using Newtonsoft.Json;
public class CampScene : MonoBehaviour
{
    public string _preScene = "MainScene2";

    void Start()
    {

    }

    void Update()
    {

    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToMainScene());
    }

    IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(0);
        Debug.Log("Returning to MainScene2");
        //SceneManager.LoadScene(_preScene);
    }
}
