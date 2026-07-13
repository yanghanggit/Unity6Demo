using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
//using UnityEngine.SceneManagement;

public class GameStageController : MonoBehaviour
{

    [Header("可点击的对象列表")]
    [SerializeField] private GameObject[] _clickableObjects;

    void Start()
    {
        Debug.Log("GameStageController started");
        Debug.Assert(_clickableObjects != null && _clickableObjects.Length == 2, "[GameStageController] _clickableObjects must have exactly 2 elements.");

        if (GameManager.Instance.IsServerConnected)
        {
            FetchAndApplyActorsAsync().Forget(); // 正式流程：拉取当前场景内的角色名单
        }
        else
        {
            ApplyMockActors(); // 模拟流程：使用假数据（用于测试）
        }
    }

    /// <summary>
    /// 正式流程：先根据 GameManager.CurrentStageName（GameWorldController 点击时记录）
    /// 从 StagesStateResponse.mapping 中取出当前场景内的角色名单，
    /// 再调用一次 FetchEntitiesDetailsAsync 获取详情（用于确认该接口可用），
    /// 最后把角色的显示名分别设置给 _clickableObjects。
    /// </summary>
    private async UniTaskVoid FetchAndApplyActorsAsync()
    {
        var session = GameManager.Instance.Session;
        var stageName = GameManager.Instance.CurrentStageName;

        if (session == null || string.IsNullOrEmpty(stageName))
        {
            Debug.LogWarning("[GameStageController] No active session or stage name, fallback to mock actors.");
            return;
        }

        try
        {
            var stagesState = await GameManager.Instance.ServerClient.FetchStagesStateAsync(session.UserName, session.GameName);
            if (!stagesState.mapping.TryGetValue(stageName, out var actorNames))
            {
                Debug.LogWarning($"[GameStageController] Stage '{stageName}' not found in stages state.");
                actorNames = new List<string>();
            }

            // 调用一次获取详情接口，确认其可用性。
            if (actorNames.Count > 0)
            {
                var detailsResponse = await GameManager.Instance.ServerClient.FetchEntitiesDetailsAsync(
                    session.UserName, session.GameName, actorNames);
                Debug.Log($"[GameStageController] FetchEntitiesDetailsAsync ok, got {detailsResponse.entities_serialization.Count} entities.");
            }

            ApplyActorsToLabels(actorNames);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameStageController] Fetch actors failed: {e.Message}");
        }
    }

    /// <summary>
    /// 模拟流程：构造一份假的角色名单，用于在没有服务器连接时测试显示逻辑。
    /// </summary>
    private void ApplyMockActors()
    {
        var mockActorNames = new List<string> { "旅行者.A", "怪物.B" };
        ApplyActorsToLabels(mockActorNames);
    }

    /// <summary>
    /// 遍历 _clickableObjects，把角色名单中对应的显示名设置给每个对象上的 WorldLabel.Text。
    /// </summary>
    private void ApplyActorsToLabels(List<string> actorNames)
    {
        for (int i = 0; i < _clickableObjects.Length; i++)
        {
            if (!_clickableObjects[i].TryGetComponent<WorldLabel>(out var worldLabel)) continue;

            if (i < actorNames.Count)
            {
                worldLabel.Text = EntityNameUtils.GetDisplayName(actorNames[i]);
            }
        }
    }

    public void OnClick(int index)
    {
        Debug.Log("Actor clicked: " + index);

        if (GameManager.Instance.IsServerConnected)
        {
            Debug.Log("Server is connected, would enter home scene.");
        }
        else
        {
            Debug.Log("Server is not connected, would mock enter next scene.");
        }
    }
}

