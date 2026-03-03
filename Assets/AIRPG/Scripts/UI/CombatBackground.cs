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
        UpdateImage();
    }

    /// <summary>
    /// 根据当前角色所在的地下城和关卡，动态更新场景背景图片
    /// </summary>
    private void UpdateImage()
    {
        if (GameContext.Instance.IsLoggedIn)
        {
            var stageName = GameContext.Instance.GetActorStage(GameContext.Instance.PlayerActorName);
            Debug.Assert(stageName != "", "[GameStateSync] Current actor's stage name is empty");
            // 获取当前角色所在场景
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
        else
        {
            Debug.LogWarning("DungeonCombatScene: Player is not logged in, cannot update background image");

            var mockStageName = "场景.山林边缘";
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(mockStageName);
            if (cachedSprite != null)
            {
                _backgroundImage.GetComponent<Image>().sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"DungeonCombatScene: Background sprite not found for mock stage: {mockStageName}");
                _backgroundImage.GetComponent<Image>().sprite = null;
            }
        }
    }
}

