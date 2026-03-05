using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatPostCombatState : MonoBehaviour, ICombatState
{
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
            _mainText.text = "战斗结束！正在处理战斗结果...";

            // 调用服务器接口进行战斗后处理，并根据返回的结果更新 UI 显示
            OnEnterSync().Forget();
        }
    }

    private async UniTaskVoid OnEnterSync()
    {
        // 显示战斗结果文本
        Debug.Log("Entered Post Combat State");

        var sessionMessages = await DungeonGamePlayManager.Instance.PostCombat();
        if (sessionMessages == null)
        {
            Debug.LogWarning("Failed to get session messages from post combat");
            _mainText.text = "战斗结束！(未能获取战斗结果)";
            return;
        }

        // 然后逐个处理返回的 SessionMessage，特别是 CombatArchiveEvent
        var showText = "战斗后事件：\n\n";

        for (int i = 0; i < sessionMessages.Count; i++)
        {
            SessionMessage sessionMessage = sessionMessages[i];
            if (sessionMessage.message_type != (int)MessageType.AGENT_EVENT)
            {
                continue;
            }

            var agentEvent = GameUtils.ParseAgentEvent(sessionMessage);
            if (agentEvent == null)
            {
                Debug.LogWarning("Failed to parse agent event from session message");
                continue;
            }

            if (agentEvent.head == (int)EventHead.COMBAT_ARCHIVE_EVENT)
            {
                Debug.Log("Processing CombatArchiveEvent from post combat");
                if (agentEvent is CombatArchiveEvent combatArchiveEvent)
                {
                    showText += $"Actor: {combatArchiveEvent.actor}\nSummary: {combatArchiveEvent.summary}\n\n";
                }
            }
        }

        //
        _mainText.text = showText;
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

        if (dungeon.current_stage_index >= dungeon.stages.Count - 1)
        {
            Debug.LogWarning("Already at the last stage of the dungeon, cannot advance further");
            _mainText.text = "已经是地下城的最后一关了！";
            return;
        }

        var responseSessionMessages = await DungeonGamePlayManager.Instance.AdvanceStage();
        if (responseSessionMessages == null)
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
        SceneManager.LoadScene(NextSceneName);
    }
}
