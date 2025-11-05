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

    // 角色猜测按钮（替换原来的 Dropdown 组件）
    public Button _wolf1GuessButton;
    public Button _wolf2GuessButton;
    public Button _witchGuessButton;
    public Button _seerGuessButton;
    
    // 猜测按钮的文本组件
    public TMP_Text _wolf1GuessButtonText;
    public TMP_Text _wolf2GuessButtonText;
    public TMP_Text _witchGuessButtonText;
    public TMP_Text _seerGuessButtonText;
    
    // 角色选择界面
    public GameObject _roleSelectionPanel;  // 角色选择面板
    public Button[] _roleSelectionButtons = new Button[6];  // 6个角色选择按钮
    public Button _roleSelectionReturnButton;  // 返回按钮
    
    // 猜测结果文本
    public TMP_Text _guessResultText;
    
    // 角色选择状态
    private int _currentSelectingGuessButtonIndex = -1;  // 当前正在选择的猜测按钮索引 (0=wolf1, 1=wolf2, 2=witch, 3=seer)
    private string[] _selectedActorNames = new string[4];  // 存储每个猜测按钮选择的角色名
    
    // 消息过滤状态（从 WerewolfGameContext 读取，不再需要按钮切换）
    private bool _showAllMessages = false;  // false=只显示发言，true=显示所有消息（包括内心和夜晚行动）

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

    // 游戏介绍界面相关组件
    public GameObject _gameIntroPanel;  // 游戏介绍面板
    public TMP_Text _introStatusText;   // 介绍界面状态文本
    public Button _introCloseButton;    // 介绍界面关闭按钮

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

    // 记录夜晚前的死亡状态，用于检测夜晚期间的新死亡
    private Dictionary<string, bool> _deathStatusBeforeNight = new Dictionary<string, bool>();

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

        // 初始化角色选择界面：默认隐藏
        if (_roleSelectionPanel != null)
        {
            _roleSelectionPanel.SetActive(false);
        }

        // 初始化选择状态
        InitializeGuessButtonTexts();

        // 从 Context 读取游戏模式，设置消息过滤状态
        _showAllMessages = WerewolfGameContext.Instance.IsDebugMode;
        Debug.Log($"Game mode: {(_showAllMessages ? "Debug Mode (显示所有消息)" : "Play Mode (只显示发言)")}");

        SetupButtonImages();

        // 初始化游戏介绍界面
        InitializeGameIntroPanel();
    }

    /// <summary>
    /// 初始化游戏介绍界面
    /// </summary>
    private void InitializeGameIntroPanel()
    {
        if (_gameIntroPanel == null)
        {
            Debug.LogWarning("_gameIntroPanel is not assigned");
            return;
        }

        // 显示介绍界面
        _gameIntroPanel.SetActive(true);

        // 设置初始文本
        if (_introStatusText != null)
        {
            _introStatusText.text = "玩家准备中......";
        }

        // 隐藏关闭按钮
        if (_introCloseButton != null)
        {
            _introCloseButton.gameObject.SetActive(false);
        }

        // 自动开始 Kick Off
        StartCoroutine(AutoKickOff());
    }

    /// <summary>
    /// 自动执行 Kick Off
    /// </summary>
    private IEnumerator AutoKickOff()
    {
        Debug.Log("Auto KickOff started");

        // 执行 KickOff 逻辑
        yield return KickOff();

        // 检查 KickOff 是否成功
        if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.ResponseData == null)
        {
            // KickOff 失败
            if (_introStatusText != null)
            {
                _introStatusText.text = "准备失败，请重新启动游戏";
            }
            Debug.LogError("Auto KickOff failed");
            yield break;
        }

        // KickOff 成功，更新游戏阶段
        _currentGamePhase = GamePhase.AfterKickOff;
        _currentPhaseCompleted = true;

        // KickOff 完成后更新界面
        if (_introStatusText != null)
        {
            _introStatusText.text = "准备已完成";
        }

        // 显示关闭按钮
        if (_introCloseButton != null)
        {
            _introCloseButton.gameObject.SetActive(true);
        }

        Debug.Log("Auto KickOff completed - Game phase updated to AfterKickOff");
    }

    /// <summary>
    /// 点击介绍界面的关闭按钮
    /// </summary>
    public void OnClickCloseIntroPanel()
    {
        if (_gameIntroPanel != null)
        {
            _gameIntroPanel.SetActive(false);
            Debug.Log("Game intro panel closed");
        }
    }

    /// <summary>
    /// 初始化猜测按钮文本
    /// </summary>
    private void InitializeGuessButtonTexts()
    {
        if (_wolf1GuessButtonText != null) _wolf1GuessButtonText.text = "选择狼人1";
        if (_wolf2GuessButtonText != null) _wolf2GuessButtonText.text = "选择狼人2";
        if (_witchGuessButtonText != null) _witchGuessButtonText.text = "选择女巫";
        if (_seerGuessButtonText != null) _seerGuessButtonText.text = "选择预言家";

        // 清空选择状态
        for (int i = 0; i < _selectedActorNames.Length; i++)
        {
            _selectedActorNames[i] = null;
        }
    }

    /// <summary>
    /// 点击猜测按钮（狼人1/狼人2/女巫/预言家）时打开角色选择界面
    /// </summary>
    /// <param name="guessButtonIndex">0=wolf1, 1=wolf2, 2=witch, 3=seer</param>
    public void OnClickGuessButton(int guessButtonIndex)
    {


        if (guessButtonIndex < 0 || guessButtonIndex >= 4)
        {
            Debug.LogWarning($"Invalid guess button index: {guessButtonIndex}");
            return;
        }

        _currentSelectingGuessButtonIndex = guessButtonIndex;
        OpenRoleSelectionPanel();
    }

    /// <summary>
    /// 打开角色选择界面
    /// </summary>
    private void OpenRoleSelectionPanel()
    {
        if (_roleSelectionPanel != null)
        {
            _roleSelectionPanel.SetActive(true);
            SetupRoleSelectionButtonImages(); // 设置角色选择按钮的图片
            UpdateRoleSelectionButtons();
            Debug.Log($"Opened role selection panel for guess button {_currentSelectingGuessButtonIndex}");
        }
    }

    /// <summary>
    /// 设置角色选择界面中所有按钮的面具图片
    /// </summary>
    private void SetupRoleSelectionButtonImages()
    {
        // 获取所有角色的 appearances
        List<string> appearances = WerewolfGameContext.Instance.GetAllActorAppearances();

        // 为每个角色选择按钮设置对应的面具图片（跳过旁白，从索引1开始）
        for (int i = 0; i < _roleSelectionButtons.Length && i + 1 < appearances.Count; i++)
        {
            if (_roleSelectionButtons[i] == null) continue;

            // 提取面具名
            string maskName = ExtractMaskName(appearances[i + 1]);
            
            // 加载对应的 Sprite
            Sprite maskSprite = LoadMaskSprite(maskName);
            
            if (maskSprite != null)
            {
                // 获取按钮的 Image 组件并设置图片
                Image buttonImage = _roleSelectionButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = maskSprite;
                    Debug.Log($"Set role selection button {i} to mask: {maskName}");
                }
                else
                {
                    Debug.LogWarning($"Role selection button {i} has no Image component");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite for role selection button {i}, mask: {maskName}");
            }
        }
    }

    /// <summary>
    /// 更新角色选择界面中按钮的状态（已选择的变暗）
    /// </summary>
    private void UpdateRoleSelectionButtons()
    {
        for (int i = 0; i < _roleSelectionButtons.Length; i++)
        {
            if (_roleSelectionButtons[i] == null) continue;

            string actorName = (i < _actorNames.Count) ? _actorNames[i] : "";
            bool isSelected = System.Array.IndexOf(_selectedActorNames, actorName) >= 0;

            // 检查是否是当前正在选择的按钮已经选择的角色
            bool isCurrentSelection = (_currentSelectingGuessButtonIndex >= 0 && 
                                      _selectedActorNames[_currentSelectingGuessButtonIndex] == actorName);

            // 更新按钮状态
            UpdateRoleSelectionButtonState(_roleSelectionButtons[i], isSelected, isCurrentSelection);
        }
    }

    /// <summary>
    /// 更新单个角色选择按钮的状态
    /// </summary>
    private void UpdateRoleSelectionButtonState(Button button, bool isSelected, bool isCurrentSelection)
    {
        if (button == null) return;

        // 获取按钮的 Image 组件
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // 如果是当前选择的角色，显示为高亮；如果被其他按钮选择，显示为变暗
            if (isCurrentSelection)
            {
                buttonImage.color = new Color(1f, 1f, 0.5f, 1f); // 黄色高亮
            }
            else if (isSelected)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 变暗
            }
            else
            {
                buttonImage.color = Color.white; // 正常
            }
        }
    }

    /// <summary>
    /// 点击角色选择界面中的角色按钮
    /// </summary>
    /// <param name="roleButtonIndex">角色按钮索引 (0-5)</param>
    public void OnClickRoleSelectionButton(int roleButtonIndex)
    {
        if (roleButtonIndex < 0 || roleButtonIndex >= _actorNames.Count)
        {
            Debug.LogWarning($"Invalid role button index: {roleButtonIndex}");
            return;
        }

        if (_currentSelectingGuessButtonIndex < 0 || _currentSelectingGuessButtonIndex >= 4)
        {
            Debug.LogWarning($"Invalid current selecting guess button index: {_currentSelectingGuessButtonIndex}");
            return;
        }

        string actorName = _actorNames[roleButtonIndex];
        string currentSelection = _selectedActorNames[_currentSelectingGuessButtonIndex];

        // 如果点击的是当前已选择的角色，取消选择
        if (currentSelection == actorName)
        {
            _selectedActorNames[_currentSelectingGuessButtonIndex] = null;
            UpdateGuessButtonText(_currentSelectingGuessButtonIndex, null);
            Debug.Log($"Deselected {actorName} from guess button {_currentSelectingGuessButtonIndex}");
        }
        else
        {
            // 检查该角色是否已被其他按钮选择
            int existingIndex = System.Array.IndexOf(_selectedActorNames, actorName);
            if (existingIndex >= 0 && existingIndex != _currentSelectingGuessButtonIndex)
            {
                Debug.LogWarning($"{actorName} is already selected by guess button {existingIndex}");
                _guessResultText.text = $"{actorName} 已被其他按钮选择";
                return;
            }

            // 选择该角色
            _selectedActorNames[_currentSelectingGuessButtonIndex] = actorName;
            UpdateGuessButtonText(_currentSelectingGuessButtonIndex, actorName);
            Debug.Log($"Selected {actorName} for guess button {_currentSelectingGuessButtonIndex}");
        }

        // 更新界面中所有按钮的状态
        UpdateRoleSelectionButtons();
    }

    /// <summary>
    /// 更新猜测按钮的显示文本
    /// </summary>
    private void UpdateGuessButtonText(int guessButtonIndex, string actorName)
    {
        TMP_Text textComponent = null;
        string defaultText = "";

        switch (guessButtonIndex)
        {
            case 0:
                textComponent = _wolf1GuessButtonText;
                defaultText = "选择狼人1";
                break;
            case 1:
                textComponent = _wolf2GuessButtonText;
                defaultText = "选择狼人2";
                break;
            case 2:
                textComponent = _witchGuessButtonText;
                defaultText = "选择女巫";
                break;
            case 3:
                textComponent = _seerGuessButtonText;
                defaultText = "选择预言家";
                break;
        }

        if (textComponent != null)
        {
            textComponent.text = string.IsNullOrEmpty(actorName) ? defaultText : actorName;
        }
    }

    /// <summary>
    /// 点击角色选择界面的返回按钮
    /// </summary>
    public void OnClickRoleSelectionReturn()
    {
        CloseRoleSelectionPanel();
    }

    /// <summary>
    /// 关闭角色选择界面
    /// </summary>
    private void CloseRoleSelectionPanel()
    {
        if (_roleSelectionPanel != null)
        {
            _roleSelectionPanel.SetActive(false);
            _currentSelectingGuessButtonIndex = -1;
            Debug.Log("Closed role selection panel");
        }
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
            // 根据 _showAllMessages 状态决定是否过滤消息
            if (!_showAllMessages)
            {
                // 只显示发言消息，过滤掉内心消息和夜晚行动
                if (msg.MessageType == WerewolfGameContext.MessageRecordType.Mind)
                {
                    continue;
                }

                if (msg.MessageType == WerewolfGameContext.MessageRecordType.NightActionEvent)
                {
                    continue;
                }
            }

            string prefix;
            switch (msg.MessageType)
            {
                case WerewolfGameContext.MessageRecordType.NightActionEvent:
                    prefix = "[夜晚行动]";
                    break;
                case WerewolfGameContext.MessageRecordType.Discussion:
                    prefix = "[发言]";
                    break;
                case WerewolfGameContext.MessageRecordType.Mind:
                    prefix = "[内心]";
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
    /// 点击检测结果按钮时调用：验证所有猜测按钮中选择的角色身份是否正确
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
        string wolf1Name = _selectedActorNames[0];
        bool wolf1Correct = CheckRoleGuess(wolf1Name, "狼人", actorNames);
        results.Add($"狼人1: {(wolf1Correct ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(wolf1Name) ? "未选择" : wolf1Name)})");
        
        // 检测狼人2
        string wolf2Name = _selectedActorNames[1];
        bool wolf2Correct = CheckRoleGuess(wolf2Name, "狼人", actorNames);
        results.Add($"狼人2: {(wolf2Correct ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(wolf2Name) ? "未选择" : wolf2Name)})");
        
        // 检测女巫
        string witchName = _selectedActorNames[2];
        bool witchCorrect = CheckRoleGuess(witchName, "女巫", actorNames);
        results.Add($"女巫: {(witchCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(witchName) ? "未选择" : witchName)})");
        
        // 检测预言家
        string seerName = _selectedActorNames[3];
        bool seerCorrect = CheckRoleGuess(seerName, "预言家", actorNames);
        results.Add($"预言家: {(seerCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(seerName) ? "未选择" : seerName)})");
        
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

                if (!_isKickOffComplete)
        {
            Debug.LogWarning("Please complete kick off before guessing");
            _mainText.text = "请先完成游戏开局 (Kick Off)";
            return;
        }
        
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

    /// <summary>
    /// 记录当前所有角色的死亡状态（在进入夜晚前调用）
    /// </summary>
    private IEnumerator RecordDeathStatusBeforeNight()
    {
        Debug.Log("=== 记录夜晚前的死亡状态 ===");

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

        // 记录所有角色的死亡状态
        _deathStatusBeforeNight.Clear();
        var actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        for (int i = 1; i < actorNames.Count; i++)
        {
            bool hasDeath = WerewolfGameContext.Instance.HasDeathComponent(i);
            _deathStatusBeforeNight[actorNames[i]] = hasDeath;
            Debug.Log($"记录 {actorNames[i]}: {(hasDeath ? "已死亡" : "存活")}");
        }
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
    /// 检测夜晚期间的新死亡，并返回死亡消息
    /// </summary>
    private IEnumerator GetNightDeathMessage(System.Action<string> callback)
    {
        Debug.Log("=== 检测夜晚期间的新死亡 ===");

        // 获取最新的角色实体数据
        yield return _actorDetailsAction.Call(
            WerewolfGameContext.Instance.ActorDetailsUrl,
            WerewolfGameContext.Instance.GetAllActorNames()
        );

        if (_actorDetailsAction.ResponseData == null)
        {
            Debug.LogError("ActorDetailsAction ResponseData is null");
            callback?.Invoke("");
            yield break;
        }

        // 更新角色实体数据
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.ResponseData.actor_entities_serialization
        );

        // 检测新死亡的角色
        List<string> newDeaths = new List<string>();
        var actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        
        for (int i = 1; i < actorNames.Count; i++)
        {
            string actorName = actorNames[i];
            bool currentlyDead = WerewolfGameContext.Instance.HasDeathComponent(i);
            
            // 如果之前记录的状态存在
            if (_deathStatusBeforeNight.ContainsKey(actorName))
            {
                bool wasDeadBefore = _deathStatusBeforeNight[actorName];
                
                // 如果之前活着，现在死了，说明是夜晚期间死亡
                if (!wasDeadBefore && currentlyDead)
                {
                    newDeaths.Add(actorName);
                    Debug.Log($"检测到新死亡: {actorName}");
                }
            }
            
            // 更新按钮状态
            if (currentlyDead && i - 1 < 6)
            {
                UpdateButtonState(i - 1, false);
            }
        }

        // 生成死亡消息
        string deathMessage;
        if (newDeaths.Count > 0)
        {
            List<string> deathMessages = new List<string>();
            foreach (string deadActor in newDeaths)
            {
                deathMessages.Add($"昨晚角色（{deadActor}）死了");
            }
            deathMessage = string.Join("\n", deathMessages);
            Debug.Log($"夜晚死亡消息: {deathMessage}");
        }
        else
        {
            deathMessage = "昨晚是平安夜";
            Debug.Log("昨晚是平安夜");
        }

        callback?.Invoke(deathMessage);
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

    // OnClickKickOff 方法已移除，因为 kick off 现在是自动执行的
    // Kick off 会在场景启动时自动通过 AutoKickOff() 执行

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

        // 隐藏 loading，显示完成文本
        SetLoadingState(false);
        _mainText.text = "开局已完成\n点击角色按钮查看对应消息";
        // UpdateButtonTextsWithRoles(); // 显示真实身份

    }

    private IEnumerator Time()
    {
        // 隐藏角色详情图片
        HideActorDetailsPanel();

        // 检查当前阶段是否是夜晚后（用于判断是否需要添加死亡信息）
        string currentPhase = WerewolfGameContext.Instance.CurrentPhase;
        bool isAfterNight = !string.IsNullOrEmpty(currentPhase) && currentPhase.StartsWith("night_");
        Debug.Log($"Time called - Current Phase: {currentPhase}, IsAfterNight: {isAfterNight}");

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

        // 如果是夜晚后的时间推进，检测死亡情况
        string nightDeathMessage = "";
        if (isAfterNight)
        {
            bool deathMessageReceived = false;
            yield return GetNightDeathMessage((message) =>
            {
                nightDeathMessage = message;
                deathMessageReceived = true;
            });

            // 等待回调完成
            while (!deathMessageReceived)
            {
                yield return null;
            }

            Debug.Log($"Night death message: {nightDeathMessage}");
        }
        else
        {
            // 不是夜晚后的时间推进，只检测死亡状态（更新按钮）
            StartCoroutine(CheckActorDeathStatus());
        }

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
            string baseMessage = GetFormattedMainText(_sessionMessagesAction.ResponseData.session_messages);
            
            // 如果是夜晚后的时间推进，在消息前添加死亡信息
            if (isAfterNight && !string.IsNullOrEmpty(nightDeathMessage))
            {
                _mainText.text = nightDeathMessage + "\n\n" + baseMessage;
            }
            else
            {
                _mainText.text = baseMessage;
            }
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

        // 在进入夜晚前，记录所有角色的死亡状态
        yield return RecordDeathStatusBeforeNight();

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
        _mainText.text = "夜晚行动已完成";
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

    /// <summary>
    /// 获取格式化的消息文本（不直接设置到 _mainText）
    /// </summary>
    private string GetFormattedMainText(List<SessionMessage> messages)
    {
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            Debug.Log($"Client Message {i}: " + JsonUtility.ToJson(message));
        }

        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(messages);
        return string.Join("\n", processedMessages);
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

        // 重置游戏上下文中的所有数据
        WerewolfGameContext.Instance.Reset();

        // 加载 Launch Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("WerewolfGameLaunchScene");
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
                // Kick off 已经在场景启动时自动完成，这里不应该再被调用
                Debug.LogWarning("Game phase is NotStarted, but kick off should have been completed automatically");
                _lastErrorMessage = "游戏初始化错误";
                _mainText.text = "游戏初始化错误，请重新启动场景";
                yield break;

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
                return "游戏正在初始化...";
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
