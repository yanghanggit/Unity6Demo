using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// 地下城概览场景控制器
/// 负责显示地下城的宏观信息，包括关卡列表和怪物预览
/// 提供进入地下城和返回主场景的功能
/// </summary>
public class DungeonOverviewScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string _preScene = "MainScene";
    [SerializeField] private string _nextScene = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText;

    /// <summary>
    /// 场景初始化
    /// 验证必要组件并加载地下城概览数据
    /// </summary>
    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");

        _mainText.text = "Loading dungeon data...";

        if (ApiEndpointsManager.GameRootResponse != null)
        {
            StartCoroutine(LoadDungeonOverview());
        }

    }

    /// <summary>
    /// UI按钮回调：进入地下城
    /// 触发地下城传送流程，成功后切换到地下城场景
    /// </summary>
    public void OnClickTransDungeon()
    {
        Debug.Log("OnClickTransDungeon");
        StartCoroutine(EnterDungeon());
    }

    /// <summary>
    /// 执行进入地下城的流程
    /// 调用传送API，验证响应后切换场景
    /// </summary>
    private IEnumerator EnterDungeon()
    {
        bool success = false;

        // 1. 调用传送地下城API
        yield return HomeGamePlayManager.Instance.TransDungeon(
            (result) =>
            {
                success = result;
                if (!result)
                {
                    _mainText.text = "Trans dungeon failed";
                }
            });

        if (!success)
        {
            yield break;
        }

        // 2. 刷新全局游戏状态
        yield return GameStateSync.Instance.RefreshMappingAndEntitiesFromServer();

        // 设置待进入的关卡名称为地下城的第一个关卡
        //DungeonCombatScene.PendingStageName = GameContext.Instance.Dungeon.stages[0].name;

        // 3. 切换到地下城场景
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// 加载并显示地下城概览信息
    /// 从服务器刷新地下城数据，并格式化显示在UI上
    /// </summary>
    private IEnumerator LoadDungeonOverview()
    {
        yield return GameStateSync.Instance.RefreshDungeonFromServer();

        _mainText.text = GameUtils.FormatDungeonOverview(GameContext.Instance.Dungeon);
    }

    /// <summary>
    /// UI按钮回调：返回主场景
    /// 触发返回流程
    /// </summary>
    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToMainScene());
    }

    /// <summary>
    /// 执行返回主场景的流程
    /// 验证游戏状态后切换到主场景
    /// </summary>
    private IEnumerator ReturnToMainScene()
    {
        if (ApiEndpointsManager.GameRootResponse != null)
        {
            Debug.Log("Returning to MainScene");
            yield return new WaitForSeconds(0);
            SceneManager.LoadScene(_preScene);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in CampScene.");
        }
    }
}
