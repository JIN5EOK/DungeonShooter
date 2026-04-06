namespace DungeonShooter
{
    public interface IEntityStateMachine
    {
        public EntityBase Entity { get; }
        public void Initialize(params IEntityState[] states);
        public IEntityInputContext InputContext { get; }
        public void RequestChangeState(EntityStates nextStates);
    }
}
