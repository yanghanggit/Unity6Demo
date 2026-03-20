using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonOverviewDetailPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _enterDungeonButton; // 进入地下城的按钮
    [SerializeField] private TMP_Text _mainText; // 显示地下城详细信息的文本组件
    [SerializeField] private Image _backgroundImage; // 显示地下城相关图片的背景组件

    void Start()
    {
        Debug.Assert(_enterDungeonButton != null, "_enterDungeonButton is null");
        Debug.Assert(_mainText != null, "_mainText is null");
        Debug.Assert(_backgroundImage != null, "_backgroundImage is null");
        Debug.Assert(_backgroundImage.sprite == null, "_backgroundImage should start with no sprite");

        // 设置初始状态
        _enterDungeonButton.GetComponentInChildren<TMP_Text>().text = "Enter Dungeon";
        _mainText.text = string.Empty;
    }

    /// <summary>
    /// 关闭详情面板
    /// </summary>
    public void OnClickCloseButton()
    {
        gameObject.SetActive(false);
    }

    public async UniTaskVoid OnRefreshView(string dungeonName)
    {
        Debug.Log($"DungeonOverviewDetailPanel refreshing view for dungeon: {dungeonName}");

        await UniTask.Yield(); // 等待一帧，确保 UI 已经更新

        var dungeon = DungeonOverviewScene.CachedDungeonOverviews.Find(d => d.name == dungeonName);
        if (dungeon == null)
        {
            Debug.LogError($"Dungeon with name {dungeonName} not found in cached overviews");
            return;
        }

        // 更新进入地下城按钮的文本，实际项目中可以根据 dungeon 数据动态设置
        _enterDungeonButton.GetComponentInChildren<TMP_Text>().text = $"Enter {dungeonName}";

        // 将 dungeon 数据结构json化后显示在界面上，实际项目中应该格式化显示
        _mainText.text = JsonUtility.ToJson(dungeon, true);

        // 设置背景图片，实际项目中应该根据 dungeon 数据中的图片 URL 来加载
        if (!string.IsNullOrEmpty(dungeon.image.url))
        {
            Debug.Log($"Dungeon {dungeonName} has image URL: {dungeon.image.url}");
            var fullUrl = GameContext.BaseUrl.TrimEnd('/') + dungeon.image.url; // 由调用方外部构造完整 URL，避免在组件内部处理 URL 拼接逻辑
            SetBackgroundImage(fullUrl).Forget();
        }
        else
        {
            Debug.LogWarning($"Dungeon {dungeonName} does not have a valid image URL");
            _backgroundImage.sprite = null; // 清除背景图片
        }
    }

    /// <summary>
    /// 从完整 URL 加载纹理并设置到背景图片上
    /// </summary>
    /// <param name="fullUrl">纹理的完整 URL，由调用方外部构造</param>
    private async UniTask SetBackgroundImage(string fullUrl)
    {
        if (SpriteCacheManager.Instance == null)
        {
            Debug.LogError("[DungeonOverviewDetailPanel] SpriteCacheManager instance is not available");
            return;
        }

        if (string.IsNullOrEmpty(fullUrl))
        {
            Debug.LogError("[DungeonOverviewDetailPanel] URL is null or empty");
            return;
        }

        var texture = await SpriteCacheManager.Instance.LoadRemoteTexture(fullUrl);
        if (texture == null)
        {
            Debug.LogError($"[DungeonOverviewDetailPanel] Failed to load image: {fullUrl}");
            return;
        }

        // 销毁旧的 Sprite 防止 native 内存泄漏（不影响全局缓存的 Texture2D）
        if (_backgroundImage.sprite != null)
        {
            Destroy(_backgroundImage.sprite);
        }

        _backgroundImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
