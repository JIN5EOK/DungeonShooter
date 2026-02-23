namespace DungeonShooter
{
    public interface IEntityStateMachine
    {
        public EntityBase Entity { get; }
        public IEntityInputContext InputContext { get; }
        public void RequestChangeState(EntityStates nextStates);
    }
}
