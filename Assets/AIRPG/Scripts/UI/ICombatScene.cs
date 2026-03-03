
public interface ICombatScene
{
    /// <summary>
    /// 切换战斗状态
    /// </summary>
    /// <param name="newState">新的战斗状态</param>
    void SwitchCombatState(CombatState newState);

}
