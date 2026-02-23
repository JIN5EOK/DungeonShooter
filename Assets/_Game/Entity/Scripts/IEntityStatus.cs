namespace DungeonShooter
{
    /// <summary>
    /// Entity의 현재 상태(현재 체력 등) 수치 관련 인터페이스.
    /// StatType과 대응되는 StatusType별 현재값을 보관한다. Modifier 없음.
    /// </summary>
    public interface IEntityStatus
    {
        public void Initialize(EntityStatsTableEntry entry);
        public IEntityStatusValue GetStatus(StatusType type);
    }
}
