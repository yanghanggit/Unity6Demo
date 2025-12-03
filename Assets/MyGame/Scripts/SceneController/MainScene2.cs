using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
public class MainScene2 : MonoBehaviour
{
    // 静态属性用于场景间传递配置
    public static HomeSceneConfig PendingHomeSceneConfig { get; set; }

    public string _preScene = "LoginScene";

    public string _nextScene = "HomeScene";

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

    void OnDestroy()
    {
        // 清除回调,避免内存泄漏
        if (_playerInfoBar != null)
        {
            PlayerInfoBar playerInfoBar = _playerInfoBar.GetComponent<PlayerInfoBar>();
            if (playerInfoBar != null)
            {
                playerInfoBar.OnHeadIconClickedCallback -= OnHeadIconClicked;
            }
        }

        if (_playerInfoDetails != null)
        {
            PlayerInfoDetails playerInfoDetails = _playerInfoDetails.GetComponent<PlayerInfoDetails>();
            if (playerInfoDetails != null)
            {
                playerInfoDetails.OnCloseButtonClickedCallback -= OnClickClosePlayerInfoDetails;
            }
        }
    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickCamp()
    {
        Debug.Log("OnClickCamp");
        StartCoroutine(TransitionToScene(_campSceneConfig));
    }

    public void OnClickRestaurant()
    {
        Debug.Log("OnClickRestaurant");
        StartCoroutine(TransitionToScene(_restaurantSceneConfig));
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

        // 清除数据
        GameContext.ClearInstance();

        // 返回登录场景
        yield return new WaitForSeconds(0);
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
    /// 验证场景转换事件是否成功
    /// 检查是否收到了预期的 TransStageEvent
    /// </summary>
    private bool ValidateTransStageEvent(List<SessionMessage> sessionMessages, string targetStageName)
    {
        bool checkTransStageEvent = false;

        Debug.Log($"Fetched {sessionMessages.Count} session messages from server after transitioning to stage {targetStageName}");
        var lastAgentEventsHistory = GameContext.Instance.LastAgentEventsHistory;
        if (lastAgentEventsHistory.ContainsKey(GameContext.Instance.ActorName))
        {
            var agentEvents = lastAgentEventsHistory[GameContext.Instance.ActorName];
            Debug.Log($"There are {agentEvents.Count} agent events for actor {GameContext.Instance.ActorName} after transitioning to stage {targetStageName}");

            foreach (var agentEvent in agentEvents)
            {
                if (agentEvent.head == (int)EventHead.TRANS_STAGE_EVENT)
                {
                    TransStageEvent transStageEvent = (TransStageEvent)agentEvent;
                    Debug.Assert(transStageEvent.actor == GameContext.Instance.ActorName, "TransStageEvent actor does not match current actor");
                    Debug.Log($"{transStageEvent.actor} Agent Event: (trans_stage) from {transStageEvent.from_stage} to {transStageEvent.to_stage}");
                    checkTransStageEvent = true;
                }
            }
        }

        if (!checkTransStageEvent)
        {
            Debug.LogWarning($"No TransStageEvent found for actor {GameContext.Instance.ActorName} after transitioning to stage {targetStageName}");
        }

        return checkTransStageEvent;
    }

    /// <summary>
    /// 将玩家角色转移到指定的Stage(服务器状态)和Scene(Unity场景)
    /// 如果玩家已在目标Stage中,则直接加载Scene
    /// </summary>
    private IEnumerator TransitionToScene(HomeSceneConfig sceneConfig)
    {
        // 是否已经在该场景中
        var currentStageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);
        if (currentStageName != sceneConfig.StageName)
        {
            // 不在，通知服务器转换场景
            yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/switch_stage",
            new Dictionary<string, string>
            {
                ["stage_name"] = sceneConfig.StageName
            });

            if (_homeGamePlayApi.RespData == null)
            {
                // 请求失败，就不能往后走
                Debug.LogError($"ExecuteTransStage = {sceneConfig.StageName} request failed");
                yield break;
            }

            // 请求成功，刷新全局状态，这么做有点笨，但保证万无一失
            yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

            // 尝试获取最新的消息并验证场景转换事件
            yield return GameStateSync.Instance.FetchSessionMessagesFromServer(
            (sessionMessages) =>
                {
                    ValidateTransStageEvent(sessionMessages, sceneConfig.StageName);
                }
            );

            // 请求成功
            Debug.Log($"ExecuteTransStage = {sceneConfig.StageName} completed");
        }
        else
        {
            Debug.Log($"Already in target stage: {sceneConfig.StageName}, no need to switch.");
        }

        // 到这里一定能打开场景，就进行切换！
        yield return new WaitForSeconds(0.0f);

        // 将配置传递给下一个场景
        PendingHomeSceneConfig = sceneConfig;

        // 加载目标场景
        SceneManager.LoadScene("HomeScene");
    }
}
