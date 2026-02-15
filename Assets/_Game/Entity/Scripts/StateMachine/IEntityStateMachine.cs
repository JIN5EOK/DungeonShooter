namespace DungeonShooter
{
    public interface IEntityStateMachine
    {
        EntityBase Entity { get; }
        EntityInputContext InputContext { get; }
        void RequestChangeState(EntityStates nextStates);
    }
}
