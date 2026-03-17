using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatPostCombatState : MonoBehaviour, ICombatState
{
    public static readonly string PreSceneName = "MainScene";
    public static readonly string NextSceneName = "DungeonCombatScene";

    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 战斗后面板文本对象

    public ICombatScene CombatScene { get; set; }

    void Start()
    {
        Debug.Assert(_mainText != null, "_mainText is null");
    }

    /// <summary>
    /// 点击继续按钮的处理逻辑，根据新的战斗状态切换 UI 显示和交互逻辑
    /// </summary>
    public void OnClickAdvanceButton()
    {
        Debug.Log("Advance Button Clicked");
        ExecuteAdvanceStage().Forget();  // 在这里添加点击继续按钮后的逻辑，例如返回主界面或进入下一个关卡
    }

    public void OnClickExitButton()
    {
        Debug.Log("Exit Button Clicked");
        //SceneManager.LoadScene("HomeScene"); // 返回主界面
        ExecuteExitDungeon().Forget();
    }

    public void OnEnter()
    {
        // 显示战斗结果文本
        Debug.Log("Entered Post Combat State");

        if (!GameContext.Instance.IsLoggedIn)
        {
            _mainText.text = "战斗结束！(模拟数据)";
        }
        else
        {
            _mainText.text = "战斗结束！";
        }
    }

    /// <summary>
    /// 点击继续按钮的处理逻辑，根据新的战斗状态切换 UI 显示和交互逻辑
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ExecuteAdvanceStage()
    {

        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not in dungeon, cannot advance to next stage");
            await UniTask.Yield();
            SceneManager.LoadScene(NextSceneName);
            return;
        }

        var dungeon = await GameStateSync.Instance.GetDungeon();
        if (dungeon == null)
        {
            Debug.LogError("Failed to get dungeon information from server");
            _mainText.text = "Failed to get dungeon information";
            return;
        }

        if (dungeon.current_room_index >= dungeon.rooms.Count - 1)
        {
            Debug.LogWarning("Already at the last stage of the dungeon, cannot advance further");
            _mainText.text = "已经是地下城的最后一关了！";
            return;
        }

        var response = await DungeonGamePlayManager.Instance.AdvanceStage();
        if (response == null)
        {
            Debug.LogWarning("Failed to advance to next dungeon, no messages returned");
            _mainText.text = "推进下一关失败！";
            return;
        }

        var stagesState = await GameStateSync.Instance.GetStagesState();
        if (stagesState == null)
        {
            Debug.LogError("[HomeScene] Failed to get stages state from server");
            _mainText.text = "Failed to get stage information";
            return;
        }

        var targetStageName = string.Empty;
        foreach (var kvp in stagesState)
        {
            if (kvp.Value.Contains(GameContext.Instance.PlayerActorName))
            {
                targetStageName = kvp.Key;
                break;
            }
        }

        await UniTask.Yield();

        DungeonCombatScene.CachedStageName = targetStageName;
        DungeonCombatScene.CachedDungeonName = dungeon.name;

        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// 退出地下城的处理逻辑
    /// 检查战斗状态，调用退出地下城接口，清空缓存并返回主场景
    /// </summary>
    private async UniTaskVoid ExecuteExitDungeon()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("Not in dungeon, cannot advance to next stage");
            await UniTask.Yield();
            SceneManager.LoadScene(NextSceneName);
            return;
        }

        var combatState = await GameStateSync.Instance.GetCombat();
        if (combatState == null)
        {
            Debug.LogError("Failed to get combat state information from server");
            _mainText.text = "Failed to get combat state information";
            return;
        }

        if (combatState.state != CombatState.POST_COMBAT)
        {
            Debug.LogWarning("Combat is not finished, cannot exit dungeon");
            _mainText.text = "战斗还未结束，无法退出地下城！";
            return;
        }

        var response = await DungeonGamePlayManager.Instance.ExitDungeon();
        if (response == null)
        {
            Debug.LogWarning("Failed to exit dungeon, no messages returned");
            _mainText.text = "退出地下城失败！";
            return;
        }

        await UniTask.Yield();

        DungeonCombatScene.CachedStageName = string.Empty; // 退出地下城后不再缓存关卡信息
        DungeonCombatScene.CachedDungeonName = string.Empty; // 退出地下城后不再缓存地下城信息

        SceneManager.LoadScene(PreSceneName);
    }
}
