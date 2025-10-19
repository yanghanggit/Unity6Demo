using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WerewolfGamePlayScene : MonoBehaviour
{
    public TMP_Text _mainText;

    public WerewolfGamePlayAction _werewolfGamePlayAction;

    public SessionMessagesAction _sessionMessagesAction;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_werewolfGamePlayAction != null, "_werewolfGamePlayAction is null");
        Debug.Assert(_sessionMessagesAction != null, "_sessionMessagesAction is null");
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
        // 设置请求参数
        _werewolfGamePlayAction.Setup(
            WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/kickoff" } }
        );

        // 发送请求
        yield return _werewolfGamePlayAction.Call();
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

    private IEnumerator Time()
    {
        _werewolfGamePlayAction.Setup(
            WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/time" } }
        );

        yield return _werewolfGamePlayAction.Call();
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
        _werewolfGamePlayAction.Setup(
           WerewolfGameContext.Instance.GameplayUrl,
           WerewolfGameContext.Instance.UserName,
           WerewolfGameContext.Instance.GameName,
           new Dictionary<string, string>
           { { "user_input", "/night" } }
       );

        yield return _werewolfGamePlayAction.Call();
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

    private IEnumerator Day()
    {
        _werewolfGamePlayAction.Setup(
            WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/day" } }
        );

        yield return _werewolfGamePlayAction.Call();
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

    private IEnumerator Vote()
    {
        _werewolfGamePlayAction.Setup(
            WerewolfGameContext.Instance.GameplayUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            new Dictionary<string, string>
            { { "user_input", "/vote" } }
        );

        yield return _werewolfGamePlayAction.Call();
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

    private void UpdateMainTextByClientMessages(List<SessionMessage> messages)
    {
        _mainText.text = "";
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            Debug.Log($"Client Message {i}: " + JsonUtility.ToJson(message));
            _mainText.text += JsonUtility.ToJson(message) + "\n";
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
