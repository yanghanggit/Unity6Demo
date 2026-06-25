// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using TMPro;

// /// <summary>
// /// 角色滚动视图项组件
// /// 用于在动态滚动视图中显示单个角色的信息和交互
// /// </summary>
// public class HomeSceneActorScrollViewItem : UIBehaviour, IScrollViewItem
// {
//     // UI组件引用
//     [Header("UI Components")]
//     [SerializeField] private Image _icon;                               // 角色图标
//     [SerializeField] private TMP_Text _title;                           // 角色名称文本
//                                                                         //[SerializeField] private StringGameEvent _onActorClickedEvent; // 角色点击事件

//     [SerializeField] private UIEventGameEvent _onHomeSceneActorItemClickedEvent; // MainScene HomeScene 列表项被点击事件, 这个事件自己不可以再听了，是发送端，不能再监听了，否则会死循环。


//     /// <summary>
//     /// 当前显示的角色名称
//     /// </summary>
//     private string _actorName = string.Empty;

//     /// <summary>
//     /// 按钮点击事件处理
//     /// 触发角色点击游戏事件,将角色名称传递给监听者
//     /// </summary>
//     public void OnClick()
//     {
//         Debug.Log("Clicked on " + _actorName);

//         // 创建并发送结构化的事件数据，通知需要刷新UI
//         //var elementData = CardBuilder.GetElement(_currentIndex);
//         // 创建并发送结构化的事件数据，通知系统哪个卡牌要素被点击了
//         var eventData = new UIEventData(
//             UIEventType.HomeSceneActorItemClicked,
//             _actorName
//         );
//         _onHomeSceneActorItemClickedEvent.Raise(eventData);
//     }

//     /// <summary>
//     /// 实现IDynamicScrollViewItem接口的更新方法
//     /// 根据索引更新显示的角色信息
//     /// </summary>
//     /// <param name="index">在滚动视图中的索引位置</param>
//     public void OnUpdateItem(int index)
//     {
//         // 验证所有必需的UI组件引用
//         Debug.Assert(_icon != null, "_icon != null");
//         Debug.Assert(_title != null, "_title != null");
//         Debug.Assert(_onHomeSceneActorItemClickedEvent != null, "onActorClickedEvent != null");

//         // 获取当前场景中除了玩家角色外的其他角色列表

//         // 这段是正常的逻辑，也就是说有服务器返回其他角色数据
//         Debug.Assert(index < HomeScene.CachedActorNames.Count, "index < ActorNamesInCurrentStage.Count");

//         // 根据索引获取对应的角色名称
//         _actorName = HomeScene.CachedActorNames[index];

//         // 更新UI显示:设置角色名称文本
//         _title.text = GameUtils.GetDisplayName(_actorName);

//         // 更新UI显示:从纹理管理器加载并设置角色图标
//         var actorSprite = SpriteCacheManager.Instance.GetSprite(_actorName);
//         Debug.Assert(actorSprite != null, "Player actor sprite is null for entity: " + _actorName);
//         if (actorSprite != null)
//         {
//             _icon.sprite = actorSprite;
//         }
//         else
//         {
//             Debug.LogWarning($"Sprite not found for actor: {_actorName}");
//             _icon.sprite = null;
//         }
//     }
// }
