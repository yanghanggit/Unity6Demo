using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
//using UnityEngine.UI;
public class MainScene2 : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public LogoutAction _logoutAction;

    public StagesStateAction _viewHomeAction;

    public ActorDetailsAction _viewActorAction;

    public GameObject _dungeonButton;

    void Start()
    {
        Debug.Assert(_logoutAction != null, "_logoutAction is null");
        Debug.Assert(_viewHomeAction != null, "_viewHomeAction is null");
        Debug.Assert(_viewActorAction != null, "_viewActorAction is null");
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");

        StartCoroutine(LoadHomeAndActorData());
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
        yield return _logoutAction.Call(GameContext.Instance.LogoutUrl, GameContext.Instance.UserName, GameContext.Instance.GameName);
        if (_logoutAction.ReqResult == null || !_logoutAction.ReqResult.isSuccess)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        GameContext.Instance.UserName = "";
        GameContext.Instance.GameName = "";
        GameContext.Instance.ActorName = "";

        SceneManager.LoadScene(_preScene);
    }


    private IEnumerator LoadHomeAndActorData()
    {
        yield return _viewHomeAction.Call(GameContext.Instance.HomeStateUrl);
        if (_viewHomeAction.ResponseData == null)
        {
            yield break;
        }

        GameContext.Instance.Mapping = _viewHomeAction.ResponseData.mapping;

        //提取Mapping中所有的values组成一个List
        List<string> allActors = new List<string>();
        foreach (var kvp in GameContext.Instance.Mapping)
        {
            allActors.AddRange(kvp.Value);
        }
        //打印 allActors
        Debug.Log("All Actors: " + string.Join(", ", allActors));
        yield return _viewActorAction.Call(GameContext.Instance.ActorDetailsUrl, allActors);
        if (_viewActorAction.ResponseData == null)
        {
            yield break;
        }

        GameContext.Instance.ActorEntitiesSerialization = _viewActorAction.ResponseData.actor_entities_serialization;

        Debug.Log("Home and Actor views updated");

        // 打印 GameContext.Instance.ActorEntitiesSerialization 的详细信息
        var actorEntitiesSerialization = GameContext.Instance.ActorEntitiesSerialization;
        for (int i = 0; i < actorEntitiesSerialization.Count; i++)
        {
            var entitySerialization = actorEntitiesSerialization[i];
            try
            {
                // 直接将 EntitySerialization 序列化为 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(entitySerialization, Formatting.Indented);
                Debug.Log($"Actor[{i}] JSON:\n{jsonString}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to serialize Actor[{i}] to JSON: {ex.Message}");
            }
        }
    }
}
