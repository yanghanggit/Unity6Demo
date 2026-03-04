using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatPostCombatState : MonoBehaviour, ICombatState
{
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
        gameObject.SetActive(false);
        // 在这里添加点击继续按钮后的逻辑，例如返回主界面或进入下一个关卡
        ExecuteAdvanceNext().Forget();
    }

    public void OnEnter()
    {
        // 显示战斗结果文本
        Debug.Log("Entered Post Combat State");
    }


    private async UniTaskVoid ExecuteAdvanceNext()
    {
        // var messages = await DungeonGamePlayManager.Instance.AdvanceNextDungeon();
        // if (messages == null)
        // {
        //     Debug.LogWarning("Failed to advance to next dungeon, no messages returned");
        //     return;
        // }

        // var syncErr = await GameStateSync.Instance.RefreshCombatStateFromServer();
        // if (syncErr != GameSyncError.None)
        // {
        //     Debug.LogError($"[DungeonCombatScene] Failed to refresh dungeon and actors data: {syncErr}");
        //     return;
        // }

        await UniTask.Yield();
        SceneManager.LoadScene("DungeonCombatScene2");
    }

}
