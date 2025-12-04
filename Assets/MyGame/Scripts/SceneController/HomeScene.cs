using UnityEngine;
using Mosframe;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 主场景控制器
/// 负责管理主场景的UI交互、角色选择和状态切换
/// </summary>
public class HomeScene : MonoBehaviour, IStringGameEventListener
{
    /// <summary>
    /// 静态属性用于在场景切换时传递 HomeSceneConfig 配置数据
    /// MainScene2 会在切换场景前设置此属性,HomeScene 在 Awake 时读取并清空
    /// </summary>
    public static HomeSceneConfig PendingHomeSceneConfig { get; set; }
    // UI组件引用
    [Header("UI Components")]
    [SerializeField] private GameObject _background;            // 场景背景
    [SerializeField] private GameObject _currentActor;          // 当前选中的角色显示对象
    [SerializeField] private GameObject _speechBubble;          // 对话气泡UI
    [SerializeField] private TMP_Text _speechBubbleText;        // 对话气泡文本
    [SerializeField] private GameObject _mainState;             // 主状态UI容器
    [SerializeField] private GameObject _inputState;            // 输入状态UI容器
    [SerializeField] private TMP_InputField _inputField;       // 输入字段 (TMP)
    [SerializeField] private DynamicScrollView _scrollView;     // 动态滚动视图

    // 配置和API
    [Header("Scene Config")]
    [SerializeField] private HomeSceneConfig _homeSceneConfig; // 场景配置数据
    [SerializeField] private string _preScene = "MainScene2";   // 上一个场景名称

    [Header("API Components")]
    [SerializeField] private HomeGamePlayApi _homeGamePlayApi; // 游戏玩法API接口

    // 事件系统
    [Header("Events")]
    [SerializeField] private StringGameEvent _onActorClickedEvent; // 角色点击事件

    // 私有成员变量
    /// <summary>
    /// 当前选中的角色名称
    /// </summary>
    private string _selectedActorName = string.Empty;

    //
    void Awake()
    {
        // 如果有从 MainScene2 传递过来的配置,使用它
        if (PendingHomeSceneConfig != null)
        {
            _homeSceneConfig = PendingHomeSceneConfig;
            PendingHomeSceneConfig = null; // 用完立即清空
        }

        /// 验证所有必需的组件引用
        // 背景组件验证
        Debug.Assert(_background != null, "_background is null");
        Debug.Assert(_background.GetComponent<SpriteRenderer>() != null, "_background is missing SpriteRenderer component");
        Debug.Assert(_background.GetComponent<BoxCollider2D>() != null, "_background is missing BoxCollider2D component");
        Debug.Assert(_background.GetComponent<SpriteClickHandler>() != null, "_background is missing SpriteClickHandler component");

        // 角色组件验证
        Debug.Assert(_currentActor != null, "_currentActor is null");
        Debug.Assert(_currentActor.GetComponent<SpriteRenderer>() != null, "_currentActor is missing SpriteRenderer component");
        Debug.Assert(_currentActor.GetComponent<BoxCollider2D>() != null, "_currentActor is missing BoxCollider2D component");
        Debug.Assert(_currentActor.GetComponent<SpriteClickHandler>() != null, "_currentActor is missing SpriteClickHandler component");
        Debug.Assert(_speechBubble != null, "_speechBubble is null");
        Debug.Assert(_speechBubbleText != null, "_speechBubbleText is null");
        Debug.Assert(_mainState != null, "_mainState is null");
        Debug.Assert(_inputState != null, "_inputState is null");
        Debug.Assert(_inputField != null, "_inputField is null");
        Debug.Assert(_scrollView != null, "_scrollView is null");
        Debug.Assert(_homeSceneConfig != null, "_homeSceneConfig is null");
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
        Debug.Assert(_onActorClickedEvent != null, "onActorClickedEvent is null");

        // 设置背景图像
        var stageSprite = TextureManager.Instance.GetSprite(_homeSceneConfig.StageName);
        if (stageSprite != null)
        {
            _background.GetComponent<SpriteRenderer>().sprite = stageSprite;
        }
        else
        {
            Debug.LogWarning("Stage sprite not found for: " + _homeSceneConfig.StageName);
        }
    }

