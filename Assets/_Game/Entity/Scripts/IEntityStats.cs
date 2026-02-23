namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯(최대 체력 등) 수치 관련 인터페이스.
    /// </summary>
    public interface IEntityStats
    {
        IEntityStat GetStat(StatType type);
        void ApplyStatBonus(object key, StatBonus bonus);
        void RemoveStatBonus(object key);
    }
}
