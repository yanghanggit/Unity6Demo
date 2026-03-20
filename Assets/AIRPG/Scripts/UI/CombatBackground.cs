using Cysharp.Threading.Tasks;
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

        if (!GameContext.Instance.IsLoggedIn)
        {
            // 根据当前角色所在的地下城和关卡，动态更新场景背景图片
            var stageName = MockData.MockStageName; // 这里使用 mock 数据，实际项目中应该根据当前关卡状态获取对应的 stage name
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(stageName);
            if (cachedSprite != null)
            {
                _backgroundImage.sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"DungeonCombatScene: Background sprite not found for stage: {stageName}");
                _backgroundImage.sprite = null;
            }
        }
        else
        {
            RefreshBackgroundFromCachedDungeon().Forget();
        }
    }

    private async UniTaskVoid RefreshBackgroundFromCachedDungeon()
    {
        _backgroundImage.sprite = null;
        var dungeon = DungeonCombatScene.CachedDungeon;
        if (dungeon == null)
        {
            Debug.LogWarning("[CombatBackground] CachedDungeon is null, cannot set background image");
            return;
        }

        var index = dungeon.current_room_index;
        if (index < 0 || index >= dungeon.rooms.Count)
        {
            Debug.LogWarning($"[CombatBackground] current_room_index ({index}) is out of range, cannot set background image");
            return;
        }

        var imageUrl = dungeon.rooms[index].image.url;
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var fullUrl = GameContext.BaseUrl.TrimEnd('/') + imageUrl;
            await SetBackgroundImage(fullUrl);
        }
        else
        {
            var cachedSprite = SpriteCacheManager.Instance.GetSprite(dungeon.rooms[index].stage.name);
            if (cachedSprite != null)
            {
                _backgroundImage.sprite = cachedSprite;
            }
            else
            {
                Debug.LogWarning($"[CombatBackground] No valid image URL for current dungeon room, and no cached sprite found for stage: {dungeon.rooms[index].stage.name}");
                _backgroundImage.sprite = null;
            }
        }
    }
    private async UniTask SetBackgroundImage(string fullUrl)
    {
        if (SpriteCacheManager.Instance == null)
        {
            Debug.LogError("[CombatBackground] SpriteCacheManager instance is not available");
            return;
        }

        if (string.IsNullOrEmpty(fullUrl))
        {
            Debug.LogError("[CombatBackground] URL is null or empty");
            return;
        }

        var texture = await SpriteCacheManager.Instance.LoadRemoteTexture(fullUrl);
        if (texture == null)
        {
            Debug.LogError($"[CombatBackground] Failed to load image: {fullUrl}");
            return;
        }

        // 交由 SpriteCacheManager 持有 Sprite 生命周期，无需手动 Destroy
        // 用 URL 作为 key；HasSprite 防止重复 AddSprite 导致 RemoteTextureCache 中的纹理被意外销毁
        if (!SpriteCacheManager.Instance.HasSprite(fullUrl))
        {
            SpriteCacheManager.Instance.AddSprite(fullUrl, texture);
        }

        _backgroundImage.sprite = SpriteCacheManager.Instance.GetSprite(fullUrl);
    }
}

