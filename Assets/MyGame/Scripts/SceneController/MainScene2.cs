using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
public class MainScene2 : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public LogoutApi _logoutApi;

    public HomeGamePlayApi _homeGamePlayApi;

    public GameObject _dungeonButton;

    public GameObject _playerInfoBar;

    public GameObject _playerInfoDetails;

    public HomeSceneConfig _campSceneConfig;

    public HomeSceneConfig _restaurantSceneConfig;

    void Start()
    {
        Debug.Assert(_logoutApi != null, "_logoutApi is null");
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");
        Debug.Assert(_playerInfoBar != null, "_playerInfoBar is null");
        Debug.Assert(_playerInfoDetails != null, "_playerInfoDetails is null");
        Debug.Assert(_campSceneConfig != null, "_campSceneConfig is null");
        Debug.Assert(_restaurantSceneConfig != null, "_restaurantSceneConfig is null");

        // 设置头像点击回调
        _playerInfoBar.GetComponent<PlayerInfoBar>().OnHeadIconClickedCallback += OnHeadIconClicked;

        // 设置关闭回调
        _playerInfoDetails.GetComponent<PlayerInfoDetails>().OnCloseButtonClickedCallback += OnClickClosePlayerInfoDetails;
        _playerInfoDetails.SetActive(false);

        // 直接刷新
        StartCoroutine(RefreshGameState());
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        // 清除回调,避免内存泄漏
        Debug.Assert(_playerInfoBar != null, "_playerInfoBar is null");
        Debug.Assert(_playerInfoBar.GetComponent<PlayerInfoBar>() != null, "_playerInfoBar PlayerInfoBar component is null");
        Debug.Assert(_playerInfoDetails != null, "_playerInfoDetails is null");
        Debug.Assert(_playerInfoDetails.GetComponent<PlayerInfoDetails>() != null, "_playerInfoDetails PlayerInfoDetails component is null");

        _playerInfoBar.GetComponent<PlayerInfoBar>().OnHeadIconClickedCallback -= OnHeadIconClicked;
        _playerInfoDetails.GetComponent<PlayerInfoDetails>().OnCloseButtonClickedCallback -= OnClickClosePlayerInfoDetails;
    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickCamp()
    {
        Debug.Log("OnClickCamp");
        StartCoroutine(TransitionToStageAndScene(_campSceneConfig.StageName, _campSceneConfig.SceneDisplayName));
    }

    public void OnClickRestaurant()
    {
        Debug.Log("OnClickRestaurant");
        StartCoroutine(TransitionToStageAndScene(_restaurantSceneConfig.StageName, _restaurantSceneConfig.SceneDisplayName));
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
        yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

        // 测试一下是否可以正确获取玩家角色的 EntitySerialization 并序列化为 JSON。
        var playerActorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
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

    private void OnHeadIconClicked()
    {
        Debug.Log("Head icon clicked in MainScene2!");
        _playerInfoDetails.SetActive(true);
    }

    public void OnClickClosePlayerInfoDetails()
    {
        Debug.Log("Player info details clicked!");
        _playerInfoDetails.SetActive(false);
    }

    /// <summary>
    /// 将玩家角色转移到指定的Stage(服务器状态)和Scene(Unity场景)
    /// 如果玩家已在目标Stage中,则直接加载Scene
    /// </summary>
    private IEnumerator TransitionToStageAndScene(string targetStageName, string loadSceneName)
    {
        // 是否已经在该场景中
        var currentStageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        if (currentStageName != targetStageName)
        {
            // 不在，通知服务器转换场景
            yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/trans_home",
            new Dictionary<string, string>
            {
                ["stage_name"] = targetStageName
            });

            if (_homeGamePlayApi.RespData == null)
            {
                // 请求失败，就不能往后走
                Debug.LogError($"ExecuteTransStage = {targetStageName} request failed");
                yield break;
            }

            yield return GameStateSync.Instance.FetchSessionMessagesFromServer(
            (sessionMessages) =>
                {
                    Debug.Log($"Fetched {sessionMessages.Count} session messages from server after transitioning to stage {targetStageName}");
                }
            );

            // 请求成功
            Debug.Log($"ExecuteTransStage = {targetStageName} completed");
        }
        else
        {
            Debug.Log($"Already in target stage: {targetStageName}");
        }

        // 到这里一定能打开场景，就进行切换！
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(loadSceneName);
    }
}