    // Unity生命周期方法
    /// <summary>
    /// 场景初始化方法
    /// 执行组件引用验证和初始UI状态设置
    /// 注册所有事件监听器
    /// </summary>
    void Start()
    {
        /// 注册所有事件监听器
        // 注册角色点击事件监听器
        _onActorClickedEvent.RegisterListener(this);

        // 注册背景点击事件
        SpriteClickHandler backgroundClickHandler = _background.GetComponent<SpriteClickHandler>();
        Debug.Assert(backgroundClickHandler != null, "_background is missing SpriteClickHandler component");
        backgroundClickHandler.OnSpriteClicked += OnBackgroundClicked;

        // 注册当前角色点击事件
        SpriteClickHandler currentActorClickHandler = _currentActor.GetComponent<SpriteClickHandler>();
        Debug.Assert(currentActorClickHandler != null, "_currentActor is missing SpriteClickHandler component");
        currentActorClickHandler.OnSpriteClicked += OnCurrentActorClicked;

        // 上部的当前精灵与对话气泡初始化
        Debug.Assert(string.IsNullOrEmpty(_selectedActorName), "_selectedActorName should be empty at start");
        UpdateActorDisplay(_selectedActorName);    // 初始化角色显示为空

        // 中下部的UI状态初始化，带有滚动视图的主状态
        _mainState.SetActive(true);                  // 显示主状态UI

        // 刷新角色列表
        RefreshActorList();

        // 隐藏输入状态UI
        _inputState.SetActive(false);             // 隐藏输入状态UI
        _inputField.text = string.Empty;          // 清空输入字段
    }

    /// <summary>
    /// 当对象被销毁时调用
    /// 注销所有事件监听器,防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        // 注销角色点击事件监听器
        if (_onActorClickedEvent != null)
        {
            _onActorClickedEvent.UnregisterListener(this);
        }

        // 注销背景点击事件
        if (_background != null)
        {
            SpriteClickHandler backgroundClickHandler = _background.GetComponent<SpriteClickHandler>();
            if (backgroundClickHandler != null)
            {
                backgroundClickHandler.OnSpriteClicked -= OnBackgroundClicked;
            }
        }

