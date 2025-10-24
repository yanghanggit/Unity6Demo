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
            _mainText.text = "当前阶段未设置";
        }
    }

    // 显示特定阶段的角色消息
    private void ShowActorMessagesForPhase(string actorName, string phase)
    {
        var messages = WerewolfGameContext.Instance.GetMessagesByActorAndPhase(actorName, phase);
        if (messages == null || messages.Count == 0)
        {
            _mainText.text = $"{actorName} 在 {GetPhaseFriendlyName(phase)} 阶段没有消息";
            return;
        }

        List<string> displayMessages = new List<string>();
        displayMessages.Add($"=== {actorName} 的 {GetPhaseFriendlyName(phase)} 阶段消息 ===");
        
        AppendMessagesWithPrefix(displayMessages, messages);
        
        _mainText.text = string.Join("\n", displayMessages);
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

        // 记录处理前的消息数量
        int messageCountBefore = WerewolfGameContext.Instance.MessageRecords.Count;

        // 处理消息以更新阶段信息（会添加到记录中）
        WerewolfGameContext.Instance.ConvertClientMessagesToText(
            _sessionMessagesAction.ResponseData.session_messages, "day");

        SetLoadingState(false);
        
        // 只显示本次新增的消息
        ShowNewlyAddedMessages(messageCountBefore);
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
