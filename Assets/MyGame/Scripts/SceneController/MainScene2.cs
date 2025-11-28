using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using Newtonsoft.Json;
//using UnityEngine.UI;
public class MainScene2 : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public LogoutApi _logoutApi;

    //public StagesStateApi _stageStateApi;

   // public ActorDetailsApi _actorDetailApi;

    public GameObject _dungeonButton;

    void Start()
    {
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
        //Debug.Assert(_stageStateApi != null, "_stageStateApi is null");
        //Debug.Assert(_actorDetailApi != null, "_actorDetailApi is null");
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");

        // 直接刷新
        StartCoroutine(GameStateSync.Instance.RefreshStagesMappingAndActorsFromServer());
    }

    void Update()
    {

    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickCamp()
    {
        Debug.Log("OnClickCamp");
        StartCoroutine(OpenCampScene());
    }

    IEnumerator OpenCampScene()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene("CampScene");
    }

    public void OnClickRestaurant()
    {
        Debug.Log("OnClickRestaurant");
        StartCoroutine(OpenRestaurantScene());
    }

    IEnumerator OpenRestaurantScene()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene("RestaurantScene");
    }
    public void OnClickDungeon()
    {
        Debug.Log("OnClickDungeon");
        StartCoroutine(OpenViewDungeonScene());
    }

    IEnumerator OpenViewDungeonScene()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene("ViewDungeonScene");
    }

    IEnumerator ReturnToLoginScene()
    {
        yield return _logoutApi.Call(GameContext.Instance.LogoutUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        if (_logoutApi.ReqResult == null || !_logoutApi.ReqResult.isSuccess)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        GameContext.Instance.UserName = "";
        GameContext.Instance.GameName = "";
        GameContext.Instance.ActorName = "";

        SceneManager.LoadScene(_preScene);
    }
}
