using Mosframe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色滚动视图项组件
/// 用于在动态滚动视图中显示单个角色的信息和交互
/// </summary>
public class ActorScrollViewItem : UIBehaviour, IDynamicScrollViewItem
{
    // UI组件引用
    [Header("UI Components")]
    [SerializeField] private Image _icon;                               // 角色图标
    [SerializeField] private TMP_Text _title;                           // 角色名称文本
    [SerializeField] private Image _background;                         // 背景图片
    [SerializeField] private Button _overlayButton;             // 覆盖层按钮,用于接收点击
    [SerializeField] private StringGameEvent _onActorClickedEvent; // 角色点击事件

    /// <summary>
    /// 当前显示的角色名称
    /// </summary>
    private string _actorName = string.Empty;

    /// <summary>
    /// 当组件被启用时调用
    /// 注册按钮点击事件监听
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        _overlayButton.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 当组件被禁用时调用
    /// 注销按钮点击事件监听,防止内存泄漏
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        _overlayButton.onClick.RemoveListener(OnClick);
    }

    /// <summary>
    /// 按钮点击事件处理
    /// 触发角色点击游戏事件,将角色名称传递给监听者
    /// </summary>
    void OnClick()
    {
        Debug.Log("Clicked on " + _actorName);
        _onActorClickedEvent.Raise(_actorName);
    }

    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// 根据索引更新显示的角色信息
    /// </summary>
    /// <param name="index">在滚动视图中的索引位置</param>
    public void onUpdateItem(int index)
    {
        // 验证所有必需的UI组件引用
        Debug.Assert(_icon != null, "_icon != null");
        Debug.Assert(_title != null, "_title != null");
        Debug.Assert(_background != null, "_background != null");
        Debug.Assert(_overlayButton != null, "_overlayButton != null");
        Debug.Assert(_onActorClickedEvent != null, "onActorClickedEvent != null");

        // 获取当前场景中除了玩家角色外的其他角色列表
        
        if (RootResp.Get() != null)
        {
            var actorsInStage = GameContext.Instance.GetOtherActorsInCurrentStage();
        
            // 这段是正常的逻辑，也就是说有服务器返回其他角色数据
            Debug.Assert(actorsInStage.Count > 0, "actorsInStage.Count > 0");
            Debug.Assert(index < actorsInStage.Count, "index < actorsInStage.Count");

            // 根据索引获取对应的角色名称
            _actorName = actorsInStage[index];

            // 更新UI显示:设置角色名称文本
            _title.text = _actorName;

            // 更新UI显示:从纹理管理器加载并设置角色图标
            var actorSprite = TextureManager.Instance.GetSprite(_actorName);
            Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + _actorName);
            _icon.sprite = actorSprite;
        }
        else
        {
            // 如果没有其他角色，显示默认信息
            _actorName = string.Empty;
            _title.text = "";
            // 设置一个默认的图标或清空图标
            _icon.sprite = null; // 或者设置为一个默认的Sprite
        }

    }
}
