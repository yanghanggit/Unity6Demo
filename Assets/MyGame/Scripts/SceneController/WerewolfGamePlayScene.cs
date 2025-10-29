using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WerewolfGamePlayScene : MonoBehaviour
{
    public TMP_Text _mainText;

    public TMP_Text _subText;

    public WerewolfGamePlayAction _werewolfGamePlayAction;

    public StagesStateAction _stagesStateAction;

    public WerewolfGameStateAction _werewolfGameStateAction;

    public ActorDetailsAction _actorDetailsAction;

    public SessionMessagesAction _sessionMessagesAction;

    // 角色按钮相关组件数组（在 Inspector 中配置，长度为6）
    public TMP_Text[] actorButtonTexts = new TMP_Text[6];
    public Button[] actorButtons = new Button[6];
    public Image[] actorButtonImages = new Image[6];

    // 下一阶段按钮的Text组件
    public TMP_Text _nextPhaseButtonText;

    // 角色选择 Dropdown 组件（在 Inspector 中配置）
    public TMP_Dropdown _WolfDropdown1;
    public TMP_Dropdown _WitchDropdown;
    public TMP_Dropdown _WolfDropdown2;
    public TMP_Dropdown _SeerDropdown;
    public TMP_Text _guessResultText;

    // 面具图片资源的字典（在 Inspector 中配置）
    [System.Serializable]
    public class MaskSpriteEntry
    {
        public string maskName;  // 面具名称，如 "处女面具"
        public Sprite maskImage;    // 对应的 Sprite 资源
    }
    public MaskSpriteEntry[] maskSprites;  // 面具图片数组

    // Loading 图像(带 Animator 组件的 GameObject)
    public GameObject _loadingImage;

    // 点击角色按钮时显示的图片
    public GameObject _actorDetailsBackgroundImage;

    public GameObject _actorDetailsImage;

    // 白天和夜晚背景图片
    public GameObject _dayBackgroundImage;
    public GameObject _nightBackgroundImage;

    // 新增的界面和按钮
    public GameObject _newPanel;  // 新的界面面板
    private bool _isKickOffComplete = false;
    private bool _isPanelVisible = false;  // 记录新界面的显示状态
    private List<string> _actorNames = new List<string>();

    // 游戏阶段状态枚举
    private enum GamePhase
    {
        NotStarted,      // 游戏未开始
        AfterKickOff,    // 完成开局
        AfterFirstTime,  // 完成第一次时间推进（开局后）
        AfterNight,      // 完成夜晚
        AfterTimeAfterNight,  // 完成夜晚后的时间推进
        AfterDay,        // 完成白天讨论
        DiscussionComplete,  // 白天讨论已完成，等待进入投票
        AfterVote,       // 完成投票
        AfterTimeAfterVote    // 完成投票后的时间推进（准备进入下一个夜晚）
    }

    private GamePhase _currentGamePhase = GamePhase.NotStarted;

    // 标记当前阶段是否成功完成
    private bool _currentPhaseCompleted = true;
    private string _lastErrorMessage = "";

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_werewolfGamePlayAction != null, "_werewolfGamePlayAction is null");
        Debug.Assert(_sessionMessagesAction != null, "_sessionMessagesAction is null");
        Debug.Assert(_loadingImage != null, "_loadingImage is null");

        // 初始状态：隐藏 loading，显示主文本
        SetLoadingState(false);

        // 初始化新界面：默认隐藏
        if (_newPanel != null)
        {
            _newPanel.SetActive(false);
            _isPanelVisible = false;
        }

        SetupButtonImages();
    }

    private void SetupButtonImages()
    {
        // 从 WerewolfGameContext 获取所有角色的 appearances 和原始名字
        List<string> appearances = WerewolfGameContext.Instance.GetAllActorAppearances();
        List<string> actorNames = WerewolfGameContext.Instance.GetAllActorNames();

        _actorNames.Clear();

        // 确保使用 appearances[i+1] 时不会越界（第0个为旁白）
        for (int i = 0; i < actorButtonImages.Length && i + 1 < appearances.Count; i++)
        {
            string maskName = ExtractMaskName(appearances[i + 1]);
            string actorName = (actorNames != null && actorNames.Count > i + 1) ? actorNames[i + 1] : maskName;
            _actorNames.Add(actorName);

            // 根据面具名设置按钮图像
            if (actorButtonImages[i] != null)
            {
                Sprite maskSprite = LoadMaskSprite(maskName);
                if (maskSprite != null)
                {
                    actorButtonImages[i].sprite = maskSprite;
                }
                else
                {
                    Debug.LogWarning($"Failed to load sprite for mask: {maskName}");
                }
            }
        }
    }

    /// <summary>
    /// 根据面具名加载对应的 Sprite 资源
    /// 从预配置的 maskSprites 数组中查找
    /// </summary>
    private Sprite LoadMaskSprite(string maskName)
    {
        if (maskSprites == null || maskSprites.Length == 0)
        {
            Debug.LogWarning("maskSprites array is empty. Please configure it in Inspector.");
            return null;
        }

        foreach (var entry in maskSprites)
        {
            if (entry.maskName == maskName && entry.maskImage != null)
            {
                return entry.maskImage;
            }
        }

        Debug.LogWarning($"Mask sprite not found in maskSprites array: {maskName}");
        return null;
    }

    // 新增：按钮回调（在 Inspector 中传入 0..5）
    public void OnClickActorButton(int buttonIndex)
    {
        if (!_isKickOffComplete)
        {
            Debug.LogWarning("Please complete kick off");
            _mainText.text = "请先完成游戏开局 (Kick Off) ";
            return;
        }

        if (buttonIndex < 0 || buttonIndex >= _actorNames.Count)
        {
            Debug.LogWarning($"Invalid button index: {buttonIndex}");
            return;
        }

        // 显示角色详情图片
        if (_actorDetailsBackgroundImage != null)
        {
            _actorDetailsBackgroundImage.SetActive(true);
        }

        // 将角色详情图片换成对应按钮的图片
        if (_actorDetailsImage != null && buttonIndex >= 0 && buttonIndex < actorButtonImages.Length)
        {
            Image detailsImage = _actorDetailsImage.GetComponent<Image>();
            if (detailsImage != null && actorButtonImages[buttonIndex] != null)
            {
                detailsImage.sprite = actorButtonImages[buttonIndex].sprite;
                Debug.Log($"Updated actor details image to match button {buttonIndex + 1}");
            }
            else
            {
                Debug.LogWarning($"Failed to update actor details image: detailsImage={detailsImage != null}, actorButtonImage={actorButtonImages[buttonIndex] != null}");
            }
        }

        string actorName = _actorNames[buttonIndex];

        // 显示当前阶段的消息
        string currentPhase = WerewolfGameContext.Instance.CurrentPhase;

        if (!string.IsNullOrEmpty(currentPhase))
        {
            Debug.Log($"Showing messages for actor: {actorName}, phase: {currentPhase}");
            ShowActorMessagesForPhase(actorName, currentPhase);
        }
        else
        {
            Debug.LogWarning("Current phase is not set");
            _subText.text = "当前阶段未设置";
        }
    }

    // 显示特定阶段的角色消息
    private void ShowActorMessagesForPhase(string actorName, string phase)
    {
        var messages = WerewolfGameContext.Instance.GetMessagesByActorAndPhase(actorName, phase);
        if (messages == null || messages.Count == 0)
        {
            _subText.text = $"{actorName} 在 {GetPhaseFriendlyName(phase)} 阶段没有消息";
            return;
        }

        List<string> displayMessages = new List<string>();
        displayMessages.Add($"=== {actorName} 的 {GetPhaseFriendlyName(phase)} 阶段消息 ===");

        AppendMessagesWithPrefix(displayMessages, messages);

        _subText.text = string.Join("\n", displayMessages);
    }

    // 显示新增的消息（从指定索引开始）
    private void ShowNewlyAddedMessages(int startIndex)
    {
        string currentPhase = WerewolfGameContext.Instance.CurrentPhase;
        List<string> displayMessages = new List<string>();
        displayMessages.Add($"=== {GetPhaseFriendlyName(currentPhase)} 阶段消息 ===\n");

        // 获取所有消息记录
        var allMessages = WerewolfGameContext.Instance.MessageRecords;

        // 按角色分组新增的消息
        Dictionary<string, List<WerewolfGameContext.MessageRecord>> newMessagesByActor =
            new Dictionary<string, List<WerewolfGameContext.MessageRecord>>();

        for (int i = startIndex; i < allMessages.Count; i++)
        {
            var msg = allMessages[i];
            if (!newMessagesByActor.ContainsKey(msg.Actor))
            {
                newMessagesByActor[msg.Actor] = new List<WerewolfGameContext.MessageRecord>();
            }
            newMessagesByActor[msg.Actor].Add(msg);
        }

        // 按角色顺序显示
        foreach (string actorName in _actorNames)
        {
            if (newMessagesByActor.ContainsKey(actorName) && newMessagesByActor[actorName].Count > 0)
            {
                displayMessages.Add($"--- {actorName} ---");
                AppendMessagesWithPrefix(displayMessages, newMessagesByActor[actorName]);
                displayMessages.Add("");
            }
        }

        _mainText.text = string.Join("\n", displayMessages);
    }

    // 添加带前缀的消息到列表
    private void AppendMessagesWithPrefix(List<string> displayMessages, List<WerewolfGameContext.MessageRecord> messages)
    {
        foreach (var msg in messages)
        {
            string prefix;
            switch (msg.MessageType)
            {
                case WerewolfGameContext.MessageRecordType.NightActionEvent:
                    prefix = "[夜晚行动]";
                    break;
                case WerewolfGameContext.MessageRecordType.Mind:
                    prefix = "[内心]";
                    break;
                case WerewolfGameContext.MessageRecordType.Discussion:
                    prefix = "[发言]";
                    break;
                default:
                    prefix = "[未知]";
                    break;
            }
            displayMessages.Add($"{prefix} {msg.Content}");
        }
    }

    // 将阶段标识转换为友好名称
    private string GetPhaseFriendlyName(string phase)
    {
        if (phase == "kickoff")
        {
            return "开局";
        }
        else if (phase.StartsWith("night_"))
        {
            string turnStr = phase.Substring(6); // 提取 turn number
            if (int.TryParse(turnStr, out int turn))
            {
                int nightNumber = (turn + 1) / 2;
                return $"第{nightNumber}个夜晚";
            }
        }
        else if (phase.StartsWith("day_"))
        {
            string turnStr = phase.Substring(4); // 提取 turn number
            if (int.TryParse(turnStr, out int turn))
            {
                int dayNumber = turn / 2;
                return $"第{dayNumber}个白天";
            }
        }

        return phase; // 如果无法解析，返回原始phase
    }

    private string ExtractMaskName(string appearance)
    {

        // 查找包含"面具"的词
        if (appearance.Contains("面具"))
        {
            // 提取"XX面具"格式的文本
            int maskIndex = appearance.IndexOf("面具");
            if (maskIndex >= 2)
            {
                // 提取面具前的2个字符 + "面具"
                return appearance.Substring(maskIndex - 2, 4);
            }
        }
        return appearance;
    }

    /// <summary>
    /// 更新按钮文字为真实身份
    /// </summary>
    private void UpdateButtonTextsWithRoles()
    {
        // 从索引1开始（跳过旁白），对应按钮索引0-5
        for (int i = 0; i < actorButtonTexts.Length; i++)
        {
            if (actorButtonTexts[i] != null)
            {
                int actorIndex = i + 1; // 角色实体索引（跳过索引0的旁白）
                string role = WerewolfGameContext.Instance.GetActorRole(actorIndex);
                actorButtonTexts[i].text = role;
                Debug.Log($"Updated button {i + 1} to role: {role} (actorIndex: {actorIndex})");
            }
        }
    }

    /// <summary>
    /// 初始化所有角色 Dropdown 组件，填充角色名选项（跳过旁白）
    /// </summary>
    private void InitializeActorDropdown()
    {
        // 获取所有角色名（跳过第一个，即旁白）
        List<string> actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        
        // 创建选项列表（从索引 1 开始，跳过旁白）
        List<string> dropdownOptions = new List<string>();
        for (int i = 1; i < actorNames.Count && i <= 6; i++)
        {
            dropdownOptions.Add(actorNames[i]);
        }

        // 初始化所有 Dropdown
        InitializeSingleDropdown(_WolfDropdown1, dropdownOptions, "WolfDropdown1");
        InitializeSingleDropdown(_WolfDropdown2, dropdownOptions, "WolfDropdown2");
        InitializeSingleDropdown(_WitchDropdown, dropdownOptions, "WitchDropdown");
        InitializeSingleDropdown(_SeerDropdown, dropdownOptions, "SeerDropdown");

        Debug.Log($"All Dropdowns initialized with {dropdownOptions.Count} actors: {string.Join(", ", dropdownOptions)}");
    }

    /// <summary>
    /// 初始化单个 Dropdown 组件
    /// </summary>
    private void InitializeSingleDropdown(TMP_Dropdown dropdown, List<string> options, string dropdownName)
    {
        if (dropdown == null)
        {
            Debug.LogWarning($"{dropdownName} is not assigned in Inspector");
            return;
        }

        // 清空现有选项
        dropdown.ClearOptions();

        // 添加选项到 Dropdown
        dropdown.AddOptions(options);

        // 设置默认选中第一个角色
        if (options.Count > 0)
        {
            dropdown.value = 0;
        }

        Debug.Log($"{dropdownName} initialized successfully");
    }

    /// <summary>
    /// 获取指定 Dropdown 选中的角色名
    /// </summary>
    private string GetSelectedActorFromDropdown(TMP_Dropdown dropdown, string dropdownName)
    {
        if (dropdown == null)
        {
            Debug.LogWarning($"{dropdownName} is not assigned");
            return string.Empty;
        }

        if (dropdown.options.Count == 0)
        {
            Debug.LogWarning($"{dropdownName} has no options");
            return string.Empty;
        }

        int selectedIndex = dropdown.value;
        if (selectedIndex >= 0 && selectedIndex < dropdown.options.Count)
        {
            return dropdown.options[selectedIndex].text;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取狼人1 Dropdown 选中的角色名
    /// </summary>
    public string GetSelectedWolf1()
    {
        return GetSelectedActorFromDropdown(_WolfDropdown1, "WolfDropdown1");
    }

    /// <summary>
    /// 获取狼人2 Dropdown 选中的角色名
    /// </summary>
    public string GetSelectedWolf2()
    {
        return GetSelectedActorFromDropdown(_WolfDropdown2, "WolfDropdown2");
    }

    /// <summary>
    /// 获取女巫 Dropdown 选中的角色名
    /// </summary>
    public string GetSelectedWitch()
    {
        return GetSelectedActorFromDropdown(_WitchDropdown, "WitchDropdown");
    }

    /// <summary>
    /// 获取预言家 Dropdown 选中的角色名
    /// </summary>
    public string GetSelectedSeer()
    {
        return GetSelectedActorFromDropdown(_SeerDropdown, "SeerDropdown");
    }


    /// <summary>
    /// 打印所有 Dropdown 的当前选择（用于调试）
    /// </summary>
    public void LogAllDropdownSelections()
    {
        Debug.Log("=== Current Dropdown Selections ===");
        Debug.Log($"Wolf1: {GetSelectedWolf1()}");
        Debug.Log($"Wolf2: {GetSelectedWolf2()}");
        Debug.Log($"Witch: {GetSelectedWitch()}");
        Debug.Log($"Seer: {GetSelectedSeer()}");
    }

    /// <summary>
    /// 点击检测结果按钮时调用：验证所有 Dropdown 中选择的角色身份是否正确
    /// </summary>
    public void OnClickGuessResult()
    {
        if (!_isKickOffComplete)
        {
            Debug.LogWarning("Please complete kick off before guessing");
            if (_guessResultText != null)
            {
                _guessResultText.text = "请先完成游戏开局 (Kick Off)";
            }
            return;
        }

        // 获取所有角色名
        List<string> actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        
        // 验证结果列表
        List<string> results = new List<string>();
        
        // 检测狼人1
        string wolf1Name = GetSelectedWolf1();
        bool wolf1Correct = CheckRoleGuess(wolf1Name, "狼人", actorNames);
        results.Add($"狼人1: {(wolf1Correct ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {wolf1Name})");
        
        // 检测狼人2
        string wolf2Name = GetSelectedWolf2();
        bool wolf2Correct = CheckRoleGuess(wolf2Name, "狼人", actorNames);
        results.Add($"狼人2: {(wolf2Correct ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {wolf2Name})");
        
        // 检测女巫
        string witchName = GetSelectedWitch();
        bool witchCorrect = CheckRoleGuess(witchName, "女巫", actorNames);
        results.Add($"女巫: {(witchCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {witchName})");
        
        // 检测预言家
        string seerName = GetSelectedSeer();
        bool seerCorrect = CheckRoleGuess(seerName, "预言家", actorNames);
        results.Add($"预言家: {(seerCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {seerName})");
        
        // 计算总体正确率
        int correctCount = (wolf1Correct ? 1 : 0) + (wolf2Correct ? 1 : 0) + 
                          (witchCorrect ? 1 : 0) + (seerCorrect ? 1 : 0);
        
        // 显示结果
        string resultText = "=== 身份猜测结果 ===\n" + 
                           string.Join("\n", results) + 
                           $"\n\n总计: {correctCount}/4 正确";
        
        if (_guessResultText != null)
        {
            _guessResultText.text = resultText;
        }
        
        Debug.Log(resultText);
    }

    /// <summary>
    /// 检查角色名对应的身份是否匹配预期角色
    /// </summary>
    /// <param name="actorName">被选择的角色名</param>
    /// <param name="expectedRole">预期的角色（如"狼人"、"女巫"、"预言家"）</param>
    /// <param name="actorNames">所有角色名列表</param>
    /// <returns>是否匹配</returns>
    private bool CheckRoleGuess(string actorName, string expectedRole, List<string> actorNames)
    {
        if (string.IsNullOrEmpty(actorName))
        {
            Debug.LogWarning("Actor name is empty");
            return false;
        }

        // 在角色列表中找到对应的索引
        int actorIndex = actorNames.IndexOf(actorName);
        if (actorIndex < 0)
        {
            Debug.LogWarning($"Actor not found: {actorName}");
            return false;
        }

        // 获取该角色的真实身份
        string actualRole = WerewolfGameContext.Instance.GetActorRole(actorIndex);
        
        Debug.Log($"Checking {actorName} (index {actorIndex}): Expected={expectedRole}, Actual={actualRole}");
        
        // 比较身份
        return actualRole == expectedRole;
    }

    /// <summary>
    /// Dropdown 值改变时的回调（可以在 Inspector 中绑定）
    /// 通用回调，可以用于任何一个 Dropdown
    /// </summary>
    public void OnDropdownValueChanged(int dropdownIndex)
    {
        string selectedActor = string.Empty;
        string dropdownName = string.Empty;

        switch (dropdownIndex)
        {
            case 0: // Wolf1
                selectedActor = GetSelectedWolf1();
                dropdownName = "狼人1";
                break;
            case 1: // Wolf2
                selectedActor = GetSelectedWolf2();
                dropdownName = "狼人2";
                break;
            case 2: // Witch
                selectedActor = GetSelectedWitch();
                dropdownName = "女巫";
                break;
            case 3: // Seer
                selectedActor = GetSelectedSeer();
                dropdownName = "预言家";
                break;
        }

        Debug.Log($"{dropdownName} Dropdown selection changed to: {selectedActor}");
        
        // 可以在这里添加其他逻辑，例如自动显示该角色的信息
        // if (!string.IsNullOrEmpty(selectedActor))
        // {
        //     ShowActorMessagesForPhase(selectedActor, WerewolfGameContext.Instance.CurrentPhase);
        // }
    }

    /// <summary>
    /// 隐藏角色详情面板
    /// </summary>
    private void HideActorDetailsPanel()
    {
        if (_actorDetailsBackgroundImage != null)
        {
            _actorDetailsBackgroundImage.SetActive(false);
        }
    }

    /// <summary>
    /// 点击关闭按钮时隐藏角色详情面板
    public void OnClickCloseActorDetails()
    {
        Debug.Log("OnClickCloseActorDetails - Hiding actor details panel");
        HideActorDetailsPanel();
    }

    /// <summary>
    /// 切换新界面的显示/隐藏状态
    /// </summary>
    public void OnClickTogglePanel()
    {
        _isPanelVisible = !_isPanelVisible;
        
        if (_newPanel != null)
        {
            _newPanel.SetActive(_isPanelVisible);
            Debug.Log($"Toggle panel - New state: {(_isPanelVisible ? "Visible" : "Hidden")}");
        }
        else
        {
            Debug.LogWarning("_newPanel is not assigned in Inspector");
        }
    }

    /// <summary>
    /// 设置 Loading 状态
    /// </summary>
    /// <param name="isLoading">true=显示loading并隐藏文本, false=隐藏loading并显示文本</param>
    private void SetLoadingState(bool isLoading)
    {
        if (_loadingImage != null)
        {
            _loadingImage.SetActive(isLoading);
        }

        if (_mainText != null)
        {
            _mainText.gameObject.SetActive(!isLoading);
        }
    }

    /// <summary>
    /// 根据消息内容切换背景图片
    /// </summary>
    /// <param name="messages">会话消息列表</param>
    private void SwitchBackgroundByMessages(List<SessionMessage> messages)
    {
        if (_dayBackgroundImage == null || _nightBackgroundImage == null)
        {
            Debug.LogWarning("Background images not assigned");
            return;
        }

        bool hasDay = false;
        bool hasNight = false;

        // 将消息转换为文本并检测是否包含"白天"或"夜晚"
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(messages);

        foreach (var messageText in processedMessages)
        {
            if (messageText.Contains("白天"))
            {
                hasDay = true;
            }
            if (messageText.Contains("夜晚"))
            {
                hasNight = true;
            }
        }

        // 根据检测结果切换背景
        if (hasDay)
        {
            _dayBackgroundImage.SetActive(true);
            _nightBackgroundImage.SetActive(false);
            Debug.Log("切换到白天背景");
        }
        else if (hasNight)
        {
            _dayBackgroundImage.SetActive(false);
            _nightBackgroundImage.SetActive(true);
            Debug.Log("切换到夜晚背景");
        }
        // 如果两个都没有,保持不变(不做任何操作)
    }

    private IEnumerator CheckActorDeathStatus()
    {
        Debug.Log("=== 检测角色死亡状态 ===");

        // 获取最新的角色实体数据
        yield return _actorDetailsAction.Call(
            WerewolfGameContext.Instance.ActorDetailsUrl,
            WerewolfGameContext.Instance.GetAllActorNames()
        );

        if (_actorDetailsAction.ResponseData == null)
        {
            Debug.LogError("ActorDetailsAction ResponseData is null");
            yield break;
        }

        // 更新角色实体数据
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.ResponseData.actor_entities_serialization
        );

        // 检测每个角色的死亡状态（从索引1开始，跳过旁白）
        var actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        for (int i = 1; i < actorNames.Count; i++)
        {
            bool hasDeath = WerewolfGameContext.Instance.HasDeathComponent(i);
            Debug.Log($"{actorNames[i]}: {(hasDeath ? "已死亡 ☠" : "存活 ✓")}");

            // 更新对应按钮的状态（i-1 是因为按钮索引从0开始，而角色从1开始）
            if (hasDeath && i - 1 < 6)
            {
                UpdateButtonState(i - 1, false);
            }
        }
    }

    /// <summary>
    /// 更新按钮状态：禁用并变灰
    /// </summary>
    /// <param name="buttonIndex">按钮索引 (0-5)</param>
    /// <param name="isAlive">是否存活</param>
    private void UpdateButtonState(int buttonIndex, bool isAlive)
    {
        if (buttonIndex < 0 || buttonIndex >= actorButtons.Length || actorButtons[buttonIndex] == null)
        {
            Debug.LogWarning($"Invalid button at index: {buttonIndex}");
            return;
        }

        Button button = actorButtons[buttonIndex];
        button.interactable = isAlive;

        // 修改按钮颜色
        ColorBlock colors = button.colors;
        Color targetColor = isAlive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.normalColor = targetColor;
        colors.highlightedColor = isAlive ? new Color(0.9f, 0.9f, 0.9f, 1f) : targetColor;
        colors.pressedColor = isAlive ? new Color(0.8f, 0.8f, 0.8f, 1f) : targetColor;
        colors.disabledColor = targetColor;
        button.colors = colors;

        Debug.Log($"Button {buttonIndex + 1}: {(isAlive ? "Active" : "Dead (Disabled)")}");
    }

    public void OnClickKickOff()
    {
        Debug.Log("OnClickKickOff");
        StartCoroutine(KickOff());
    }

    public void OnClickTime()
    {
        Debug.Log("OnClickTime");
        StartCoroutine(Time());
    }

    public void OnClickNight()
    {
        Debug.Log("OnClickNight");
        StartCoroutine(Night());
    }

    public void OnClickDay()
    {
        Debug.Log("OnClickDay");
        StartCoroutine(Day());
    }

    public void OnClickVote()
    {
        Debug.Log("OnClickVote");
        StartCoroutine(Vote());
    }


    private IEnumerator KickOff()
    {
        // 隐藏角色详情图片
        HideActorDetailsPanel();

        // 显示 loading 动画，隐藏文本
        SetLoadingState(true);

        // 发送请求
        yield return _werewolfGamePlayAction.Call(WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/kickoff" } });

        if (_werewolfGamePlayAction.ResponseData == null)
        {
            Debug.LogError("WerewolfGamePlayAction ResponseData is null");
            // 出错时也要隐藏 loading
            SetLoadingState(false);
            yield break;
        }

        // 获取会话消息
        _sessionMessagesAction.Setup(
            WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            Debug.LogError("SessionMessagesAction ResponseData is null");
            // 出错时也要隐藏 loading
            SetLoadingState(false);
            _mainText.text = "开局失败：获取消息错误";
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 处理消息（记录已在 GameContext 中做），但 UI 显示为"开局已完成"
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "kickoff");
        Debug.Log("Kickoff processed messages:\n" + string.Join("\n", processedMessages));

        _isKickOffComplete = true;
        SetupButtonImages(); // 更新按钮绑定的 actor 名称
        
        // 初始化 Dropdown 选项
        InitializeActorDropdown();

        // 隐藏 loading，显示完成文本
        SetLoadingState(false);
        _mainText.text = "开局已完成\n点击角色按钮查看对应消息";
        // UpdateButtonTextsWithRoles(); // 显示真实身份

    }

    private IEnumerator Time()
    {
        // 隐藏角色详情图片
        HideActorDetailsPanel();

        //
        yield return _werewolfGamePlayAction.Call(WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/time" } });

        if (_werewolfGamePlayAction.ResponseData == null)
        {
            Debug.LogError("WerewolfGamePlayAction ResponseData is null");
            yield break;
        }

        // 获取会话消息
        _sessionMessagesAction.Setup(
            WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 根据消息内容切换背景图片
        SwitchBackgroundByMessages(_sessionMessagesAction.ResponseData.session_messages);

        // 获取并更新游戏状态（包括胜利条件）
        yield return UpdateGameStateInTime();

        // 检测并显示胜利条件
        string victoryCondition = WerewolfGameContext.Instance.VictoryCondition;
        Debug.Log($"=== Victory Condition Check ===");
        Debug.Log($"胜利情况: {(string.IsNullOrEmpty(victoryCondition) ? "None" : victoryCondition)}");

        StartCoroutine(CheckActorDeathStatus());

        // 根据胜利条件显示结果
        if (victoryCondition == "TOWN_VICTORY")
        {
            UpdateButtonTextsWithRoles(); // 显示真实身份
            _mainText.text = "村民胜利！\n游戏将在10秒后重新开始...";
            yield return new WaitForSeconds(10f);
            RestartGame();
        }
        else if (victoryCondition == "WEREWOLVES_VICTORY")
        {
            UpdateButtonTextsWithRoles(); // 显示真实身份
            _mainText.text = "狼人胜利！\n游戏将在10秒后重新开始...";
            yield return new WaitForSeconds(10f);
            RestartGame();
        }
        else
        {
            // 没有胜利条件时显示正常消息
            UpdateMainTextByClientMessages(_sessionMessagesAction.ResponseData.session_messages);
        }
    }

    private IEnumerator Night()
    {
        // 检查是否已完成 KickOff
        if (!_isKickOffComplete)
        {
            Debug.LogWarning("Must complete kick off before night!");
            _mainText.text = "必须先完成游戏开局 (Kick Off) 才能进入夜晚";
            yield break;
        }

        // 隐藏角色详情图片
        HideActorDetailsPanel();

        // 显示 loading 动画，隐藏文本
        SetLoadingState(true);
        // 
        yield return _werewolfGamePlayAction.Call(WerewolfGameContext.Instance.GameplayUrl,
           WerewolfGameContext.Instance.UserName,
           WerewolfGameContext.Instance.GameName,
           new Dictionary<string, string>
           { { "user_input", "/night" } });

        if (_werewolfGamePlayAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("WerewolfGamePlayAction ResponseData is null");
            yield break;
        }

        // 获取会话消息
        _sessionMessagesAction.Setup(
            WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 处理消息
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "night");
        Debug.Log("Night processed messages:\n" + string.Join("\n", processedMessages));

        // 隐藏 loading，显示完成文本
        SetLoadingState(false);
        _mainText.text = "夜晚行动已完成\n点击角色按钮查看夜晚行动细节";
    }

    private IEnumerator Day()
    {
        // 隐藏角色详情图片
        HideActorDetailsPanel();

        // 显示 loading 动画，隐藏文本
        SetLoadingState(true);

        // 先检查讨论完成状态
        yield return UpdateGameStateInTime();

        bool isDiscussionComplete = WerewolfGameContext.Instance.IsDiscussionComplete;
        Debug.Log($"Discussion Complete Status: {isDiscussionComplete}");

        // 如果讨论已完成，直接显示提示信息，不再调用 /day
        if (isDiscussionComplete)
        {
            SetLoadingState(false);
            _mainText.text = "讨论已完成";
            yield break;
        }

        // 讨论未完成，继续执行 /day 命令
        yield return _werewolfGamePlayAction.Call(WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/day" } });

        if (_werewolfGamePlayAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("WerewolfGamePlayAction ResponseData is null");
            yield break;
        }

        // 获取会话消息
        _sessionMessagesAction.Setup(
            WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 记录处理前的消息数量
        int messageCountBefore = WerewolfGameContext.Instance.MessageRecords.Count;

        // 处理消息以更新阶段信息（会添加到记录中）
        WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "day");

        SetLoadingState(false);

        // 显示本次新增的消息
        ShowNewlyAddedMessages(messageCountBefore);
    }

    private IEnumerator Vote()
    {
        // 隐藏角色详情图片
        HideActorDetailsPanel();

        // 显示 loading 动画，隐藏文本
        SetLoadingState(true);
        // 
        yield return _werewolfGamePlayAction.Call(WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/vote" } });

        if (_werewolfGamePlayAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("WerewolfGamePlayAction ResponseData is null");
            yield break;
        }

        // 获取会话消息
        _sessionMessagesAction.Setup(
            WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId
        );

        yield return _sessionMessagesAction.Call();
        if (_sessionMessagesAction.ResponseData == null)
        {
            SetLoadingState(false);
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 处理所有消息（包括角色的内心想法等），记录到 GameContext 中
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "vote");
        Debug.Log("Vote processed messages:\n" + string.Join("\n", processedMessages));

        SetLoadingState(false);

        // 只显示投票结果（EventHead.NONE 的消息）
        ShowVoteResultOnly(_sessionMessagesAction.ResponseData.session_messages);
    }

    /// <summary>
    /// 只显示投票结果（EventHead.NONE 类型的消息，即游戏事件如"谁被投票出局"）
    /// 使用 WerewolfGameContext 的统一消息处理逻辑
    /// </summary>
    private void ShowVoteResultOnly(List<SessionMessage> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            _mainText.text = "没有投票结果";
            return;
        }

        // 筛选出 EventHead.NONE 类型的消息（游戏事件）
        List<SessionMessage> voteEventMessages = new List<SessionMessage>();

        foreach (var message in messages)
        {
            // 只处理 AGENT_EVENT 类型的消息
            if (message.message_type == (int)MessageType.AGENT_EVENT && message.data != null)
            {
                // 检查是否为 EventHead.NONE（游戏事件消息）
                if (message.data.ContainsKey("head"))
                {
                    object headObj = message.data["head"];
                    
                    // 转换 head 值并检查是否为 EventHead.NONE
                    int headValue = headObj is int intHead ? intHead : 
                                   headObj is long longHead ? (int)longHead : 
                                   int.TryParse(headObj?.ToString(), out int parsedHead) ? parsedHead : -1;
                    
                    if (headValue == (int)EventHead.NONE)
                    {
                        voteEventMessages.Add(message);
                    }
                }
            }
        }

        // 使用统一的消息处理方法显示结果
        if (voteEventMessages.Count > 0)
        {
            var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(voteEventMessages);
            _mainText.text = "投票阶段结果：\n" + string.Join("\n", processedMessages) + "\n\n点击角色按钮查看详细消息";
        }
        else
        {
            _mainText.text = "投票完成，但没有找到出局结果";
        }
    }

    private void UpdateMainTextByClientMessages(List<SessionMessage> messages)
    {
        _mainText.text = "";
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            Debug.Log($"Client Message {i}: " + JsonUtility.ToJson(message));
        }

        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(messages);
        _mainText.text = string.Join("\n", processedMessages);
    }

    private void UpdateLastSequenceIdFromResponse()
    {
        if (_sessionMessagesAction.ResponseData == null)
        {
            Debug.LogWarning("SessionMessagesAction ResponseData is null");
            Debug.Assert(false, "SessionMessagesAction ResponseData is null");
            return;
        }

        if (_sessionMessagesAction.ResponseLastSequenceId < 0)
        {
            Debug.LogWarning("Invalid last sequence ID");
            return;
        }

        WerewolfGameContext.Instance.LastSequenceId = _sessionMessagesAction.ResponseLastSequenceId;
    }

    private IEnumerator UpdateGameStateInTime()
    {
        // 调用 WerewolfGameStateAction 获取游戏状态（包含 victory_condition）
        yield return _werewolfGameStateAction.Call(
            WerewolfGameContext.Instance.StateUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName
        );

        if (_werewolfGameStateAction.ResponseData == null)
        {
            Debug.LogError("WerewolfGameStateAction ResponseData is null");
            yield break;
        }

        // 只更新 victory_condition
        WerewolfGameContext.Instance.UpdateGameState(
            _werewolfGameStateAction.ResponseData.game_time,
            new List<string>(), // 不需要更新角色列表
            "", // 不需要更新场景名
            _werewolfGameStateAction.ResponseData.victory_condition,
            _werewolfGameStateAction.ResponseData.is_discussion_complete
        );

        Debug.Log($"Victory Condition updated: {_werewolfGameStateAction.ResponseData.victory_condition}");
    }

    /// <summary>
    /// 重新开始游戏：重置所有数据并返回到 Launch Scene
    /// </summary>
    private void RestartGame()
    {
        Debug.Log("Restarting game...");

        // 清空所有 Dropdown
        ClearAllDropdowns();

        // 重置游戏上下文中的所有数据
        WerewolfGameContext.Instance.Reset();

        // 加载 Launch Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("WerewolfGameLaunchScene");
    }

    /// <summary>
    /// 清空所有 Dropdown 的选项
    /// </summary>
    private void ClearAllDropdowns()
    {
        if (_WolfDropdown1 != null)
        {
            _WolfDropdown1.ClearOptions();
        }
        
        if (_WolfDropdown2 != null)
        {
            _WolfDropdown2.ClearOptions();
        }
        
        if (_WitchDropdown != null)
        {
            _WitchDropdown.ClearOptions();
        }
        
        if (_SeerDropdown != null)
        {
            _SeerDropdown.ClearOptions();
        }
        
        Debug.Log("All dropdowns cleared");
    }

    /// <summary>
    /// 下一阶段按钮：根据当前游戏阶段执行相应的操作
    /// </summary>
    public void OnClickNextPhase()
    {
        Debug.Log($"OnClickNextPhase - Current Phase: {_currentGamePhase}");

        // 将按钮文字更改为"继续"
        if (_nextPhaseButtonText != null)
        {
            _nextPhaseButtonText.text = "继续";
        }

        StartCoroutine(ExecuteNextPhase());
    }

    /// <summary>
    /// 执行下一个游戏阶段
    /// </summary>
    private IEnumerator ExecuteNextPhase()
    {
        // 检查上一阶段是否成功完成（但允许重试同一阶段）
        if (!_currentPhaseCompleted && !string.IsNullOrEmpty(_lastErrorMessage))
        {
            // 显示上次错误信息，但允许重试
            Debug.Log($"Retrying phase after previous failure: {_lastErrorMessage}");
            _mainText.text = $"重试上一阶段\n上次失败原因：{_lastErrorMessage}";

            // 清除错误，准备重试
            _lastErrorMessage = "";
        }

        // 重置完成标记（每次都重置，允许重试）
        _currentPhaseCompleted = false;
        _lastErrorMessage = "";

        switch (_currentGamePhase)
        {
            case GamePhase.NotStarted:
                // 1. 执行 KickOff 开局
                Debug.Log("Next Phase: Executing KickOff...");
                yield return KickOff();

                // 检查 KickOff 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "开局失败";
                    _mainText.text = "开局失败，请重试";
                    yield break;
                }

                _currentPhaseCompleted = true;
                _currentGamePhase = GamePhase.AfterKickOff;
                break;

            case GamePhase.AfterKickOff:
                // 2. 执行第一次 Time 推进时间
                Debug.Log("Next Phase: Executing Time (after KickOff)...");
                yield return Time();

                // 检查 Time 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "时间推进失败";
                    _mainText.text = "时间推进失败，请重试";
                    yield break;
                }

                // 检查游戏是否结束
                yield return UpdateGameStateInTime();
                string victoryAfterKickOff = WerewolfGameContext.Instance.VictoryCondition;

                if (victoryAfterKickOff == "TOWN_VICTORY" || victoryAfterKickOff == "WEREWOLVES_VICTORY")
                {
                    Debug.Log($"Game ended after first Time with condition: {victoryAfterKickOff}");
                    _currentPhaseCompleted = true;
                    _currentGamePhase = GamePhase.NotStarted;
                }
                else
                {
                    Debug.Log($"Game continues, victory condition: {(string.IsNullOrEmpty(victoryAfterKickOff) ? "NONE" : victoryAfterKickOff)}");
                    _currentPhaseCompleted = true;
                    _currentGamePhase = GamePhase.AfterFirstTime;
                }
                break;

            case GamePhase.AfterFirstTime:
            case GamePhase.AfterTimeAfterVote:
                // 3. 执行 Night 夜晚阶段
                Debug.Log("Next Phase: Executing Night...");
                yield return Night();

                // 检查 Night 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "夜晚阶段失败";
                    _mainText.text = "夜晚阶段失败，请重试";
                    yield break;
                }

                _currentPhaseCompleted = true;
                _currentGamePhase = GamePhase.AfterNight;
                break;

            case GamePhase.AfterNight:
                // 4. 执行 Time 推进时间（夜晚后）
                Debug.Log("Next Phase: Executing Time (after Night)...");
                yield return Time();

                // 检查 Time 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "时间推进失败（夜晚后）";
                    _mainText.text = "时间推进失败，请重试";
                    yield break;
                }

                // 检查游戏是否结束
                yield return UpdateGameStateInTime();
                string victoryAfterNight = WerewolfGameContext.Instance.VictoryCondition;

                if (victoryAfterNight == "TOWN_VICTORY" || victoryAfterNight == "WEREWOLVES_VICTORY")
                {
                    Debug.Log($"Game ended after Night Time with condition: {victoryAfterNight}");
                    _currentPhaseCompleted = true;
                    _currentGamePhase = GamePhase.NotStarted;
                }
                else
                {
                    Debug.Log($"Game continues, victory condition: {(string.IsNullOrEmpty(victoryAfterNight) ? "NONE" : victoryAfterNight)}");
                    _currentPhaseCompleted = true;
                    _currentGamePhase = GamePhase.AfterTimeAfterNight;
                }
                break;

            case GamePhase.AfterTimeAfterNight:
                // 5. 执行 Day 白天讨论（可能需要多次）
                Debug.Log("Next Phase: Executing Day...");
                yield return Day();

                // 检查 Day 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "白天讨论失败";
                    _mainText.text = "白天讨论失败，请重试";
                    yield break;
                }

                // 检查讨论是否完成
                yield return UpdateGameStateInTime();
                bool isDiscussionComplete = WerewolfGameContext.Instance.IsDiscussionComplete;

                _currentPhaseCompleted = true;

                if (isDiscussionComplete)
                {
                    // 讨论完成，显示提示并等待玩家再次点击
                    _currentGamePhase = GamePhase.DiscussionComplete;
                    // _mainText.text = "白天讨论已完成\n点击进入投票阶段";
                    Debug.Log("Discussion complete, waiting for player to proceed to vote");
                }
                else
                {
                    // 讨论未完成，保持在当前阶段，等待再次点击继续 Day
                    _currentGamePhase = GamePhase.AfterTimeAfterNight;
                }
                break;

            case GamePhase.DiscussionComplete:
                // 6. 讨论完成后，玩家点击进入投票阶段
                Debug.Log("Player confirmed discussion complete, proceeding to Vote...");

                // 隐藏角色详情图片
                HideActorDetailsPanel();

                _currentGamePhase = GamePhase.AfterDay;
                _mainText.text = "白天讨论已完成\n准备进入投票阶段...";
                _currentPhaseCompleted = true;
                break;

            case GamePhase.AfterDay:
                // 7. 执行 Vote 投票阶段
                Debug.Log("Next Phase: Executing Vote...");
                yield return Vote();

                // 检查 Vote 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "投票阶段失败";
                    _mainText.text = "投票阶段失败，请重试";
                    yield break;
                }

                _currentPhaseCompleted = true;
                _currentGamePhase = GamePhase.AfterVote;
                break;

            case GamePhase.AfterVote:
                // 7. 执行 Time 推进时间（投票后）
                Debug.Log("Next Phase: Executing Time (after Vote)...");
                yield return Time();

                // 检查 Time 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
                {
                    _lastErrorMessage = "时间推进失败（投票后）";
                    _mainText.text = "时间推进失败，请重试";
                    yield break;
                }

                // 检查游戏是否结束
                yield return UpdateGameStateInTime();
                string victoryAfterVote = WerewolfGameContext.Instance.VictoryCondition;

                if (victoryAfterVote == "TOWN_VICTORY" || victoryAfterVote == "WEREWOLVES_VICTORY")
                {
                    Debug.Log($"Game ended with victory condition: {victoryAfterVote}");
                    _currentPhaseCompleted = true;
                    _currentGamePhase = GamePhase.NotStarted;
                }
                else
                {
                    Debug.Log($"Game continues, victory condition: {(string.IsNullOrEmpty(victoryAfterVote) ? "NONE" : victoryAfterVote)}");
                    _currentPhaseCompleted = true;
                    // 进入下一个循环：回到夜晚阶段
                    _currentGamePhase = GamePhase.AfterTimeAfterVote;
                }
                break;
        }

        Debug.Log($"Next Phase Complete - New Phase: {_currentGamePhase}, Completed: {_currentPhaseCompleted}");

        // 重置按钮文字为当前阶段的提示文本
        ResetNextPhaseButtonText();
    }

    /// <summary>
    /// 获取当前阶段的提示文本（可选，用于UI显示）
    /// </summary>
    public string GetCurrentPhaseHint()
    {
        switch (_currentGamePhase)
        {
            case GamePhase.NotStarted:
                return "点击开始游戏 (KickOff)";
            case GamePhase.AfterKickOff:
                return "点击推进时间 (Time)";
            case GamePhase.AfterFirstTime:
                return "点击进入夜晚 (Night)";
            case GamePhase.AfterNight:
                return "点击推进时间 (Time)";
            case GamePhase.AfterTimeAfterNight:
                return "点击开始白天讨论 (Day)";
            case GamePhase.DiscussionComplete:
                return "讨论已完成，点击进入投票 (Vote)";
            case GamePhase.AfterDay:
                return "点击开始投票 (Vote)";
            case GamePhase.AfterVote:
                return "点击推进时间 (Time)";
            case GamePhase.AfterTimeAfterVote:
                return "点击进入下一个夜晚 (Night)";
            default:
                return "未知阶段";
        }
    }

    /// <summary>
    /// 重置下一阶段按钮的文字为当前阶段的提示文本
    /// </summary>
    public void ResetNextPhaseButtonText()
    {
        if (_nextPhaseButtonText != null)
        {
            _nextPhaseButtonText.text = GetCurrentPhaseHint();
        }
    }
}
