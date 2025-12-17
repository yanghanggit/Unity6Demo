using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
// using Newtonsoft.Json;
// using System;

/// <summary>
/// 主场景控制器(MainScene2)
/// 负责管理主场景的UI交互、场景切换、玩家信息显示等核心功能
/// 作为玩家进入游戏后的主要控制中心,提供前往不同游戏场景的入口
/// </summary>
public class MainScene2 : MonoBehaviour
{

    [Header("Scene Settings")]
    /// <summary>
    /// 返回按钮要跳转的上一个场景名称
    /// </summary>
    [SerializeField] private string _preScene = "LoginScene";

    /// <summary>
    /// 场景转换时要加载的目标场景名称
    /// </summary>
    [SerializeField] private string _nextScene = "HomeScene";

    /// <summary>
    /// 地牢浏览场景名称
    /// </summary>
    [SerializeField] private string _dungeonOverviewScene = "DungeonOverviewScene";


    [Header("HomeSceneConfigs")]
    /// <summary>
    /// 营地场景配置数据(包含 StageName 和 SceneDisplayName)
    /// </summary>
    [SerializeField] private HomeSceneConfig _campSceneConfig;

    /// <summary>
    /// 餐厅场景配置数据(包含 StageName 和 SceneDisplayName)
    /// </summary>
    [SerializeField] private HomeSceneConfig _restaurantSceneConfig;

    [Header("UI Components")]
    /// <summary>
    /// 地牢按钮对象
    /// </summary>
    [SerializeField] private GameObject _dungeonButton;

    /// <summary>
    /// 玩家信息栏UI对象(显示玩家头像等基本信息)
    /// </summary>
    [SerializeField] private GameObject _playerInfoBar;

    /// <summary>
    /// 玩家详细信息面板UI对象(点击头像后显示)
    /// </summary>
    [SerializeField] private GameObject _playerInfoDetails;

    /// <summary>
    /// 营地区域角色图标容器
    /// </summary>
    [SerializeField] private GameObject _campActorIconsContainer;

    /// <summary>
    /// 餐厅区域角色图标容器
    /// </summary>
    [SerializeField] private GameObject _restaurantActorIconsContainer;

    /// <summary>
    /// 角色迷你图标预制件
    /// </summary>
    [SerializeField] private GameObject _actorMiniIconPrefab;
    /// <summary>
    /// 场景启动初始化方法
    /// 验证所有必需的组件引用,注册UI事件回调,刷新游戏状态
    /// </summary>
    void Start()
    {
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");
        Debug.Assert(_playerInfoBar != null, "_playerInfoBar is null");
        Debug.Assert(_playerInfoDetails != null, "_playerInfoDetails is null");
        Debug.Assert(_campSceneConfig != null, "_campSceneConfig is null");
        Debug.Assert(_restaurantSceneConfig != null, "_restaurantSceneConfig is null");
        Debug.Assert(_campActorIconsContainer != null, "_campActorIconsContainer is null");
        Debug.Assert(_restaurantActorIconsContainer != null, "_restaurantActorIconsContainer is null");
        Debug.Assert(_actorMiniIconPrefab != null, "_actorMiniIconPrefab is null");
        Debug.Assert(_actorMiniIconPrefab.GetComponent<ActorMiniIcon>() != null, "ActorMiniIcon component not found on _actorMiniIconPrefab");

        // 设置头像点击回调
        _playerInfoBar.GetComponent<PlayerInfoBar>().OnHeadIconClickedCallback += OnHeadIconClicked;

        // 设置关闭回调
        _playerInfoDetails.GetComponent<PlayerInfoDetails>().OnCloseButtonClickedCallback += OnClickClosePlayerInfoDetails;
        _playerInfoDetails.SetActive(false);

        // 启动时立即刷新游戏状态
        StartCoroutine(RefreshGameState());
    }

    /// <summary>
    /// 场景销毁时的清理方法
    /// 取消注册所有事件回调,防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        // 清除玩家信息栏的头像点击回调
        if (_playerInfoBar != null)
        {
            PlayerInfoBar playerInfoBar = _playerInfoBar.GetComponent<PlayerInfoBar>();
            if (playerInfoBar != null)
            {
                playerInfoBar.OnHeadIconClickedCallback -= OnHeadIconClicked;
            }
        }