        // 注销当前角色点击事件
        if (_currentActor != null)
        {
            SpriteClickHandler currentActorClickHandler = _currentActor.GetComponent<SpriteClickHandler>();
            if (currentActorClickHandler != null)
            {
                currentActorClickHandler.OnSpriteClicked -= OnCurrentActorClicked;
            }
        }
    }

    // UI按钮回调方法
    /// <summary>
    /// 运行按钮点击回调
    /// TODO: 实现游戏开始逻辑
    /// </summary>
    public void OnRunButtonClicked()
    {
        Debug.Log("Run button clicked in HomeScene.");
        StartCoroutine(AdvanceHomeState());
    }

    /// <summary>
    /// 返回按钮点击回调
    /// TODO: 实现返回上一场景逻辑
    /// </summary>
    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked in HomeScene.");
        StartCoroutine(ReturnToMainScene());
    }

    // IStringGameEventListener接口实现
    /// <summary>
    /// 字符串游戏事件回调方法
    /// 当角色被点击时触发,处理角色选择逻辑
    /// </summary>
    /// <param name="actorName">被点击的角色名称</param>
    public void OnEventRaised(string actorName)
    {
        Debug.Log($"[HomeScene] Actor clicked: {actorName}");

        // 防止重复选择同一角色
        if (_selectedActorName == actorName)
        {
            Debug.Log($"[HomeScene] Actor {actorName} is already selected.");
            return;
        }

        // 更新选中的角色名称
        _selectedActorName = actorName;
        Debug.Log($"[HomeScene] Selected actor updated to: {_selectedActorName}");

        // 更新UI显示选中的角色
        UpdateActorDisplay(actorName);
    }

    // 私有辅助方法
    /// <summary>
    /// 刷新场景中的角色列表
    /// 如果游戏已正规登录,则加载当前场景的角色列表并更新滚动视图
    /// 可以多次调用以更新角色列表显示
    /// </summary>
    private void RefreshActorList()
    {
        if (RootResp.Get() != null)
        {
            // 走到这里就是有正规登陆的，加载当前场景的角色列表
            var actorsInStage = GameContext.Instance.GetOtherActorsInCurrentStage();
            if (actorsInStage.Count > 0)
            {
                Debug.Log($"Actors in current stage: {string.Join(", ", actorsInStage)}");
                _scrollView.totalItemCount = actorsInStage.Count;
            }
            else
            {
                _scrollView.totalItemCount = 0;
                Debug.Log("No other actors found in current stage");
            }
        }
        else
        {
            Debug.Log("[HomeScene] GameContext Root is null");
            _scrollView.totalItemCount = 0;
        }
    }

    /// <summary>
    /// 更新角色选择后的UI显示
    /// 包括显示/隐藏角色精灵、对话气泡、更新角色图像,以及显示该角色最近一轮的事件历史
    /// </summary>
    /// <param name="selectedActorName">选中的角色名称,如果为空则隐藏所有角色相关UI</param>
    private void UpdateActorDisplay(string selectedActorName)
    {
        Debug.Log($"[HomeScene] Handling selection for actor: {selectedActorName}");

        if (string.IsNullOrEmpty(selectedActorName))
        {
            _currentActor.SetActive(false);  // 隐藏角色显示
            _speechBubble.SetActive(false);  // 隐藏对话气泡
            _speechBubbleText.text = string.Empty; // 更新提示文本
            return;
        }

        // 显示选中的角色和对话气泡
        _currentActor.SetActive(true); // 显示选中的角色
        _speechBubble.SetActive(true);  // 显示对话气泡
        _speechBubbleText.text = $"你选择了: {selectedActorName}"; // 更新提示文本

        // 更新当前角色的Sprite显示
        var actorSprite = TextureManager.Instance.GetSprite(selectedActorName);
        Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + selectedActorName);
        _currentActor.GetComponent<SpriteRenderer>().sprite = actorSprite;

        // 显示该角色的最近事件（如果有）
        var latestRoundEventsForActor = GameContext.Instance.GetLatestRoundEventsForActor(selectedActorName);
        if (latestRoundEventsForActor.Count > 0)
        {
            List<string> agentEventSummaries = new();
            foreach (var agentEvent in latestRoundEventsForActor)
            {
                Debug.Log($"[HomeScene] Last event for {selectedActorName}: {agentEvent.GetType().Name}");
                var summary = MyUtils.FormatAgentEventSummary(agentEvent);
                if (!string.IsNullOrEmpty(summary))
                {
                    agentEventSummaries.Add(summary);
                }
            }

            // 设置内容
            if (agentEventSummaries.Count > 0)
            {
                _speechBubbleText.text = string.Join("\n", agentEventSummaries);
            }
        }
    }

    /// <summary>
    /// 背景点击事件回调
    /// 当背景被点击时触发,返回主状态
    /// </summary>
    /// <param name="clickHandler">触发点击的SpriteClickHandler组件</param>
    private void OnBackgroundClicked(SpriteClickHandler clickHandler)
    {
        Debug.Log("背景被点击，返回主状态。");
        _mainState.SetActive(true);                  // 显示主状态UI
        _inputState.SetActive(false);                // 隐藏输入状态UI
    }

    /// <summary>
    /// 当前角色点击事件回调
    /// 当选中的角色精灵被点击时触发
    /// </summary>
    /// <param name="clickHandler">触发点击的SpriteClickHandler组件</param>
    private void OnCurrentActorClicked(SpriteClickHandler clickHandler)
    {
        Debug.Log($"精灵 {clickHandler.gameObject.name} 被点击了！");
        _mainState.SetActive(false);                // 隐藏主状态UI
        _inputState.SetActive(true);                // 显示输入状态UI
    }

    /// <summary>
    /// 返回主场景的协程
    /// 检查游戏是否已正确设置,如果是则加载MainScene2场景
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator ReturnToMainScene()
    {
        // 检查游戏是否已正确初始化
        if (RootResp.Get() != null)
        {
            yield return new WaitForSeconds(0);
            SceneManager.LoadScene(_preScene);
        }
        else
        {
            // 游戏未初始化,保持在当前场景
            Debug.LogWarning($"Game is not set up. Staying in {_homeSceneConfig.StageName}.");
        }
    }

    /// <summary>
    /// 推进家园场景状态的协程
    /// 调用服务器API推进场景中所有角色(包括NPC)的行动,并同步最新的游戏状态
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator AdvanceHomeState()
    {
        // 调用 HomeGameplay API 的 /advancing 端点,推进场景状态
        yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/advancing");

        // 检查API调用是否成功
        if (_homeGamePlayApi.RespData == null)
        {
            Debug.LogError("HomeGameplayApi /advancing request failed");
            yield break;
        }

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return GameStateSync.Instance.FetchSessionMessagesFromServer(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"Fetched {sessionMessages.Count} session messages from server after home advancing");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("Failed to fetch session messages after home advancing, skipping event processing");
            yield break;
        }

        // 检查是否有角色执行了场景切换事件，如果有就需要更新UI
        var actorsWithTransStageEvents = MyUtils.GetActorsWithTransStageEvents(GameContext.Instance.LastAgentEventsHistory);
        if (actorsWithTransStageEvents.Count > 0)
        {
            Debug.Log($"Actors with TransStageEvents: {string.Join(", ", actorsWithTransStageEvents)}");

            // 刷新游戏状态以确保数据同步
            yield return GameStateSync.Instance.RefreshMappingAndActorsFromServer();

            // 刷新角色列表
            RefreshActorList();

            // 如果当前选中的角色执行了场景转换,则清空选择
            if (actorsWithTransStageEvents.Contains(_selectedActorName))
            {
                _selectedActorName = string.Empty;
            }
        }
        else
        {
            Debug.Log("No actors executed TransStageEvents after advancing home state");
        }

        // 更新角色显示(可能已变化)
        UpdateActorDisplay(_selectedActorName);
    }

    /// <summary>
    /// InputField (TMP) - On Value Changed 事件处理器
    /// </summary>
    /// <param name="value">输入字段的当前值</param>
    public void OnInputFieldValueChanged(string value)
    {
        Debug.Log($"InputField value changed: {value}");
        Debug.Log("OnValueChanged: " + _inputField.text);
    }

    /// <summary>
    /// InputField (TMP) - On End Edit 事件处理器
    /// </summary>
    /// <param name="value">输入字段的最终值</param>
    public void OnInputFieldEndEdit(string value)
    {
        Debug.Log($"InputField end edit: {value}");
    }

    /// <summary>
    /// InputField (TMP) - On Select 事件处理器
    /// </summary>
    /// <param name="value">输入字段被选中时的值</param>
    public void OnInputFieldSelect(string value)
    {
        Debug.Log($"InputField selected: {value}");
    }

    /// <summary>
    /// InputField (TMP) - On Deselect 事件处理器
    /// </summary>
    /// <param name="value">输入字段被取消选中时的值</param>
    public void OnInputFieldDeselect(string value)
    {
        Debug.Log($"InputField deselected: {value}");
    }

    /// <summary>
    /// 发送消息按钮点击回调
    /// 验证游戏状态、角色选择和输入内容后,执行说话动作
    /// </summary>
    public void OnClickSendMessage()
    {
        Debug.Log("Send Message button clicked");
        if (RootResp.Get() != null && !string.IsNullOrEmpty(_selectedActorName) && !string.IsNullOrEmpty(_inputField.text))
        {
            StartCoroutine(ExecuteSpeakAction(_selectedActorName, _inputField.text));
        }
        else
        {
            Debug.LogWarning("Cannot send message. Ensure game is set up, a sprite is selected, and input field is not empty.");
        }
    }

    /// <summary>
    /// 执行说话动作的协程
    /// 调用服务器API发送消息到目标角色,并同步最新的游戏状态
    /// </summary>
    /// <param name="targetActorName">目标角色名称</param>
    /// <param name="messageContent">消息内容</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator ExecuteSpeakAction(string targetActorName, string messageContent)
    {
        yield return _homeGamePlayApi.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/speak", new Dictionary<string, string>
            {
                ["target"] = targetActorName,
                ["content"] = messageContent
            });

        Debug.Log("Speak action executed");
        if (_homeGamePlayApi.RespData == null)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }

        // 从服务器获取并同步最新的会话消息
        bool fetchSuccess = false;
        yield return GameStateSync.Instance.FetchSessionMessagesFromServer(
            (success, sessionMessages) =>
            {
                fetchSuccess = success;
                if (success)
                {
                    Debug.Log($"Fetched {sessionMessages.Count} session messages from server after home advancing");
                }
            }
        );

        // 检查消息获取是否成功
        if (!fetchSuccess)
        {
            Debug.LogError("Failed to fetch session messages after home advancing");
            yield break;
        }

        // 清空输入字段,返回主状态.
        _inputField.text = string.Empty;
        _mainState.SetActive(true);                  // 显示主状态UI
        _inputState.SetActive(false);                // 隐藏输入状态UI
    }
}
