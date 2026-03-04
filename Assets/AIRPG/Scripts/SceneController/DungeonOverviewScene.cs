using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

/// <summary>
/// 地下城概览场景控制器
/// 负责显示地下城的宏观信息，包括关卡列表和怪物预览
/// 提供进入地下城和返回主场景的功能
/// </summary>
public class DungeonOverviewScene : MonoBehaviour
{
    public static readonly string PreSceneName = "MainScene";
    public static readonly string NextSceneName = "DungeonCombatScene2";

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
        LoadDungeonOverview().Forget();
    }

    /// <summary>
    /// UI按钮回调：进入地下城
    /// 触发地下城传送流程，成功后切换到地下城场景
    /// </summary>
    public void OnClickTransDungeon()
    {
        Debug.Log("OnClickTransDungeon");
        EnterDungeon().Forget();
    }

    /// <summary>
    /// 执行进入地下城的流程
    /// 调用传送API，验证响应后切换场景
    /// </summary>
    private async UniTaskVoid EnterDungeon()
    {
        var dungeonTransitionResult = await HomeGamePlayManager.Instance.TransDungeon();
        if (!dungeonTransitionResult)
        {
            _mainText.text = "Trans dungeon failed";
            return;
        }

        var syncErr = await GameStateSync.Instance.RefreshMappingAndEntitiesFromServer();
        if (syncErr != GameSyncError.None)
        {
            Debug.LogError($"[LoginScene] RefreshMappingAndEntitiesFromServer failed: {syncErr}");
            _mainText.text = "Failed to sync game state";
            return;
        }

        await UniTask.Yield();

        // 成功进入地下城后切换到地下城战斗场景
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 加载并显示地下城概览信息
    /// 从服务器刷新地下城数据，并格式化显示在UI上
    /// </summary>
    private async UniTaskVoid LoadDungeonOverview()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Player is not logged in, cannot load dungeon overview");
            _mainText.text = "Player is not logged in";
            return;
        }


        var dungeon = await GameStateSync.Instance.RefreshDungeonFromServer();
        if (dungeon == null)
        {
            Debug.LogError("Failed to refresh dungeon data");
            _mainText.text = "Failed to load dungeon data";
            return;
        }

        _mainText.text = GameUtils.FormatDungeonOverview(GameContext.Instance.Dungeon);
    }

    /// <summary>
    /// UI按钮回调：返回主场景
    /// 触发返回流程
    /// </summary>
    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        ReturnToMainScene().Forget();
    }

    /// <summary>
    /// 执行返回主场景的流程
    /// 验证游戏状态后切换到主场景
    /// </summary>
    private async UniTaskVoid ReturnToMainScene()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            Debug.Log("Returning to MainScene");
            await UniTask.Yield();
            SceneManager.LoadScene(PreSceneName);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in CampScene.");
        }
    }
}
