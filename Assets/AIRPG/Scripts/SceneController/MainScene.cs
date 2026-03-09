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

    public static readonly string PreSceneName = "LoginScene";
    public static readonly string NextSceneName = "HomeScene";
    public static readonly string DungeonOverviewSceneName = "DungeonOverviewScene";

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
    [SerializeField] private PlayerTopBar _topBar;

    /// <summary>
    /// 玩家详细信息面板UI对象(点击头像后显示)
    /// </summary>
    [SerializeField] private GameObject _playerInfoPanel;

    /// <summary>
    /// 角色迷你图标预制件
    /// </summary>
    [SerializeField] private GameObject _actorAvatarPrefab;


    [SerializeField] private HomeScenesPanel _homeScenesPanel;

    /// <summary>
    /// 场景启动初始化方法
    /// 验证所有必需的组件引用,注册UI事件回调,刷新游戏状态
    /// </summary>
    void Start()
    {
        Debug.Assert(_dungeonButton != null, "_dungeonButton is null");
        Debug.Assert(_topBar != null, "_topBar is null");
        Debug.Assert(_playerInfoPanel != null, "_playerInfoPanel is null");
        Debug.Assert(_actorAvatarPrefab != null, "_actorAvatarPrefab is null");
        Debug.Assert(_actorAvatarPrefab.GetComponent<ActorIcon>() != null, "ActorIcon component not found on _actorAvatarPrefab");
        Debug.Assert(_homeScenesPanel != null, "_homeScenesPanel is null");

        //_topBar.gameObject.setActive(true);
        _topBar.gameObject.SetActive(true);
        _playerInfoPanel.gameObject.SetActive(false);
        _homeScenesPanel.gameObject.SetActive(true);

        // 启动时立即刷新游戏状态
        RefreshActorLocations().Forget();
    }

    /// <summary>
    /// 返回按钮点击事件处理
    /// 触发登出流程并返回登录场景
    /// </summary>
    public void OnClickBack()
    {
        LogoutAsync().Forget();
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
    public void OnClickAdvanceGameState()
    {
        Debug.Log("Run button clicked in MainScene.");
        AdvanceGameStateAsync().Forget();
    }

    /// <summary>
    /// 玩家头像点击事件回调
    /// 显示玩家详细信息面板
    /// </summary>
    public void OnClickTopBarPlayerIcon()
    {
        //Debug.Log("Head icon clicked in MainScene!");
        _playerInfoPanel.SetActive(true);
    }

    /// <summary>
    /// 打开地牢浏览场景的协程
    /// 直接加载 ViewDungeonScene 场景
    /// </summary>
    private async UniTaskVoid LoadDungeonOverviewScene()
    {
        await UniTask.Yield();
        SceneManager.LoadScene(DungeonOverviewSceneName);
    }

    /// <summary>
    /// 返回登录场景的协程
    /// 1. 使用 SessionManager 执行登出
    /// 2. 加载登录场景
    /// </summary>
    private async UniTaskVoid LogoutAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, skipping logout process");
            return;
        }

        bool isLogoutSuccessful = await SessionManager.Instance.Logout();
        if (!isLogoutSuccessful)
        {
            Debug.LogError("[MainScene] Logout failed");
            return;
        }

        await UniTask.Yield();
        SceneManager.LoadScene(PreSceneName);
    }

    /// <summary>
    /// 推进游戏状态的协程
    /// 调用 HomeGamePlayManager 推进所有角色的行动，并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private async UniTaskVoid AdvanceGameStateAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot advance game state");
            return;
        }

        bool isGameSuccessfullyAdvanced = await HomeGamePlayManager.Instance.AdvanceGame(new List<string>());
        if (!isGameSuccessfullyAdvanced)
        {
            Debug.LogError("[MainScene] AdvanceGame failed");
            return;
        }

        Debug.Log("[MainScene] Game state advanced successfully");
        RefreshActorLocations().Forget();
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
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot transition to scene");
            await UniTask.Yield();
            HomeScene.PendingHomeSceneConfig = sceneConfig;
            SceneManager.LoadScene(NextSceneName);
            return;
        }


        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("Failed to refresh stage-actor mapping from server");
            return;
        }

        var currentStageName = string.Empty;
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                currentStageName = kvp.Key;
                break;
            }
        }

        Debug.Log($"Current stage: {currentStageName}, Target stage: {sceneConfig.StageName}");
        if (currentStageName != sceneConfig.StageName)
        {
            bool switchSuccess = await HomeGamePlayManager.Instance.SwitchStage(sceneConfig.StageName);

            if (!switchSuccess)
            {
                Debug.LogError($"[MainScene] SwitchStage to {sceneConfig.StageName} failed");
                return;
            }
        }
        else
        {
            Debug.Log($"Already in target stage: {sceneConfig.StageName}, no need to switch.");
        }

        await UniTask.Yield();
        HomeScene.PendingHomeSceneConfig = sceneConfig;
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 刷新场景中的角色位置显示
    /// 遍历所有角色,根据他们所在的 Stage 在对应区域显示迷你图标
    /// </summary>
    private async UniTaskVoid RefreshActorLocations()
    {
        // 清空现有图标
        for (int i = 0; i < _homeSceneObjects.Length; i++)
        {
            ClearActorIcons(_homeSceneObjects[i]);
        }

        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, skipping actor location refresh");
            return;
        }

        // 从服务器刷新场景-角色映射关系
        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("Failed to refresh stage-actor mapping from server");
            return;
        }

        // 遍历 stagesState 中的每个场景和对应的角色列表
        foreach (var kvp in stagesState)
        {
            var stageName = kvp.Key;
            var actorNames = kvp.Value;
            actorNames.Remove(GameContext.Instance.PlayerActorName); // 移除玩家自己

            for (int i = 0; i < _homeSceneNames.Length; i++)
            {
                if (stageName != _homeSceneNames[i])
                {
                    continue;
                }

                for (int j = 0; j < actorNames.Count; j++)
                {
                    var actorName = actorNames[j];
                    CreateActorIcon(actorName, _homeSceneObjects[i]);
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
        // 实例化图标
        GameObject iconInstance = Instantiate(_actorAvatarPrefab, container.transform);
        // 设置图标大小
        if (iconInstance.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.sizeDelta = new Vector2(100, 100); // 宽100像素，高100像素
        }
        iconInstance.GetComponent<ActorIcon>().ActorName = actorName;
    }

    /// <summary>
    /// 清空指定容器中的所有角色图标
    /// </summary>
    /// <param name="container">图标容器</param>
    private void ClearActorIcons(GameObject container)
    {
        // 销毁容器中的所有子对象
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
