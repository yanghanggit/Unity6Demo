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

    public StagesStateApi _stagesStateAction;

    public WerewolfGameStateAction _werewolfGameStateAction;

    public ActorDetailsApi _actorDetailsAction;

    public SessionMessagesApi _sessionMessagesAction;

    // 角色按钮相关组件数组（在 Inspector 中配置，长度为8）
    public TMP_Text[] actorButtonTexts = new TMP_Text[8];
    public Button[] actorButtons = new Button[8];
    public Image[] actorButtonImages = new Image[8];
    
    // 死亡覆盖图片数组（在 Inspector 中配置，长度为8）
    // 这些图片会在角色死亡时显示在原有图片上方
    public Image[] actorDeathOverlayImages = new Image[8];

    // 下一阶段按钮的Text组件
    public TMP_Text _nextPhaseButtonText;

    // 角色详情界面中的身份猜测按钮（在角色详情界面中配置）
    public Button _detailWolf1Button;      // 详情界面中的"狼人1"按钮
    public Button _detailWolf2Button;      // 详情界面中的"狼人2"按钮
    public Button _detailWitchButton;      // 详情界面中的"女巫"按钮
    public Button _detailSeerButton;       // 详情界面中的"预言家"按钮
    public Button _detailWolf3Button;      // 详情界面中的"狼人3"按钮
    public Button _detailHunterButton;     // 详情界面中的"猎人"按钮
    
    // 详情界面猜测按钮的文本组件
    public TMP_Text _detailWolf1ButtonText;
    public TMP_Text _detailWolf2ButtonText;
    public TMP_Text _detailWitchButtonText;
    public TMP_Text _detailSeerButtonText;
    public TMP_Text _detailWolf3ButtonText;
    public TMP_Text _detailHunterButtonText;
    
    // 角色猜测状态：存储每个身份对应的角色名
    // 索引: 0=狼人1, 1=狼人2, 2=狼人3, 3=女巫, 4=预言家, 5=猎人
    private string[] _selectedActorNames = new string[6];
    
    // 记录哪些选择是在第一天做出的（第二天时这些选择将被锁定）
    private bool[] _isLockedFromDay1 = new bool[6];
    
    // 当前正在查看的角色索引（用于详情界面）
    private int _currentViewingActorIndex = -1;
    
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
    
    // 猜测功能是否可用的标记
    private bool _isGuessingEnabled = false;
    
    // 游戏是否已结束
    private bool _isGameEnded = false;
    
    // 保存夜晚阶段的猎人击杀消息（在 Time 阶段显示）
    private string _hunterKillMessageFromNight = "";

    // 自动重试相关配置
    private const int MAX_KICKOFF_RETRY_COUNT = 3;  // 最大重试次数
    private const float RETRY_DELAY_SECONDS = 2f;   // 重试延迟（秒）

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

        // 初始化猜测按钮状态
        InitializeGuessButtons();

        // 从 Context 读取游戏模式，设置消息过滤状态
        _showAllMessages = WerewolfGameContext.Instance.IsDebugMode;
        Debug.Log($"Game mode: {(_showAllMessages ? "Debug Mode (显示所有消息)" : "Play Mode (只显示发言)")}");

        SetupButtonImages();
        
        // 初始化死亡覆盖图片为隐藏状态
        InitializeDeathOverlayImages();

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
    /// 自动执行 Kick Off（带重试机制）
    /// </summary>
    private IEnumerator AutoKickOff()
    {
        Debug.Log("Auto KickOff started");

        int retryCount = 0;
        bool success = false;

        // 重试循环
        while (retryCount < MAX_KICKOFF_RETRY_COUNT && !success)
        {
            if (retryCount > 0)
            {
                Debug.Log($"Retrying KickOff... Attempt {retryCount + 1}/{MAX_KICKOFF_RETRY_COUNT}");
                if (_introStatusText != null)
                {
                    _introStatusText.text = $"玩家准备中... (重新尝试 {retryCount + 1}/{MAX_KICKOFF_RETRY_COUNT})";
                }
                
                // 等待一段时间后重试
                yield return new WaitForSeconds(RETRY_DELAY_SECONDS);
            }
            else
            {
                if (_introStatusText != null)
                {
                    _introStatusText.text = "玩家准备中......";
                }
            }

            // 执行 KickOff 逻辑
            yield return KickOff();

            // 检查 KickOff 是否成功
            if (_werewolfGamePlayAction.ResponseData != null && _sessionMessagesAction.RespData != null)
            {
                // KickOff 成功
                success = true;
                Debug.Log($"Auto KickOff succeeded on attempt {retryCount + 1}");
            }
            else
            {
                // KickOff 失败，准备重试
                retryCount++;
                Debug.LogWarning($"Auto KickOff failed on attempt {retryCount}");
            }
        }

        // 检查最终结果
        if (!success)
        {
            // 所有重试都失败了
            if (_introStatusText != null)
            {
                _introStatusText.text = $"玩家准备失败（已重试{MAX_KICKOFF_RETRY_COUNT}次）\n请检查网络连接或重新启动游戏";
            }
            Debug.LogError($"Auto KickOff failed after {MAX_KICKOFF_RETRY_COUNT} attempts");
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
    /// 初始化猜测按钮状态
    /// </summary>
    private void InitializeGuessButtons()
    {
        // 清空选择状态和锁定状态
        for (int i = 0; i < _selectedActorNames.Length; i++)
        {
            _selectedActorNames[i] = null;
            _isLockedFromDay1[i] = false;
        }

        // 初始化详情界面中的猜测按钮文本
        if (_detailWolf1ButtonText != null) _detailWolf1ButtonText.text = "狼人1";
        if (_detailWolf2ButtonText != null) _detailWolf2ButtonText.text = "狼人2";
        if (_detailWolf3ButtonText != null) _detailWolf3ButtonText.text = "狼人3";
        if (_detailWitchButtonText != null) _detailWitchButtonText.text = "女巫";
        if (_detailSeerButtonText != null) _detailSeerButtonText.text = "预言家";
        if (_detailHunterButtonText != null) _detailHunterButtonText.text = "猎人";
        
        // 注意：不在这里调用 UpdateGuessButtonsAvailability()
        // 因为按钮状态会在打开角色详情界面时（OnClickActorButton）更新
    }

    /// <summary>
    /// 判断当前是否可以进行猜测
    /// 只在第1天和第2天的白天讨论开始到投票结束之间可以猜测
    /// </summary>
    private bool CanGuessNow()
    {
        // 获取当前是第几个白天
        int currentDay = GetCurrentDayNumber();
        
        // 只在第1天或第2天可以猜测
        if (currentDay != 1 && currentDay != 2)
        {
            return false;
        }

        // 只在白天讨论阶段、讨论完成阶段、投票阶段可以猜测
        return _currentGamePhase == GamePhase.AfterDay ||
               _currentGamePhase == GamePhase.DiscussionComplete ||
               _currentGamePhase == GamePhase.AfterVote;
    }

    /// <summary>
    /// 更新所有猜测按钮的可用性
    /// </summary>
    private void UpdateGuessButtonsAvailability()
    {
        _isGuessingEnabled = CanGuessNow();
        
        // 只在按钮存在时更新状态（按钮可能在角色详情界面未打开时不存在）
        if (_detailWolf1Button != null) _detailWolf1Button.interactable = _isGuessingEnabled;
        if (_detailWolf2Button != null) _detailWolf2Button.interactable = _isGuessingEnabled;
        if (_detailWitchButton != null) _detailWitchButton.interactable = _isGuessingEnabled;
        if (_detailSeerButton != null) _detailSeerButton.interactable = _isGuessingEnabled;
        if (_detailWolf3Button != null) _detailWolf3Button.interactable = _isGuessingEnabled;
        if (_detailHunterButton != null) _detailHunterButton.interactable = _isGuessingEnabled;

        Debug.Log($"Guessing buttons availability updated: Enabled={_isGuessingEnabled}, Day={GetCurrentDayNumber()}, Phase={_currentGamePhase}");
    }

    /// <summary>
    /// 点击角色详情界面中的猜测按钮
    /// </summary>
    /// <param name="roleTypeIndex">角色类型索引 (0=狼人1, 1=狼人2, 2=女巫, 3=预言家)</param>
    public void OnClickDetailGuessButton(int roleTypeIndex)
    {
        // 检查猜测功能是否可用
        if (!_isGuessingEnabled)
        {
            Debug.LogWarning("Guessing is not available at this phase");
            return;
        }

        if (roleTypeIndex < 0 || roleTypeIndex >= 6)
        {
            Debug.LogWarning($"Invalid role type index: {roleTypeIndex}");
            return;
        }

        if (_currentViewingActorIndex < 0 || _currentViewingActorIndex >= _actorNames.Count)
        {
            Debug.LogWarning($"Invalid current viewing actor index: {_currentViewingActorIndex}");
            return;
        }

        string actorName = _actorNames[_currentViewingActorIndex];
        string currentlyAssignedToRole = _selectedActorNames[roleTypeIndex];
        int currentDay = GetCurrentDayNumber();

        // 检查是否在第二天尝试修改第一天的选择
        if (currentDay == 2 && _isLockedFromDay1[roleTypeIndex])
        {
            Debug.LogWarning($"{GetRoleTypeName(roleTypeIndex)} 已在第一天锁定，无法在第二天修改");
            return;
        }

        // 情况1: 点击的身份已经分配给当前角色 -> 取消选择（仅第一天可以）
        if (currentlyAssignedToRole == actorName)
        {
            // 第二天不允许取消已锁定的选择
            if (currentDay == 2 && _isLockedFromDay1[roleTypeIndex])
            {
                Debug.LogWarning($"第二天无法取消第一天的选择: {GetRoleTypeName(roleTypeIndex)}");
                return;
            }

            // 取消选择
            _selectedActorNames[roleTypeIndex] = null;
            
            // 如果是在第一天取消选择，也需要清除锁定标记
            if (currentDay == 1)
            {
                _isLockedFromDay1[roleTypeIndex] = false;
                Debug.Log($"Cleared lock for {GetRoleTypeName(roleTypeIndex)} (Day 1 cancellation)");
            }
            
            Debug.Log($"Deselected {actorName} from {GetRoleTypeName(roleTypeIndex)}");
        }
        // 情况2: 点击的身份已分配给其他角色 -> 不允许操作
        else if (!string.IsNullOrEmpty(currentlyAssignedToRole))
        {
            Debug.LogWarning($"{GetRoleTypeName(roleTypeIndex)} 已分配给 {currentlyAssignedToRole}");
        }
        // 情况3: 身份未分配 -> 检查当前角色是否已选择其他身份
        else
        {
            // 查找当前角色是否已经选择了其他身份
            int previousRoleIndex = -1;
            for (int i = 0; i < _selectedActorNames.Length; i++)
            {
                if (_selectedActorNames[i] == actorName)
                {
                    previousRoleIndex = i;
                    break;
                }
            }

            // 如果当前角色已选择其他身份，检查是否可以取消
            if (previousRoleIndex >= 0)
            {
                // 第二天不允许取消第一天锁定的选择
                if (currentDay == 2 && _isLockedFromDay1[previousRoleIndex])
                {
                    Debug.LogWarning($"第二天无法更改第一天的选择: {GetRoleTypeName(previousRoleIndex)}");
                    return;
                }

                string previousRoleName = GetRoleTypeName(previousRoleIndex);
                _selectedActorNames[previousRoleIndex] = null;
                
                // 如果是在第一天更改选择，清除之前的锁定标记
                if (currentDay == 1)
                {
                    _isLockedFromDay1[previousRoleIndex] = false;
                    Debug.Log($"Cleared lock for {previousRoleName} (Day 1 role change)");
                }
                
                Debug.Log($"Removed {actorName} from {previousRoleName}");
            }

            // 分配新身份
            _selectedActorNames[roleTypeIndex] = actorName;
            
            // 如果是第一天的选择，标记为锁定
            if (currentDay == 1)
            {
                _isLockedFromDay1[roleTypeIndex] = true;
                Debug.Log($"Locked {GetRoleTypeName(roleTypeIndex)} for {actorName} from Day 1");
            }
            
            Debug.Log($"Assigned {actorName} as {GetRoleTypeName(roleTypeIndex)}");
        }

        // 更新按钮状态
        UpdateDetailGuessButtonsState();
    }

    /// <summary>
    /// 获取角色类型名称
    /// </summary>
    private string GetRoleTypeName(int roleTypeIndex)
    {
        switch (roleTypeIndex)
        {
            case 0: return "狼人1";
            case 1: return "狼人2";
            case 2: return "狼人3";
            case 3: return "女巫";
            case 4: return "预言家";
            case 5: return "猎人";
            default: return "未知";
        }
    }

    /// <summary>
    /// 更新详情界面中猜测按钮的状态
    /// </summary>
    private void UpdateDetailGuessButtonsState()
    {
        if (_currentViewingActorIndex < 0 || _currentViewingActorIndex >= _actorNames.Count)
        {
            return;
        }

        // 先更新猜测功能是否可用的状态
        _isGuessingEnabled = CanGuessNow();

        string currentActorName = _actorNames[_currentViewingActorIndex];

        // 更新6个猜测按钮的状态（按新顺序）
        UpdateSingleDetailGuessButton(_detailWolf1Button, _detailWolf1ButtonText, 0, currentActorName);
        UpdateSingleDetailGuessButton(_detailWolf2Button, _detailWolf2ButtonText, 1, currentActorName);
        UpdateSingleDetailGuessButton(_detailWolf3Button, _detailWolf3ButtonText, 2, currentActorName);
        UpdateSingleDetailGuessButton(_detailWitchButton, _detailWitchButtonText, 3, currentActorName);
        UpdateSingleDetailGuessButton(_detailSeerButton, _detailSeerButtonText, 4, currentActorName);
        UpdateSingleDetailGuessButton(_detailHunterButton, _detailHunterButtonText, 5, currentActorName);
    }

    /// <summary>
    /// 更新单个详情猜测按钮的状态
    /// </summary>
    private void UpdateSingleDetailGuessButton(Button button, TMP_Text buttonText, int roleTypeIndex, string currentActorName)
    {
        if (button == null) return;

        string assignedActor = _selectedActorNames[roleTypeIndex];
        bool isAssigned = !string.IsNullOrEmpty(assignedActor);
        bool isCurrentActor = assignedActor == currentActorName;
        int currentDay = GetCurrentDayNumber();
        bool isLockedFromDay1 = _isLockedFromDay1[roleTypeIndex];

        // 如果猜测功能不可用，禁用所有按钮
        if (!_isGuessingEnabled)
        {
            button.interactable = false;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            button.colors = colors;

            if (buttonText != null)
            {
                if (isAssigned)
                {
                    buttonText.text = $"{GetRoleTypeName(roleTypeIndex)}\n({assignedActor})\n[已锁定]";
                }
                else
                {
                    buttonText.text = $"{GetRoleTypeName(roleTypeIndex)}\n[已锁定]";
                }
            }
            return;
        }

        // 特殊处理：第二天时，第一天锁定的选择显示为高亮+已锁定
        if (currentDay == 2 && isLockedFromDay1 && isCurrentActor)
        {
            button.interactable = false; // 禁止交互
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(1f, 1f, 0.5f, 1f); // 黄色高亮
            button.colors = colors;

            if (buttonText != null)
            {
                buttonText.text = $"{GetRoleTypeName(roleTypeIndex)}\n(已选)\n[已锁定]";
            }
            return;
        }

        // 更新按钮的可交互状态和颜色（猜测功能可用时）
        if (isAssigned && !isCurrentActor)
        {
            // 已被其他角色选择：禁用按钮，变灰
            button.interactable = false;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            button.colors = colors;

            // 更新文本显示已分配的角色
            if (buttonText != null)
            {
                buttonText.text = $"{GetRoleTypeName(roleTypeIndex)}\n({assignedActor})";
            }
        }
        else if (isCurrentActor)
        {
            // 当前角色已选择此身份：高亮显示（第一天可以修改）
            button.interactable = true;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.9f, 0.8f, 0.3f, 1f); // 黄色高亮
            button.colors = colors;

            if (buttonText != null)
            {
                buttonText.text = $"{GetRoleTypeName(roleTypeIndex)}\n(已选)";
            }
        }
        else
        {
            // 未分配：正常状态
            button.interactable = true;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;

            if (buttonText != null)
            {
                buttonText.text = GetRoleTypeName(roleTypeIndex);
            }
        }
    }

    /// <summary>
    /// 初始化死亡覆盖图片：默认全部隐藏
    /// </summary>
    private void InitializeDeathOverlayImages()
    {
        if (actorDeathOverlayImages == null)
        {
            Debug.LogWarning("actorDeathOverlayImages array is not assigned");
            return;
        }
        
        for (int i = 0; i < actorDeathOverlayImages.Length; i++)
        {
            if (actorDeathOverlayImages[i] != null)
            {
                actorDeathOverlayImages[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log("Initialized death overlay images (all hidden)");
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

        // 记录当前查看的角色索引
        _currentViewingActorIndex = buttonIndex;

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

        // 更新详情界面中的猜测按钮状态
        UpdateDetailGuessButtonsState();
    }

    // 显示特定阶段的角色消息
    private void ShowActorMessagesForPhase(string actorName, string phase)
    {
        var messages = WerewolfGameContext.Instance.GetMessagesByActorAndPhase(actorName, phase);
        
        List<string> displayMessages = new List<string>();
        
        // 检查角色是否已死亡
        var actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        int actorIndex = actorNames.IndexOf(actorName);
        bool isDead = false;
        
        if (actorIndex > 0)  // 跳过旁白（索引0）
        {
            isDead = WerewolfGameContext.Instance.HasDeathComponent(actorIndex);
        }
        
        // 添加标题，如果角色已死亡则显示提示
        if (isDead)
        {
            displayMessages.Add($"=== {actorName} 的 {GetPhaseFriendlyName(phase)} 阶段消息 ===");
            displayMessages.Add("【该角色已死亡】");
        }
        else
        {
            displayMessages.Add($"=== {actorName} 的 {GetPhaseFriendlyName(phase)} 阶段消息 ===");
        }
        
        if (messages == null || messages.Count == 0)
        {
            displayMessages.Add($"{actorName} 在 {GetPhaseFriendlyName(phase)} 阶段没有消息");
        }
        else
        {
            AppendMessagesWithPrefix(displayMessages, messages);
        }

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

    /// <summary>
    /// 获取当前是第几个白天（从1开始计数）
    /// 如果不是白天阶段，返回0
    /// </summary>
    private int GetCurrentDayNumber()
    {
        string currentPhase = WerewolfGameContext.Instance.CurrentPhase;
        
        if (string.IsNullOrEmpty(currentPhase))
        {
            return 0;
        }

        if (currentPhase.StartsWith("day_"))
        {
            string turnStr = currentPhase.Substring(4); // 提取 turn number
            if (int.TryParse(turnStr, out int turn))
            {
                return turn / 2; // 与 GetPhaseFriendlyName 中的逻辑一致
            }
        }

        return 0;
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
                actorButtonTexts[i].text += "\n" + role;
                Debug.Log($"Updated button {i + 1} to role: {role} (actorIndex: {actorIndex})");
            }
        }
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
    /// 显示猜测结果（游戏结束后调用）
    /// </summary>
    private string GetGuessResultText()
    {
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
        
        // 检测狼人3
        string wolf3Name = _selectedActorNames[2];
        bool wolf3Correct = CheckRoleGuess(wolf3Name, "狼人", actorNames);
        results.Add($"狼人3: {(wolf3Correct ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(wolf3Name) ? "未选择" : wolf3Name)})");
        
        // 检测女巫
        string witchName = _selectedActorNames[3];
        bool witchCorrect = CheckRoleGuess(witchName, "女巫", actorNames);
        results.Add($"女巫: {(witchCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(witchName) ? "未选择" : witchName)})");
        
        // 检测预言家
        string seerName = _selectedActorNames[4];
        bool seerCorrect = CheckRoleGuess(seerName, "预言家", actorNames);
        results.Add($"预言家: {(seerCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(seerName) ? "未选择" : seerName)})");
        
        // 检测猎人
        string hunterName = _selectedActorNames[5];
        bool hunterCorrect = CheckRoleGuess(hunterName, "猎人", actorNames);
        results.Add($"猎人: {(hunterCorrect ? "✓ 猜测成功" : "✗ 猜测失败")} (选择了 {(string.IsNullOrEmpty(hunterName) ? "未选择" : hunterName)})");
        
        // 计算总分
        int correctCount = 0;
        if (wolf1Correct) correctCount++;
        if (wolf2Correct) correctCount++;
        if (wolf3Correct) correctCount++;
        if (witchCorrect) correctCount++;
        if (seerCorrect) correctCount++;
        if (hunterCorrect) correctCount++;
        
        // 生成结果文本
        string resultText = "\n\n=== 身份猜测结果 ===\n" + 
                           string.Join("\n", results) + 
                           $"\n\n总计: {correctCount}/6 正确";
        
        Debug.Log(resultText);
        return resultText;
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

        if (_actorDetailsAction.RespData == null)
        {
            Debug.LogError("ActorDetailsAction ResponseData is null");
            yield break;
        }

        // 更新角色实体数据
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.RespData.actor_entities_serialization
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

        if (_actorDetailsAction.RespData == null)
        {
            Debug.LogError("ActorDetailsAction ResponseData is null");
            yield break;
        }

        // 更新角色实体数据
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.RespData.actor_entities_serialization
        );

        // 检测每个角色的死亡状态（从索引1开始，跳过旁白）
        var actorNames = WerewolfGameContext.Instance.GetAllActorNames();
        for (int i = 1; i < actorNames.Count; i++)
        {
            bool hasDeath = WerewolfGameContext.Instance.HasDeathComponent(i);
            Debug.Log($"{actorNames[i]}: {(hasDeath ? "已死亡 ☠" : "存活 ✓")}");

            // 更新对应按钮的状态（i-1 是因为按钮索引从0开始，而角色从1开始）
            if (hasDeath && i - 1 < actorButtons.Length)
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

        if (_actorDetailsAction.RespData == null)
        {
            Debug.LogError("ActorDetailsAction ResponseData is null");
            callback?.Invoke("");
            yield break;
        }

        // 更新角色实体数据
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.RespData.actor_entities_serialization
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
            else if (currentlyDead)
            {
                // 如果字典中没有记录（第一个夜晚的情况），但角色已死亡，也算作新死亡
                newDeaths.Add(actorName);
                Debug.Log($"检测到新死亡（首次记录）: {actorName}");
            }
            
            // 更新按钮状态 - 无论是否是新死亡，只要已死亡就更新按钮颜色
            if (currentlyDead && i - 1 < actorButtons.Length)
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
                deathMessages.Add($"昨晚（{deadActor}）死了");
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
    /// 从会话消息中提取猎人击杀消息（EventHead.NONE 类型的消息）
    /// </summary>
    /// <param name="messages">会话消息列表</param>
    /// <returns>猎人击杀消息，如果没有则返回空字符串</returns>
    private string GetHunterKillMessage(List<SessionMessage> messages)
    {
        if (messages == null || messages.Count == 0)
        {
            return "";
        }

        // 筛选出 EventHead.NONE 类型的消息（游戏事件）
        List<string> hunterMessages = new List<string>();

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
                        // 获取消息内容
                        if (message.data.ContainsKey("message"))
                        {
                            string messageContent = message.data["message"]?.ToString() ?? "";
                            
                            // 检查消息是否包含猎人击杀相关的关键词
                            if (messageContent.Contains("猎人") && 
                                (messageContent.Contains("击杀") || messageContent.Contains("开枪") || 
                                 messageContent.Contains("带走") || messageContent.Contains("射杀")))
                            {
                                hunterMessages.Add(messageContent);
                                Debug.Log($"检测到猎人击杀消息: {messageContent}");
                            }
                        }
                    }
                }
            }
        }

        // 如果找到猎人击杀消息，返回合并后的消息
        if (hunterMessages.Count > 0)
        {
            return string.Join("\n", hunterMessages);
        }

        return "";
    }

    /// <summary>
    /// 更新按钮状态：死亡时文字变红色、显示死亡覆盖图片，但保持可点击
    /// </summary>
    /// <param name="buttonIndex">按钮索引 (0-7)</param>
    /// <param name="isAlive">是否存活</param>
    private void UpdateButtonState(int buttonIndex, bool isAlive)
    {
        if (buttonIndex < 0 || buttonIndex >= actorButtons.Length || actorButtons[buttonIndex] == null)
        {
            Debug.LogWarning($"Invalid button at index: {buttonIndex}");
            return;
        }

        Button button = actorButtons[buttonIndex];
        // 保持按钮可点击
        button.interactable = true;

        // 修改按钮颜色（保持原样）
        ColorBlock colors = button.colors;
        Color targetColor = isAlive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.normalColor = targetColor;
        colors.highlightedColor = isAlive ? new Color(0.9f, 0.9f, 0.9f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
        colors.pressedColor = isAlive ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.disabledColor = targetColor;
        button.colors = colors;
        
        // 修改文字颜色：死亡时变红色
        if (buttonIndex < actorButtonTexts.Length && actorButtonTexts[buttonIndex] != null)
        {
            actorButtonTexts[buttonIndex].color = isAlive ? Color.white : Color.red;
        }
        
        // 显示/隐藏死亡覆盖图片
        if (buttonIndex < actorDeathOverlayImages.Length && actorDeathOverlayImages[buttonIndex] != null)
        {
            actorDeathOverlayImages[buttonIndex].gameObject.SetActive(!isAlive);
        }

        Debug.Log($"Button {buttonIndex + 1}: {(isAlive ? "Active" : "Dead (Red Text + Overlay Image, but Clickable)")}");
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
        // _sessionMessagesAction.Initialize(
        //     WerewolfGameContext.Instance.SessionMessagesUrl,
        //     WerewolfGameContext.Instance.UserName,
        //     WerewolfGameContext.Instance.GameName,
        //     WerewolfGameContext.Instance.LastSequenceId
        // );

        yield return _sessionMessagesAction.Call(WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId);
        if (_sessionMessagesAction.RespData == null)
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
            _sessionMessagesAction.RespData.session_messages, "kickoff");
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
        // _sessionMessagesAction.Initialize(
        //     WerewolfGameContext.Instance.SessionMessagesUrl,
        //     WerewolfGameContext.Instance.UserName,
        //     WerewolfGameContext.Instance.GameName,
        //     WerewolfGameContext.Instance.LastSequenceId
        // );

        yield return _sessionMessagesAction.Call( WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId);
        if (_sessionMessagesAction.RespData == null)
        {
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 根据消息内容切换背景图片
        SwitchBackgroundByMessages(_sessionMessagesAction.RespData.session_messages);

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

        // 先获取正常的游戏消息（说明上一阶段发生了什么）
        string baseMessage = GetFormattedMainText(_sessionMessagesAction.RespData.session_messages);
        
        // 如果是夜晚后的时间推进，在消息前添加死亡信息和猎人击杀消息
        string stageMessage;
        if (isAfterNight && !string.IsNullOrEmpty(nightDeathMessage))
        {
            // 如果有猎人击杀消息（从夜晚阶段保存的），添加到死亡消息之后
            if (!string.IsNullOrEmpty(_hunterKillMessageFromNight))
            {
                stageMessage = nightDeathMessage + "\n" + _hunterKillMessageFromNight + "\n\n" + baseMessage;
                Debug.Log($"添加猎人击杀消息到 Time 阶段: {_hunterKillMessageFromNight}");
                // 清空猎人击杀消息，避免重复显示
                _hunterKillMessageFromNight = "";
            }
            else
            {
                stageMessage = nightDeathMessage + "\n\n" + baseMessage;
            }
        }
        else
        {
            stageMessage = baseMessage;
        }
        
        // 根据胜利条件显示结果
        if (victoryCondition == "TOWN_VICTORY")
        {
            UpdateButtonTextsWithRoles(); // 显示真实身份
            _isGameEnded = true; // 标记游戏已结束
            
            // 获取猜测结果
            string guessResultText = GetGuessResultText();
            
            // 组合显示：阶段消息 + 胜利信息 + 猜测结果
            _mainText.text = stageMessage + "\n\n=== 游戏结束 ===\n村民胜利！\n点击继续按钮重新开始" + guessResultText;
            
            // 更新按钮文本为"重新开始"
            if (_nextPhaseButtonText != null)
            {
                _nextPhaseButtonText.text = "重新开始";
            }
        }
        else if (victoryCondition == "WEREWOLVES_VICTORY")
        {
            UpdateButtonTextsWithRoles(); // 显示真实身份
            _isGameEnded = true; // 标记游戏已结束
            
            // 获取猜测结果
            string guessResultText = GetGuessResultText();
            
            // 组合显示：阶段消息 + 胜利信息 + 猜测结果
            _mainText.text = stageMessage + "\n\n=== 游戏结束 ===\n狼人胜利！\n点击继续按钮重新开始" + guessResultText;
            
            // 更新按钮文本为"重新开始"
            if (_nextPhaseButtonText != null)
            {
                _nextPhaseButtonText.text = "重新开始";
            }
        }
        else
        {
            // 没有胜利条件时只显示阶段消息
            _mainText.text = stageMessage;
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
        // _sessionMessagesAction.Initialize(
        //     WerewolfGameContext.Instance.SessionMessagesUrl,
        //     WerewolfGameContext.Instance.UserName,
        //     WerewolfGameContext.Instance.GameName,
        //     WerewolfGameContext.Instance.LastSequenceId
        // );

        yield return _sessionMessagesAction.Call(WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId);
        if (_sessionMessagesAction.RespData == null)
        {
            SetLoadingState(false);
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 处理消息
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.RespData.session_messages, "night");
        Debug.Log("Night processed messages:\n" + string.Join("\n", processedMessages));

        // 检测并保存猎人击杀消息（在下一个 Time 阶段显示）
        _hunterKillMessageFromNight = GetHunterKillMessage(_sessionMessagesAction.RespData.session_messages);
        if (!string.IsNullOrEmpty(_hunterKillMessageFromNight))
        {
            Debug.Log($"在夜晚阶段检测到猎人击杀消息: {_hunterKillMessageFromNight}");
        }

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
        // _sessionMessagesAction.Initialize(
        //     WerewolfGameContext.Instance.SessionMessagesUrl,
        //     WerewolfGameContext.Instance.UserName,
        //     WerewolfGameContext.Instance.GameName,
        //     WerewolfGameContext.Instance.LastSequenceId
        //);

        yield return _sessionMessagesAction.Call(WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId);
        if (_sessionMessagesAction.RespData == null)
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
            _sessionMessagesAction.RespData.session_messages, "day");

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
        // _sessionMessagesAction.Initialize(
        //     WerewolfGameContext.Instance.SessionMessagesUrl,
        //     WerewolfGameContext.Instance.UserName,
        //     WerewolfGameContext.Instance.GameName,
        //     WerewolfGameContext.Instance.LastSequenceId
        // );

        yield return _sessionMessagesAction.Call(WerewolfGameContext.Instance.SessionMessagesUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            WerewolfGameContext.Instance.LastSequenceId);
        if (_sessionMessagesAction.RespData == null)
        {
            SetLoadingState(false);
            Debug.LogError("SessionMessagesAction ResponseData is null");
            yield break;
        }

        // 更新最后一个序列ID
        UpdateLastSequenceIdFromResponse();

        // 处理所有消息（包括角色的内心想法等），记录到 GameContext 中
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.RespData.session_messages, "vote");
        Debug.Log("Vote processed messages:\n" + string.Join("\n", processedMessages));

        SetLoadingState(false);

        // 只显示投票结果（EventHead.NONE 的消息）
        ShowVoteResultOnly(_sessionMessagesAction.RespData.session_messages);
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
        if (_sessionMessagesAction.RespData == null)
        {
            Debug.LogWarning("SessionMessagesAction ResponseData is null");
            Debug.Assert(false, "SessionMessagesAction ResponseData is null");
            return;
        }

        if (_sessionMessagesAction.RespLastSequenceId < 0)
        {
            Debug.LogWarning("Invalid last sequence ID");
            return;
        }

        WerewolfGameContext.Instance.LastSequenceId = _sessionMessagesAction.RespLastSequenceId;
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
        
        // 重置本地游戏状态
        _isGuessingEnabled = false;
        _isGameEnded = false;
        
        // 重置猜测选择和锁定状态
        for (int i = 0; i < _selectedActorNames.Length; i++)
        {
            _selectedActorNames[i] = null;
            _isLockedFromDay1[i] = false;
        }

        // 加载 Launch Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("WerewolfGameLaunchScene");
    }

    /// <summary>
    /// 下一阶段按钮：根据当前游戏阶段执行相应的操作
    /// 如果游戏已结束，则重启游戏
    /// </summary>
    public void OnClickNextPhase()
    {
        Debug.Log($"OnClickNextPhase - Current Phase: {_currentGamePhase}, IsGameEnded: {_isGameEnded}");

        // 如果游戏已结束，重启游戏
        if (_isGameEnded)
        {
            RestartGame();
            return;
        }

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
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                
                // 记录进入白天讨论阶段
                int currentDay = GetCurrentDayNumber();
                Debug.Log($"=== Day {currentDay} starts ===");
                
                yield return Day();

                // 检查 Day 是否成功
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                
                // 更新猜测按钮可用性（进入白天讨论阶段）
                UpdateGuessButtonsAvailability();
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
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                if (_werewolfGamePlayAction.ResponseData == null || _sessionMessagesAction.RespData == null)
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
                
                // 投票后禁用猜测功能（离开白天阶段）
                UpdateGuessButtonsAvailability();
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
                return "点击重新开始 (Restart)";
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
