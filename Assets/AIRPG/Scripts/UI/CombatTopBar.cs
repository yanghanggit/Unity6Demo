using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

/// <summary>
/// 地牢战斗场景顶部UI控制器
/// </summary>
public class CombatTopBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _infoText; // 战斗信息显示对象

    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(_infoText != null, "_infoText is null");
        _infoText.text = FormatCombatInfo(DungeonCombatScene.CachedDungeon);
    }

    /// <summary>
    /// 刷新战斗状态显示，包含当前地下城、关卡和回合数等信息
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid RefreshCombatStatusAsync()
    {
        if (!GameContext.Instance.IsLoggedIn)
        {
            Debug.LogWarning("CombatOnGoingState: Player is not logged in, using mock data to refresh combat info");
            // 使用 mock 数据来刷新顶部信息显示
            _infoText.text = "Mock Dungeon | Mock Stage | 回合数: 1";
            return;
        }

        var combat = await GameStateSync.Instance.GetCombat();
        if (combat == null || combat.rounds == null)
        {
            Debug.LogError("CombatOnGoingState: Combat data is null or rounds data is null, cannot refresh combat view");
            return;
        }

        // 刷新顶部信息显示，包含当前地下城、关卡和回合数等信息
        _infoText.text = $"{FormatCombatInfo(DungeonCombatScene.CachedDungeon)} | 回合数: {combat.rounds.Count}";
    }

    /// <summary>
    /// 格式化战斗信息显示文本，包含当前地下城和关卡信息
    /// </summary>
    private string FormatCombatInfo(Dungeon dungeon)
    {
        var text = $"{dungeon.name}";
        if (dungeon.rooms.Count > 0 && dungeon.current_room_index >= 0 && dungeon.current_room_index < dungeon.rooms.Count)
        {
            var dungeonRoom = dungeon.rooms[dungeon.current_room_index];
            var stageName = dungeonRoom != null ? dungeonRoom.stage.name : "Unknown Stage";
            text += $" | {stageName}";
        }

        return text;
    }

}

