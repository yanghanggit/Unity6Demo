
public interface ICombatState
{
    public ICombatScene CombatScene { get; set; }

    void OnEnter();
}
