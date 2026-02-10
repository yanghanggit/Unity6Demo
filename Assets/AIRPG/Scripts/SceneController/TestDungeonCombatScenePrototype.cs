using UnityEngine;
using TMPro;

public class TestDungeonCombatScenePrototype : MonoBehaviour, IUIEventListener
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _mainText; // 主文本显示对象
    [SerializeField] private TMP_Text _combatInfoText; // 战斗信息显示对象

    [Header("Events")]
    [SerializeField] private UIEventGameEvent _onCardClickedEvent; // 卡牌点击事件
    [SerializeField] private UIEventGameEvent _onActorSlotClickedEvent; // 角色槽位点击事件

    void Start()
    {
        Debug.Assert(_combatInfoText != null, "_combatInfoText is null");
        Debug.Assert(_mainText != null, "Main Text component is not assigned in the inspector.");
        Debug.Assert(_onCardClickedEvent != null, "_onCardClickedEvent is null");

        // 设置初始文本内容
        _combatInfoText.text = "场景.测试地下城关卡-第1局";
        _mainText.text = "这是一个测试";

        // 注册事件监听器
        _onCardClickedEvent.RegisterListener(this);
        _onActorSlotClickedEvent.RegisterListener(this);
    }

    void OnDestroy()
    {
        // 确保在对象销毁时取消注册事件监听器，避免内存泄漏或错误调用
        if (_onCardClickedEvent != null)
        {
            _onCardClickedEvent.UnregisterListener(this);
        }

        if (_onActorSlotClickedEvent != null)
        {
            _onActorSlotClickedEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 点击 Setting 按钮
    /// </summary>
    public void OnClickSetting()
    {
        Debug.Log("Setting button clicked");
    }

    /// <summary>
    /// 点击 Info 按钮
    /// </summary>
    public void OnClickInfo()
    {
        Debug.Log("Info button clicked");
        // TODO: 显示信息面板
    }

    /// <summary>
    /// 点击 Run 按钮
    /// </summary>
    public void OnClickRun()
    {
        Debug.Log("Run button clicked");
        // TODO: 执行逃跑操作
    }

    /// <summary>
    /// IUIEventListener 接口实现
    /// 处理所有UI事件的统一入口
    /// </summary>
    public void OnEventRaised(UIEventData eventData)
    {
        Debug.Log($"[TestDungeonCombatScenePrototype] {eventData}");
        Debug.Log($"OnEventRaised: {eventData.eventType}, TargetId: {eventData.targetId}, Index: {eventData.index}, ExtraData: {eventData.extraData}");
        _mainText.text = $"事件: {eventData.eventType}\n目标: {eventData.targetId}\n索引: {eventData.index}\n额外: {eventData.extraData}";
    }
}
