namespace DungeonShooter
{
    /// <summary>
    /// 공통 스테이터스 접근용 인터페이스 (읽기/쓰기)
    /// </summary>
    public interface IEntityStats
    {
        float MoveSpeed { get; set; }
        int MaxHealth { get; set; }
        int Attack { get; set; }
        int Defense { get; set; }
    }
}
