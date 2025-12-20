using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色迷你图标组件
/// 用于显示角色的小型图标,包括角色头像和名称
/// </summary>
public class ActorMiniIcon : MonoBehaviour, IStringGameEventListener
{
    [Header("UI Components")]
    [SerializeField] private Image _actorImage;          // 角色头像图片
    [SerializeField] private TMP_Text _actorNameText;    // 角色名称文本
    [SerializeField] private GameObject _deathOverlay;    // 死亡覆盖标记（例如骷髅图标或X标记）
    [SerializeField] private StringGameEvent _onActorAvatarsRefreshEvent; // 角色头像刷新事件

    void Start()
    {
        Debug.Assert(_actorImage != null, "_actorImage is null");
        Debug.Assert(_actorNameText != null, "_actorNameText is null");
        Debug.Assert(_deathOverlay != null, "_deathOverlay is null");
        Debug.Assert(_onActorAvatarsRefreshEvent != null, "_onActorAvatarsRefreshEvent is null");

        // 初始隐藏死亡覆盖标记
        _deathOverlay.SetActive(false);

        // 注册事件监听
        _onActorAvatarsRefreshEvent.RegisterListener(this);

        //
        RefreshDisplay();
    }

    void OnDestroy()
    {
        // 注销事件监听
        if (_onActorAvatarsRefreshEvent != null)
        {
            _onActorAvatarsRefreshEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 刷新角色迷你图标的UI显示
    /// 从GameContext获取角色最新数据，更新名称、头像和死亡状态的显示
    /// </summary>
    private void RefreshDisplay()
    {
        var actorEntity = GameContext.Instance.GetActorEntitySerialization(gameObject.name);
        if (actorEntity == null)
        {
            Debug.LogWarning($"ActorMiniIcon: Actor entity not found for name: {gameObject.name}");
            return;
        }

        // 设置角色名称
        _actorNameText.text = GameUtils.GetDisplayName(gameObject.name);

        // 设置角色头像
        var avatarSprite = GetActorAvatarSprite(gameObject.name);
        if (avatarSprite != null)
        {
            _actorImage.sprite = avatarSprite;
        }
        else
        {
            Debug.LogWarning($"Actor sprite not found for: {gameObject.name}");
        }

        // 更新死亡状态显示，注意！如果从地下城回来，是会移除死亡组件的，所以即使在main2场景刷新也不会刷出死亡状态！
        var deathComponent = GameUtils.GetComponent<DeathComponent>(actorEntity);
        var isDead = deathComponent != null;
        _deathOverlay.SetActive(isDead);
        _actorImage.color = isDead ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        _actorNameText.color = isDead ? Color.red : Color.black;
    }

    public void OnEventRaised(string value)
    {
        Debug.Log($"ActorMiniIcon: Received avatar refresh event for scene: {value}");
        // 当收到头像刷新事件时，更新角色显示
        RefreshDisplay();
    }

    /// <summary>
    /// 获取角色头像精灵图
    /// 优先尝试加载头像素材（格式：角色名_头像），如果失败则降级使用全身图
    /// </summary>
    /// <param name="actorName">角色名称</param>
    /// <returns>角色头像精灵图，如果未找到返回 null</returns>
    private Sprite GetActorAvatarSprite(string actorName)
    {
        // 第一层防御：优先尝试加载头像素材（键值格式：角色名_头像）
        var avatarSprite = TextureManager.Instance.GetSprite(actorName + "_头像");

        // 第二层防御：如果没有头像素材，降级使用全身图
        if (avatarSprite == null)
        {
            avatarSprite = TextureManager.Instance.GetSprite(actorName);
        }

        return avatarSprite;
    }
}