        // 清除玩家详细信息面板的关闭按钮回调
        if (_playerInfoDetails != null)
        {
            PlayerInfoDetails playerInfoDetails = _playerInfoDetails.GetComponent<PlayerInfoDetails>();
            if (playerInfoDetails != null)
            {
                playerInfoDetails.OnCloseButtonClickedCallback -= OnClickClosePlayerInfoDetails;
            }
        }
    }

    /// <summary>
    /// 返回按钮点击事件处理
    /// 触发登出流程并返回登录场景
    /// </summary>
    public void OnClickBack()
    {
        //Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    /// <summary>
    /// 营地按钮点击事件处理
    /// 使用营地场景配置进行场景转换
    /// </summary>
    public void OnClickCamp()
    {
        //Debug.Log("OnClickCamp");
        StartCoroutine(TransitionToScene(_campSceneConfig));
    }

    /// <summary>
    /// 餐厅按钮点击事件处理
    /// 使用餐厅场景配置进行场景转换
    /// </summary>
    public void OnClickRestaurant()
    {
        //Debug.Log("OnClickRestaurant");
        StartCoroutine(TransitionToScene(_restaurantSceneConfig));
    }

    /// <summary>
    /// 地牢按钮点击事件处理
    /// 打开地牢浏览场景
    /// </summary>
    public void OnClickDungeon()
    {
        //Debug.Log("OnClickDungeon");
        StartCoroutine(LoadDungeonOverviewScene());
    }

    /// <summary>
    /// 运行按钮点击事件处理
    /// 推进游戏状态，让所有角色执行行动
    /// </summary>
    public void OnClickRun()
    {
        Debug.Log("Run button clicked in MainScene2.");
        StartCoroutine(AdvanceGameState());
    }

    /// <summary>
    /// 玩家头像点击事件回调
    /// 显示玩家详细信息面板
    /// </summary>
    private void OnHeadIconClicked()
    {
        //Debug.Log("Head icon clicked in MainScene2!");
        _playerInfoDetails.SetActive(true);
    }

    /// <summary>
    /// 关闭玩家详细信息面板的回调
    /// 隐藏玩家详细信息面板
    /// </summary>
    public void OnClickClosePlayerInfoDetails()
    {
        //Debug.Log("Player info details clicked!");
        _playerInfoDetails.SetActive(false);
    }

    /// <summary>
    /// 打开地牢浏览场景的协程
    /// 直接加载 ViewDungeonScene 场景
    /// </summary>
    IEnumerator LoadDungeonOverviewScene()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_dungeonOverviewScene);
    }

    /// <summary>
    /// 返回登录场景的协程
    /// 1. 使用 SessionManager 执行登出
    /// 2. 加载登录场景
    /// </summary>
    IEnumerator ReturnToLoginScene()
    {
        // 使用 SessionManager 执行登出
        bool logoutSuccess = false;
        yield return SessionManager.Instance.Logout(
            (success) => logoutSuccess = success
        );

        // 检查登出是否成功
        if (!logoutSuccess)
        {
            Debug.LogError("[MainScene2] Logout failed");
            yield break;
        }

        // 加载并返回到登录场景
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_preScene);
    }

    /// <summary>
    /// 刷新游戏状态的协程
    /// 1. 从服务器刷新映射和角色数据
    /// 2. 获取并序列化玩家角色的实体数据(用于调试)
    /// 3. 刷新角色位置显示
    /// </summary>
    private IEnumerator RefreshGameState()
    {
        // 从服务器同步最新的全局状态(包括映射和所有角色数据)
        yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

        // 获取玩家角色的实体序列化数据用于验证和调试
        // var playerActorEntitySerialization = GameContext.Instance.GetActorEntitySerialization(GameContext.Instance.ActorName);
        // if (playerActorEntitySerialization == null)
        // {
        //     Debug.LogError($"Player actor entity serialization not found for actor: {GameContext.Instance.ActorName}");
        //     yield break;
        // }

        // try
        // {
        //     // 将玩家角色实体数据序列化为 JSON 并输出到日志(用于调试)
        //     string jsonString = JsonConvert.SerializeObject(playerActorEntitySerialization, Formatting.Indented);
        //     Debug.Log($"Actor[{GameContext.Instance.ActorName}] JSON:\n{jsonString}");
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError($"Failed to serialize Actor[{GameContext.Instance.ActorName}] to JSON: {ex.Message}");
        // }

        // 刷新角色位置显示
        RefreshActorLocations();
    }

    /// <summary>
    /// 验证当前玩家角色是否成功执行了场景转换事件
    /// 通过检查最近的代理事件历史,确认当前玩家是否在切换场景的角色集合中
    /// </summary>
    /// <returns>如果当前玩家角色执行了场景转换返回 true,否则返回 false</returns>
    // private bool ValidateTransStageEvent()
    // {
    //     var lastAgentEventsHistory = GameContext.Instance.LastAgentEventsHistory;
    //     var actorsWithTransStageEvents = GameUtils.GetActorsWithEventType<TransStageEvent>(lastAgentEventsHistory);
    //     //Debug.Log($"Actors with TransStageEvents: {string.Join(", ", actorsWithTransStageEvents)}");
    //     return actorsWithTransStageEvents.Contains(GameContext.Instance.ActorName);
    // }

    /// <summary>
    /// 推进游戏状态的协程
    /// 调用 HomeGamePlayManager 推进所有角色的行动，并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator AdvanceGameState()
    {
        // 使用 HomeGamePlayManager 推进游戏
        bool advanceSuccess = false;
        yield return HomeGamePlayManager.Instance.AdvanceGame(
            (success) =>
            {
                advanceSuccess = success;
            }
        );

        // 检查推进是否成功
        if (!advanceSuccess)
        {
            Debug.LogError("[MainScene2] AdvanceGame failed");
            yield break;
        }

        Debug.Log("[MainScene2] Game state advanced successfully");

        // 刷新游戏状态以确保数据同步
        yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

        // 刷新角色位置显示
        RefreshActorLocations();
    }

    /// <summary>
    /// 将玩家角色转移到指定的 Stage(服务器状态) 和 Scene(Unity 场景)
    /// 完整流程:
    /// 1. 检查玩家当前是否已在目标 Stage
    /// 2. 如果不在,调用服务器 API 切换 Stage
    /// 3. 刷新游戏状态并验证转换事件
    /// 4. 设置待处理的场景配置并加载目标 Unity 场景
    /// </summary>
    /// <param name="sceneConfig">目标场景的配置数据(包含 StageName 和 SceneDisplayName)</param>
    private IEnumerator TransitionToScene(HomeSceneConfig sceneConfig)
    {
        // 获取玩家当前所在的 Stage 名称
        var currentStageName = GameContext.Instance.GetActorStage(GameContext.Instance.ActorName);

        // 检查玩家是否已在目标 Stage 中
        if (currentStageName != sceneConfig.StageName)
        {
            // 玩家不在目标 Stage,使用 HomeGamePlayManager 切换 Stage
            bool switchSuccess = false;
            yield return HomeGamePlayManager.Instance.SwitchStage(
                sceneConfig.StageName,
                (success) =>
                {
                    switchSuccess = success;
                }
            );

            // 检查切换是否成功
            if (!switchSuccess)
            {
                Debug.LogError($"[MainScene2] SwitchStage to {sceneConfig.StageName} failed");
                yield break;
            }

            // 刷新全局状态以确保数据同步
            yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

            // 验证场景转换事件
            // var isTransStageEventValid = ValidateTransStageEvent();
            // Debug.Assert(isTransStageEventValid, "ValidateTransStageEvent failed");
            // Debug.Log($"[MainScene2] TransitionToScene to {sceneConfig.StageName} completed");
        }
        else
        {
            // 玩家已在目标 Stage 中,无需切换服务器状态
            Debug.Log($"Already in target stage: {sceneConfig.StageName}, no need to switch.");
        }

        // 短暂等待以确保所有异步操作完成
        yield return new WaitForSeconds(0.0f);

        // 将场景配置设置到 HomeScene 的静态属性,供下一个场景读取
        HomeScene.PendingHomeSceneConfig = sceneConfig;

        // 加载目标 Unity 场景
        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// 刷新场景中的角色位置显示
    /// 遍历所有角色,根据他们所在的 Stage 在对应区域显示迷你图标
    /// </summary>
    private void RefreshActorLocations()
    {
        // 清空现有图标
        ClearActorIcons(_campActorIconsContainer);
        ClearActorIcons(_restaurantActorIconsContainer);

        // 获取所有角色(排除玩家自己)
        var allActors = GameContext.Instance.AllActors;
        foreach (var actorName in allActors)
        {
            // 跳过玩家自己
            if (actorName == GameContext.Instance.ActorName)
            {
                continue;
            }

            // 获取角色所在的 Stage
            var actorStage = GameContext.Instance.GetActorStage(actorName);

            // 根据 Stage 在对应区域显示角色图标
            if (actorStage == _campSceneConfig.StageName)
            {
                CreateActorIcon(actorName, _campActorIconsContainer);
            }
            else if (actorStage == _restaurantSceneConfig.StageName)
            {
                CreateActorIcon(actorName, _restaurantActorIconsContainer);
            }
        }
    }

    /// <summary>
    /// 创建角色迷你图标
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <param name="container">图标容器</param>
    private void CreateActorIcon(string actorName, GameObject container)
    {
        if (container == null || _actorMiniIconPrefab == null)
        {
            return;
        }

        // 验证角色名称
        if (string.IsNullOrEmpty(actorName))
        {
            Debug.LogWarning("Cannot create actor icon with empty actor name");
            return;
        }

        // 实例化图标
        GameObject iconObj = Instantiate(_actorMiniIconPrefab, container.transform);
        ActorMiniIcon icon = iconObj.GetComponent<ActorMiniIcon>();
        
        iconObj.SetActive(true);
        icon.BindActor(actorName);
    }

    /// <summary>
    /// 清空指定容器中的所有角色图标
    /// </summary>
    /// <param name="container">图标容器</param>
    private void ClearActorIcons(GameObject container)
    {
        if (container == null)
        {
            return;
        }

        // 销毁容器中的所有子对象
        foreach (Transform child in container.transform)
        {            Destroy(child.gameObject);
        }
    }
}
