using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地牢战斗场景背景控制器
/// </summary>
public class CombatBackground : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _backgroundImage; // 场景背景图片

    void Start()
    {
        // 断言检查，确保所有必要的组件和数据都已正确设置
        Debug.Assert(_backgroundImage != null, "_backgroundImage is null");

        // 根据当前角色所在的地下城和关卡，动态更新场景背景图片
        var stageName = GameContext.Instance.IsLoggedIn ? DungeonCombatScene2.StageName : MockData.MockStageName;
        var cachedSprite = SpriteCacheManager.Instance.GetSprite(stageName);
        if (cachedSprite != null)
        {
            _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
        }
        else
        {
            Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {stageName}");
            _backgroundImage.GetComponent<Image>().sprite = null;
        }
    }
}

