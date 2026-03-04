
public interface ICombatScene
{
    /// <summary>
    /// 切换战斗状态
    /// </summary>
    /// <param name="newState">新的战斗状态</param>
    //void SwitchCombatState(CombatState newState);

    /// <summary>
    /// 设置顶部信息显示文本，通常包含当前地下城、关卡、回合数等信息
    /// </summary>
    /// <param name="info">要显示的顶部信息文本</param>
    //void SetTopBarInfo(string info);

    void OnEnterOnGoingState();

    void OnEnterPostCombatState();

}
