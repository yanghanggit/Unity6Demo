using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameWorldController : MonoBehaviour
{
    [Header("可点击的对象列表")]
    [SerializeField] private GameObject[] _clickableObjects;

    private const string GameStageSceneName = "GameStage";

    private const string MockNextSceneName = "TestLanding";

    /// <summary>每个可点击对象对应的场景（Stage）名称，索引与 _clickableObjects 对齐。</summary>
    private List<string> _stageNamesByIndex = new();

    void Start()
    {
        Debug.Log("GameWorldController started");
        Debug.Assert(_clickableObjects != null && _clickableObjects.Length == 2, "[GameWorldController] _clickableObjects must have exactly 2 elements.");

        if (GameManager.Instance.IsServerConnected)
        {
            FetchAndApplyStagesStateAsync().Forget(); // 正式流程：从服务器拉取场景状态
        }
        else
        {
            ApplyMockStagesState(); // 模拟流程：使用假数据（用于测试）
        }
    }

    /// <summary>
    /// 正式流程：向服务器请求场景状态（对应 FetchStagesStateAsync），
    /// 然后把每个场景的 "key: value" 显示到对应的可点击对象上。
    /// </summary>
    private async UniTaskVoid FetchAndApplyStagesStateAsync()
    {
        var session = GameManager.Instance.Session;
        if (session == null)
        {
            Debug.LogWarning("[GameWorldController] No active session, fallback to mock stages state.");
            ApplyMockStagesState();
            return;
        }

        try
        {
            var response = await GameManager.Instance.ServerClient.FetchStagesStateAsync(session.UserName, session.GameName);
            ApplyStagesStateToLabels(response);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameWorldController] FetchStagesStateAsync failed: {e.Message}");
        }
    }

    /// <summary>
    /// 模拟流程：构造一份假的 StagesStateResponse，用于在没有服务器连接时测试显示逻辑。
    /// </summary>
    private void ApplyMockStagesState()
    {
        var mockResponse = new StagesStateResponse
        {
            mapping = new Dictionary<string, List<string>>
            {
                { "Scene1", new List<string> { "Hero", "Slime" } },
                { "Scene2", new List<string> { "Merchant" } },
            }
        };

        ApplyStagesStateToLabels(mockResponse);
    }

    /// <summary>
    /// 遍历 _clickableObjects，把 StagesStateResponse.mapping 中对应的 "key: value" 整段
    /// 设置给每个对象上的 WorldLabel.Text。
    /// </summary>
    private void ApplyStagesStateToLabels(StagesStateResponse response)
    {
        var keys = response.mapping.Keys.ToList();
        _stageNamesByIndex = keys;

        for (int i = 0; i < _clickableObjects.Length; i++)
        {
            var worldLabel = _clickableObjects[i].GetComponent<WorldLabel>();
            if (worldLabel == null) continue;

            if (i < keys.Count)
            {
                var key = keys[i];
                var value = response.mapping[key];
                var displayKey = EntityNameUtils.GetDisplayName(key);
                var displayValues = value.Select(EntityNameUtils.GetDisplayName);
                worldLabel.Text = $"{displayKey}: {string.Join(", ", displayValues)}";
            }
        }
    }

    /// <summary>
    /// 当场景中的某个可点击对象被点击时调用，sceneName 对应被点击对象的场景名称。
    /// </summary>
    /// <param name="sceneName">被点击对象的场景名称</param>
    public void OnClick(int index)
    {
        Debug.Log("Scene clicked: " + index);

        // 记录被点击对象对应的场景名称，供进入 GameStage 场景后的 GameStageController 读取。
        GameManager.Instance.CurrentStageName = index < _stageNamesByIndex.Count ? _stageNamesByIndex[index] : "";

        if (GameManager.Instance.IsServerConnected)
        {
            EnterHomeSceneAsync().Forget(); // 正常流程：启动新游戏
        }
        else
        {
            MockEnterNextSceneAsync().Forget(); // 模拟新游戏创建成功（用于测试）
        }
    }

    /// <summary>
    /// 进入主场景（HomeScene）的异步方法。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid EnterHomeSceneAsync()
    {
        //_button.SetEnabled(false);
        await SceneManager.LoadSceneAsync(GameStageSceneName);
    }

    /// <summary>
    /// 模拟进入下一个场景（TestLanding）的异步方法。
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid MockEnterNextSceneAsync()
    {
        // 模拟新游戏创建成功
        await UniTask.Delay(0);
        // 跳转到下一个场景
        await SceneManager.LoadSceneAsync(MockNextSceneName);
    }

    
}
