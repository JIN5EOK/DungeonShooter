namespace DungeonShooter
{
    /// <summary>
    /// Entity의 현재 상태(현재 체력 등) 수치 관련 인터페이스.
    /// </summary>
    public interface IEntityStatus
    {
        int CurrentHp { get; }
    }
}
