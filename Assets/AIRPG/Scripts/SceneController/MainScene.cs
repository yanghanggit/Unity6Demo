using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

/// <summary>
/// 主场景控制器(MainScene)
/// 负责管理主场景的UI交互、场景切换、玩家信息显示等核心功能
/// 作为玩家进入游戏后的主要控制中心,提供前往不同游戏场景的入口
/// </summary>
public class MainScene : MonoBehaviour
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
    // [SerializeField] private HomeSceneConfig _campSceneConfig;

    // /// <summary>
    // /// 餐厅场景配置数据(包含 StageName 和 SceneDisplayName)
    // /// </summary>
    // [SerializeField] private HomeSceneConfig _restaurantSceneConfig;

    [Header("Home Scene Names and Objects")]
    [SerializeField] private string[] _homeSceneNames;
    [SerializeField] private GameObject[] _homeSceneObjects;

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
    /// 角色迷你图标预制件
    /// </summary>
    [SerializeField] private GameObject _actorAvatarPrefab;
    /// <summary>
    /// 场景启动初始化方法
    /// 验证所有必需的组件引用,注册UI事件回调,刷新游戏状态
    /// </summary>
    void Start()
    {
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");
        Debug.Assert(_playerInfoBar != null, "_playerInfoBar is null");
        Debug.Assert(_playerInfoDetails != null, "_playerInfoDetails is null");
        Debug.Assert(_actorAvatarPrefab != null, "_actorMiniIconPrefab is null");
        Debug.Assert(_actorAvatarPrefab.GetComponent<ActorMiniIcon>() != null, "ActorMiniIcon component not found on _actorMiniIconPrefab");

        // 设置头像点击回调
        _playerInfoBar.GetComponent<PlayerInfoBar>().OnHeadIconClickedCallback += OnHeadIconClicked;

        // 设置关闭回调
        _playerInfoDetails.GetComponent<PlayerInfoDetails>().OnCloseButtonClickedCallback += OnClickClosePlayerInfoDetails;
        _playerInfoDetails.SetActive(false);

        // 启动时立即刷新游戏状态
        RefreshGameState().Forget();
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
        ReturnToLoginScene().Forget();
    }

    /// <summary>
    /// 营地按钮点击事件处理
    /// 使用营地场景配置进行场景转换
    /// </summary>
    public void OnClickCamp()
    {
        OnClickHomeScene(0);
    }

    /// <summary>
    /// 餐厅按钮点击事件处理
    /// 使用餐厅场景配置进行场景转换
    /// </summary>
    public void OnClickRestaurant()
    {
        OnClickHomeScene(1);
    }

    /// <summary>
    /// 家园场景按钮点击事件处理
    /// 根据索引选择对应的家园场景进行转换
    /// </summary>
    private void OnClickHomeScene(int index)
    {
        if (index < 0 || index >= _homeSceneNames.Length || index >= _homeSceneObjects.Length)
        {
            Debug.LogError("Invalid home scene index: " + index);
            return;
        }

        var sceneName = _homeSceneNames[index];
        var tempConfig = ScriptableObject.CreateInstance<HomeSceneConfig>();
        tempConfig.StageName = sceneName;
        TransitionToScene(tempConfig).Forget();
    }

    /// <summary>
    /// 地牢按钮点击事件处理
    /// 打开地牢浏览场景
    /// </summary>
    public void OnClickDungeon()
    {
        LoadDungeonOverviewScene().Forget();
    }

    /// <summary>
    /// 运行按钮点击事件处理
    /// 推进游戏状态，让所有角色执行行动
    /// </summary>
    public void OnClickRun()
    {
        Debug.Log("Run button clicked in MainScene.");
        AdvanceGameState().Forget();
    }

    /// <summary>
    /// 玩家头像点击事件回调
    /// 显示玩家详细信息面板
    /// </summary>
    private void OnHeadIconClicked()
    {
        //Debug.Log("Head icon clicked in MainScene!");
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
    async UniTaskVoid LoadDungeonOverviewScene()
    {
        await UniTask.Yield();
        SceneManager.LoadScene(_dungeonOverviewScene);
    }

    /// <summary>
    /// 返回登录场景的协程
    /// 1. 使用 SessionManager 执行登出
    /// 2. 加载登录场景
    /// </summary>
    async UniTaskVoid ReturnToLoginScene()
    {
        bool logoutSuccess = await SessionManager.Instance.Logout();

        if (!logoutSuccess)
        {
            Debug.LogError("[MainScene] Logout failed");
            return;
        }

        await UniTask.Yield();
        SceneManager.LoadScene(_preScene);
    }

    /// <summary>
    /// 刷新游戏状态的协程
    /// 1. 从服务器刷新映射和角色数据
    /// 2. 获取并序列化玩家角色的实体数据(用于调试)
    /// 3. 刷新角色位置显示
    /// </summary>
    private async UniTaskVoid RefreshGameState()
    {
        await GameStateSync.Instance.RefreshMappingAndActorsFromServer();
        RefreshActorLocations();
    }

    /// <summary>
    /// 推进游戏状态的协程
    /// 调用 HomeGamePlayManager 推进所有角色的行动，并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid AdvanceGameState()
    {
        bool advanceSuccess = await HomeGamePlayManager.Instance.AdvanceGame(new List<string>());

        if (!advanceSuccess)
        {
            Debug.LogError("[MainScene] AdvanceGame failed");
            return;
        }

        Debug.Log("[MainScene] Game state advanced successfully");

        await GameStateSync.Instance.RefreshMappingAndActorsFromServer();
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
    private async UniTaskVoid TransitionToScene(HomeSceneConfig sceneConfig)
    {
        var currentStageName = GameContext.Instance.GetActorNameStage(GameContext.Instance.PlayerActorName);

        if (currentStageName != sceneConfig.StageName)
        {
            bool switchSuccess = await HomeGamePlayManager.Instance.SwitchStage(sceneConfig.StageName);

            if (!switchSuccess)
            {
                Debug.LogError($"[MainScene] SwitchStage to {sceneConfig.StageName} failed");
                return;
            }

            await GameStateSync.Instance.RefreshMappingAndActorsFromServer();
        }
        else
        {
            Debug.Log($"Already in target stage: {sceneConfig.StageName}, no need to switch.");
        }

        await UniTask.Yield();

        HomeScene.PendingHomeSceneConfig = sceneConfig;
        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// 刷新场景中的角色位置显示
    /// 遍历所有角色,根据他们所在的 Stage 在对应区域显示迷你图标
    /// </summary>
    private void RefreshActorLocations()
    {
        // 清空现有图标
        for (int i = 0; i < _homeSceneObjects.Length; i++)
        {
            ClearActorIcons(_homeSceneObjects[i]);
        }

        // 获取所有角色(排除玩家自己)
        var allActors = GameContext.Instance.ActorNames;
        foreach (var actorName in allActors)
        {
            // 跳过玩家自己
            if (actorName == GameContext.Instance.PlayerActorName)
            {
                continue;
            }

            // 获取角色所在的 Stage
            var actorStage = GameContext.Instance.GetActorNameStage(actorName);

            // 根据 Stage 在对应区域显示角色图标
            for (int i = 0; i < _homeSceneNames.Length; i++)
            {
                if (actorStage == _homeSceneNames[i])
                {
                    CreateActorIcon(actorName, _homeSceneObjects[i]);
                    break;
                }
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
        // 验证角色名称
        if (string.IsNullOrEmpty(actorName))
        {
            Debug.LogWarning("Cannot create actor icon with empty actor name");
            return;
        }

        // 实例化图标
        GameObject iconInstance = Instantiate(_actorAvatarPrefab, container.transform);
        // 设置图标大小
        RectTransform rectTransform = iconInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(100, 100); // 宽100像素，高100像素
        }
        iconInstance.name = actorName;
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
        {
            Destroy(child.gameObject);
        }
    }
}
