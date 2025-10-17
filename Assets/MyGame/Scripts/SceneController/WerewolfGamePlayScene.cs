using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
//using System;

public class WerewolfGamePlayScene : MonoBehaviour
{
    public TMP_Text _mainText;

    public WerewolfGamePlayAction _werewolfGamePlayAction;

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_werewolfGamePlayAction != null, "_werewolfGamePlayAction is null");
    }


    public void OnClickKickOff()
    {
        Debug.Log("OnClickKickOff");
        StartCoroutine(KickOff());
    }

    public void OnClickTime()
    {
        Debug.Log("OnClickTime");
    }

    public void OnClickNight()
    {
        Debug.Log("OnClickNight");
    }

    public void OnClickDay()
    {
        Debug.Log("OnClickDay");
    }

    public void OnClickVote()
    {
        Debug.Log("OnClickVote");
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

        // 显示结果
        _mainText.text = "";
        for (int i = 0; i < _werewolfGamePlayAction.ResponseData.client_messages.Count; i++)
        {
            var message = _werewolfGamePlayAction.ResponseData.client_messages[i];
            Debug.Log($"Client Message {i}: " + JsonUtility.ToJson(message));
            _mainText.text += JsonUtility.ToJson(message) + "\n";
        }
    }
}
