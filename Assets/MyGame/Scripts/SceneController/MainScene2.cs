using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;

public class MainScene2 : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public LogoutApi _logoutApi;

    public GameObject _dungeonButton;

    void Start()
    {
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");

        // 直接刷新
        StartCoroutine(RefreshGameState());
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
        if (_logoutApi.RespData == null)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        GameContext.Instance.UserName = "";
        GameContext.Instance.GameName = "";
        GameContext.Instance.ActorName = "";

        SceneManager.LoadScene(_preScene);
    }

    private IEnumerator RefreshGameState()
    {
        // 必须先走这个一步，本质上是全局状态的刷新！
        yield return GameStateSync.Instance.RefreshStagesMappingAndActorsFromServer();

        // 测试一下是否可以正确获取玩家角色的 EntitySerialization 并序列化为 JSON。
        var playerActorEntitySerialization = GameContext.Instance.getActorEntitySerialization(GameContext.Instance.ActorName);
        if (playerActorEntitySerialization == null)
        {
            Debug.LogError($"Player actor entity serialization not found for actor: {GameContext.Instance.ActorName}");
            yield break;
        }

        try
        {
            // 直接将 EntitySerialization 序列化为 JSON 字符串
            string jsonString = JsonConvert.SerializeObject(playerActorEntitySerialization, Formatting.Indented);
            Debug.Log($"Actor[{GameContext.Instance.ActorName}] JSON:\n{jsonString}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to serialize Actor[{GameContext.Instance.ActorName}] to JSON: {ex.Message}");
        }
    }
}
