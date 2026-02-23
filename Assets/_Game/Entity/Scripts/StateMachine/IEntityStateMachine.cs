namespace DungeonShooter
{
    public interface IEntityStateMachine
    {
        EntityBase Entity { get; }
        IEntityInputContext InputContext { get; }
        void RequestChangeState(EntityStates nextStates);
    }
}
