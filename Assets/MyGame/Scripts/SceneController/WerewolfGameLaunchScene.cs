using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

// Launch
public class WerewolfGameLaunchScene : MonoBehaviour
{
    public string serverUrl = "http://192.168.192.54:8000/";

    public RootApi _rootAction;

    public WerewolfGameStartAction _werewolfGameStartAction;

    public WerewolfGameStateAction _werewolfGameStateAction;

    public ActorDetailsApi _actorDetailsAction;

    public StagesStateApi _stagesStateAction;

    // 游戏模式选择按钮
    public Button _playModeButton;   // 游玩模式按钮
    public Button _debugModeButton;  // 调试模式按钮

    void Start()
    {
        Debug.Assert(_rootAction != null, "_bootAction is null");
        Debug.Assert(_werewolfGameStartAction != null, "_werewolfGameStartAction is null");
        Debug.Assert(_actorDetailsAction != null, "_werewolfGameActorDetailsAction is null");
        Debug.Assert(_werewolfGameStateAction != null, "_werewolfGameStateAction is null");
        Debug.Assert(_stagesStateAction != null, "_stagesStateAction is null");
        Debug.Assert(_playModeButton != null, "_playModeButton is null");
        Debug.Assert(_debugModeButton != null, "_debugModeButton is null");

        _playModeButton.gameObject.SetActive(false);
        _debugModeButton.gameObject.SetActive(false);
        StartCoroutine(SetupGameApiServices());
    }

    /// <summary>
    /// 点击游玩模式按钮
    /// </summary>
    public void OnClickPlayMode()
    {
        WerewolfGameContext.Instance.IsDebugMode = false;
        Debug.Log("Selected Play Mode (只显示发言)");
        StartCoroutine(LoadNextScene());
    }

    /// <summary>
    /// 点击调试模式按钮
    /// </summary>
    public void OnClickDebugMode()
    {
        WerewolfGameContext.Instance.IsDebugMode = true;
        Debug.Log("Selected Debug Mode (显示所有消息)");
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator SetupGameApiServices()
    {
        //隐藏按钮
        _playModeButton.gameObject.SetActive(false);
        _debugModeButton.gameObject.SetActive(false);

        // Pipeline: 分段逐步处理
        yield return LoadRootApiEndpoints();
        yield return StartWerewolfGame();
        yield return LoadGameState();
        yield return UpdateGameStateContext();
        yield return LoadActorDetails();
        yield return UpdateActorEntitiesContext();

        Debug.Log("Werewolf game setup complete.");

        // 显示模式选择按钮
        _playModeButton.gameObject.SetActive(true);
        _debugModeButton.gameObject.SetActive(true);
    }

    private IEnumerator LoadRootApiEndpoints()
    {
        //加载 Root API endpoints
        yield return _rootAction.Call(serverUrl);
        if (_rootAction.RespData == null)
        {
            Debug.LogError("Failed to load Root API endpoints.");
            yield break;
        }

        // 设置游戏上下文,内含 URL 配置
        WerewolfGameContext.Instance.Root = _rootAction.RespData;
    }

    private IEnumerator StartWerewolfGame()
    {
        // 发起开始游戏请求
        yield return _werewolfGameStartAction.Call(
            WerewolfGameContext.Instance.StartUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName);

        if (_werewolfGameStartAction.ResponseData == null)
        {
            Debug.LogError("Failed to start Werewolf game.");
            yield break;
        }
    }

    private IEnumerator LoadGameState()
    {
        //
        yield return _stagesStateAction.Call(
            WerewolfGameContext.Instance.StagesStateUrl);
        if (_stagesStateAction.RespData == null)
        {
            Debug.LogError("Failed to get Stages state.");
            yield break;
        }

        // 发起游戏状态请求
        yield return _werewolfGameStateAction.Call(WerewolfGameContext.Instance.StateUrl,
            WerewolfGameContext.Instance.UserName,
            WerewolfGameContext.Instance.GameName);
        if (_werewolfGameStateAction.ResponseData == null)
        {
            Debug.LogError("Failed to get Werewolf game state.");
            yield break;
        }
    }

    private IEnumerator UpdateGameStateContext()
    {
        // 设置游戏上下文
        Debug.Assert(_stagesStateAction.RespData.mapping != null, "Mapping is null in Werewolf game state response.");
        Debug.Assert(_stagesStateAction.RespData.mapping.Count == 1, "Mapping count is not correct in Werewolf game state response.");

        //获取唯一的场景与场景中的角色列表
        string uniqueKey = "";
        List<string> uniqueValue = new List<string>();
        foreach (var kv in _stagesStateAction.RespData.mapping)
        {
            uniqueKey = kv.Key;
            uniqueValue = kv.Value;
            break;
        }

        WerewolfGameContext.Instance.UpdateGameState(
            _werewolfGameStateAction.ResponseData.game_time,
            uniqueValue,
            uniqueKey,
            _werewolfGameStateAction.ResponseData.victory_condition,
            _werewolfGameStateAction.ResponseData.is_discussion_complete
        );

        yield return null;
    }

    private IEnumerator LoadActorDetails()
    {
        // 从游戏状态中获取角色列表
        string uniqueKey = "";
        List<string> uniqueValue = new List<string>();
        foreach (var kv in _stagesStateAction.RespData.mapping)
        {
            uniqueKey = kv.Key;
            uniqueValue = kv.Value;
            break;
        }

        Debug.Log($"Loading actor details for key: {uniqueKey}, actors: {string.Join(", ", uniqueValue)}");

        // 发起查看角色请求
        yield return _actorDetailsAction.Call(WerewolfGameContext.Instance.ActorDetailsUrl,
            uniqueValue);
        if (_actorDetailsAction.RespData == null)
        {
            Debug.LogError("Failed to get Werewolf game actor details.");
            yield break;
        }
    }

    private IEnumerator UpdateActorEntitiesContext()
    {
        // 设置角色实体到游戏上下文
        WerewolfGameContext.Instance.UpdateActorEntities(
            _actorDetailsAction.RespData.actor_entities_serialization
        );

        yield return null;
    }
    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene("WerewolfGamePlayScene");
    }

}
