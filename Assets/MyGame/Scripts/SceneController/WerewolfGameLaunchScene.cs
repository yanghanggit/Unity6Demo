using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

// Launch
public class WerewolfGameLaunchScene : MonoBehaviour
{
    public string serverUrl = "http://192.168.192.54:8000/";

    public RootAction _rootAction;

    public WerewolfGameStartAction _werewolfGameStartAction;

    public WerewolfGameStateAction _werewolfGameStateAction;

    public WerewolfGameActorDetailsAction _werewolfGameActorDetailsAction;

    public Button _nextButton;

    void Start()
    {
        Debug.Assert(_rootAction != null, "_bootAction is null");
        Debug.Assert(_werewolfGameStartAction != null, "_werewolfGameStartAction is null");
        Debug.Assert(_werewolfGameActorDetailsAction != null, "_werewolfGameActorDetailsAction is null");
        Debug.Assert(_werewolfGameStateAction != null, "_werewolfGameStateAction is null");
        Debug.Assert(_nextButton != null, "_nextButton is null");

        _nextButton.gameObject.SetActive(false);
        StartCoroutine(LoadApiEndpoints());
    }

    public void OnClickNext()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadApiEndpoints()
    {
        //隐藏按钮
        _nextButton.gameObject.SetActive(false);

        //加载 Root API endpoints
        yield return _rootAction.Call(serverUrl);
        if (!_rootAction.LastRequestSuccess)
        {
            Debug.LogError("Failed to load Root API endpoints.");
            yield break;
        }

        // 设置游戏上下文，内含 URL 配置
        WerewolfGameContext.Instance.Root = _rootAction.RootResponse;

        //设置下
        _werewolfGameStartAction.Setup(
            WerewolfGameContext.Instance.StartUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName
        );

        // 发起开始游戏请求
        yield return _werewolfGameStartAction.Call();
        if (_werewolfGameStartAction.ResponseData == null)
        {
            Debug.LogError("Failed to start Werewolf game.");
            yield break;
        }

        // 设置游戏状态请求
        _werewolfGameStateAction.Setup(
            WerewolfGameContext.Instance.StateUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName
        );
        // 发起游戏状态请求
        yield return _werewolfGameStateAction.Call();
        if (_werewolfGameStateAction.ResponseData == null)
        {
            Debug.LogError("Failed to get Werewolf game state.");
            yield break;
        }

        // 设置游戏上下文
        Debug.Assert(_werewolfGameStateAction.ResponseData.mapping != null, "Mapping is null in Werewolf game state response.");
        Debug.Assert(_werewolfGameStateAction.ResponseData.mapping.Count == 1, "Mapping count is not correct in Werewolf game state response.");

        // 获取唯一的场景与场景中的角色列表
        string uniqueKey = "";
        List<string> uniqueValue = new List<string>();
        foreach (var kv in _werewolfGameStateAction.ResponseData.mapping)
        {
            uniqueKey = kv.Key;
            uniqueValue = kv.Value;
            break;
        }

        WerewolfGameContext.Instance.UpdateGameState(
            _werewolfGameStateAction.ResponseData.game_time,
            uniqueValue,
            uniqueKey
            );

        // 设置查看角色请求
        _werewolfGameActorDetailsAction.Setup(
            WerewolfGameContext.Instance.ActorDetailsUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName,
            uniqueValue
        );

        // 发起查看角色请求
        yield return _werewolfGameActorDetailsAction.Call();
        if (_werewolfGameActorDetailsAction.ResponseData == null)
        {
            Debug.LogError("Failed to get Werewolf game actor details.");
            yield break;
        }

        Debug.Log("Werewolf game setup complete.");

        // 设置角色实体到游戏上下文
        WerewolfGameContext.Instance.UpdateActorEntities(
            _werewolfGameActorDetailsAction.ResponseData.actor_entities_serialization
        );

        // 开界面，可以进行下一步
        _nextButton.gameObject.SetActive(true);
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        //SceneManager.LoadScene(_nextScene);
    }

}
