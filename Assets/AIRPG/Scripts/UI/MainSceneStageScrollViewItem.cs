using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌要素滚动视图项组件
/// 用于在动态滚动视图中显示单个卡牌要素的信息和交互
/// </summary>
public class MainSceneStageScrollViewItem : UIBehaviour, IScrollViewItem
{
    public static readonly int MaxActorIcons = 4; // 最大状态图标数量

    [Header("UI Components")]
    [SerializeField] private TMP_Text _title; // card名称文本
    [SerializeField] private Image[] _actorIcons; // 状态图标数组

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onMainSceneStageItemClickedEvent; // MainScene HomeScene 列表项被点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。


    // 保存当前索引，用于事件传递
    private int _currentIndex = -1;

    /// <summary>
    /// 当组件被启用时调用
    /// 注册按钮点击事件监听
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// 当组件被禁用时调用
    /// 注销按钮点击事件监听,防止内存泄漏
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnDestroy()
    {

    }


    /// <summary>
    /// 按钮点击事件处理方法
    /// </summary>
    public void OnClickItem()
    {
        Debug.Log($"Clicked on Home Scene State item at index: {_currentIndex}");

        var data = GetData();
        if (data == null)
        {
            Debug.LogError($"No data found for index {_currentIndex} in HomeSceneDataList");
            return;
        }

        // 这里可以根据需要实现点击后的逻辑，例如打开场景详情界面等
        Debug.Log($"Scene Name: {data.stageName}, Actors on Stage");

        // 创建并发送结构化的事件数据，通知需要刷新UI
        //var elementData = CardBuilder.GetElement(_currentIndex);
        // 创建并发送结构化的事件数据，通知系统哪个卡牌要素被点击了
        var eventData = new UIEventData(
            UIEventType.MainSceneStageItemClicked,
            data.stageName, // 可以根据需要传递更多数据，例如关卡名称、角色列表等   
            _currentIndex,
            extra: data.dungeonName
        );

        // 触发事件，通知系统哪个卡牌要素被点击了
        Debug.Assert(_onMainSceneStageItemClickedEvent != null, "_onMainSceneStageItemClickedEvent is null");
        _onMainSceneStageItemClickedEvent.Raise(eventData);
    }


    /// <summary>
    /// 实现IDynamicScrollViewItem接口的更新方法
    /// 根据索引更新显示的卡牌要素信息
    /// </summary>
    /// <param name="index">在滚动视图中的索引位置</param>
    public void OnUpdateItem(int index)
    {
        Debug.Assert(_title != null, "_title is null in HomeSceneStateScrollViewItem");
        Debug.Assert(_actorIcons != null && _actorIcons.Length == MaxActorIcons, "_actorIcons array is null or empty in HomeSceneStateScrollViewItem");
        Debug.Assert(SpriteCacheManager.Instance != null, "SpriteCacheManager instance is null in HomeSceneStateScrollViewItem");


        // 保存当前索引
        _currentIndex = index;

        if (!GameContext.Instance.IsLoggedIn)
        {
            _title.text = MockData.MockStageName + $" #{index}";

            var cachedSprite = SpriteCacheManager.Instance.GetSprite(MockData.MockActorName);
            if (cachedSprite != null)
            {
                for (int i = 0; i < MaxActorIcons; i++)
                {
                    _actorIcons[i].sprite = cachedSprite;
                    //_actorIcons[i].enabled = true;
                }
            }
            else
            {
                Debug.LogWarning($"HomeSceneStateScrollViewItem: Sprite not found for actor: {MockData.MockActorName}");
                for (int i = 0; i < MaxActorIcons; i++)
                {
                    _actorIcons[i].sprite = null;
                    //_actorIcons[i].enabled = false;
                }
            }
        }
        else
        {
            RefreshView();
        }
    }

    private void RefreshView()
    {
        var data = GetData();
        Debug.Assert(data != null, $"No data found for index {_currentIndex} in HomeSceneDataList");
        if (data == null)
        {
            Debug.LogError($"No data found for index {_currentIndex} in HomeSceneDataList");
            _title.text = "Unknown Scene";
            foreach (var icon in _actorIcons)
            {
                icon.gameObject.SetActive(false);
                icon.sprite = null;
            }
            return;
        }

        _title.text = data.stageName;
        for (int i = 0; i < MaxActorIcons; i++)
        {
            if (i < data.actorsOnStage.Count)
            {
                var actorEntity = data.actorsOnStage[i];
                var cachedSprite = SpriteCacheManager.Instance.GetSprite(actorEntity.name);
                if (cachedSprite != null)
                {
                    _actorIcons[i].gameObject.SetActive(true);
                    _actorIcons[i].sprite = cachedSprite;
                }
                else
                {
                    Debug.LogWarning($"HomeSceneStateScrollViewItem: Sprite not found for actor: {actorEntity.name}");
                    _actorIcons[i].gameObject.SetActive(false);
                    _actorIcons[i].sprite = null;
                }
            }
            else
            {
                _actorIcons[i].gameObject.SetActive(false);
                _actorIcons[i].sprite = null;
            }
        }

    }

    /// <summary>
    ///  从HomeScenesPanel的HomeSceneDataList中获取当前索引对应的数据
    /// </summary>
    /// <returns></returns>
    private HomeSceneData GetData()
    {
        if (_currentIndex >= 0 && _currentIndex < MainScene.HomeScenes.Count)
        {
            return MainScene.HomeScenes[_currentIndex];
        }

        Debug.LogError($"Invalid index {_currentIndex} for HomeSceneDataList with count {MainScene.HomeScenes.Count}");
        return null;
    }
}
