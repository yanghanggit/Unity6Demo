using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WerewolfGamePlayScene : MonoBehaviour
{
    public TMP_Text _mainText;

    public WerewolfGamePlayAction _werewolfGamePlayAction;

    public SessionMessagesAction _sessionMessagesAction;

    public TMP_Text _button1Text;
    public TMP_Text _button2Text;
    public TMP_Text _button3Text;
    public TMP_Text _button4Text;
    public TMP_Text _button5Text;
    public TMP_Text _button6Text;

    // Loading 图像（带 Animator 组件的 GameObject）
    public GameObject _loadingImage;

    private bool _isKickOffComplete = false;
    private bool _isNightComplete = false;
    private bool _isDayComplete = false; // 新增：标记 Day 是否完成
    private List<string> _actorNames = new List<string>();

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_werewolfGamePlayAction != null, "_werewolfGamePlayAction is null");
        Debug.Assert(_sessionMessagesAction != null, "_sessionMessagesAction is null");
        Debug.Assert(_loadingImage != null, "_loadingImage is null");
        
        // 初始状态：隐藏 loading，显示主文本
        SetLoadingState(false);
        
        SetupButtonTexts();
    }

    private void SetupButtonTexts()
    {
        // 从 WerewolfGameContext 获取所有角色的 appearances 和原始名字
        List<string> appearances = WerewolfGameContext.Instance.GetAllActorAppearances();
        List<string> actorNames = WerewolfGameContext.Instance.GetAllActorNames();

        TMP_Text[] buttonTexts = new TMP_Text[]
        {
            _button1Text, _button2Text, _button3Text,
            _button4Text, _button5Text, _button6Text
        };
        
        _actorNames.Clear();

        // 确保使用 appearances[i+1] 时不会越界（第0个为旁白）
        for (int i = 0; i < buttonTexts.Length && i + 1 < appearances.Count; i++)
        {
            if (buttonTexts[i] != null)
            {
                string maskName = ExtractMaskName(appearances[i + 1]); // 按钮显示面具名
                buttonTexts[i].text = maskName;

                // actorNames 使用原始角色名；若没有则回退为面具名
                string actorName = (actorNames != null && actorNames.Count > i + 1)
                    ? actorNames[i + 1]
                    : maskName;

                _actorNames.Add(actorName);
                Debug.Log($"Set button {i + 1} text to: {maskName} (actor: {actorName})");
                Debug.Log($"Actor {i + 1} appearance: {appearances[i + 1]}");
            }
            else
            {
                Debug.LogWarning($"Button {i + 1} text component is null");
            }
        }
    }

    // 新增：按钮回调（在 Inspector 中传入 0..5）
    public void OnClickActorButton(int buttonIndex)
    {
        // Day 阶段后禁用角色按钮功能
        if (_isDayComplete)
        {
            Debug.LogWarning("Actor buttons are disabled after Day phase");
            return;
        }

        if (!_isKickOffComplete && !_isNightComplete)
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

        string actorName = _actorNames[buttonIndex];
        
        // 如果 Night 完成但 Day 未完成，只显示 Night 阶段的消息
        if (_isNightComplete && !_isDayComplete)
        {
            ShowActorMessagesForPhase(actorName, "night");
        }
        else
        {
            // 否则显示所有消息（KickOff 阶段）
            ShowActorMessages(actorName);
        }
    }

    private void ShowActorMessages(string actorName)
    {
        var messages = WerewolfGameContext.Instance.GetMessagesByActor(actorName);
        if (messages == null || messages.Count == 0)
        {
            _mainText.text = $"{actorName} 暂无消息";
            return;
        }

        List<string> displayMessages = new List<string>();
        displayMessages.Add($"=== {actorName} 的消息 ===");
        foreach (var msg in messages)
        {
            string prefix = msg.MessageType == WerewolfGameContext.MessageRecordType.Mind ? "[内心]" : "[发言]";
            displayMessages.Add($"{prefix} {msg.Content}");
        }
        _mainText.text = string.Join("\n", displayMessages);
    }

    // 新增：显示特定阶段的角色消息
    private void ShowActorMessagesForPhase(string actorName, string phase)
    {
        var messages = WerewolfGameContext.Instance.GetMessagesByActorAndPhase(actorName, phase);
        if (messages == null || messages.Count == 0)
        {
            _mainText.text = $"{actorName} 该阶段没有行动";
            return;
        }

        List<string> displayMessages = new List<string>();
        displayMessages.Add($"=== {actorName} 的{phase}阶段消息 ===");
        foreach (var msg in messages)
        {
            string prefix = msg.MessageType == WerewolfGameContext.MessageRecordType.Mind ? "[内心]" : "[发言]";
            displayMessages.Add($"{prefix} {msg.Content}");
        }
        _mainText.text = string.Join("\n", displayMessages);
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
            _mainText.text = "开局失败：请求错误";
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
        SetupButtonTexts(); // 更新按钮绑定的 actor 名称
        
        // 隐藏 loading，显示完成文本
        SetLoadingState(false);
        _mainText.text = "开局已完成\n点击角色按钮查看对应消息";
    }

    private IEnumerator Time()
    {
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

        // 显示结果
        UpdateMainTextByClientMessages(_sessionMessagesAction.ResponseData.session_messages);
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

        // 处理消息（记录已在 GameContext 中做），但 UI 显示为"夜晚行动已完成"
        var processedMessages = WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "night");
        Debug.Log("Night processed messages:\n" + string.Join("\n", processedMessages));

        // 标记 Night 完成
        _isNightComplete = true;

        // 隐藏 loading，显示完成文本
        SetLoadingState(false);
        _mainText.text = "夜晚行动已完成\n点击角色按钮查看夜晚行动细节";
    }

    private IEnumerator Day()
    {
        // 显示 loading 动画，隐藏文本
        SetLoadingState(true);
        // 
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

        // 标记 Day 完成，禁用角色按钮功能
        _isDayComplete = true;

        SetLoadingState(false);

        // 显示结果
        UpdateMainTextByClientMessages(_sessionMessagesAction.ResponseData.session_messages);
    }

    private IEnumerator Vote()
    {
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

        SetLoadingState(false);

        // 显示结果
        UpdateMainTextByClientMessages(_sessionMessagesAction.ResponseData.session_messages);
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
}
