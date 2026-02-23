namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯(최대 체력 등) 수치 관련 인터페이스.
    /// </summary>
    public interface IEntityStats
    {
        public void Initialize(EntityStatsTableEntry entry);
        public IEntityStat GetStat(StatType type);
        public void ApplyStatBonus(object key, StatBonus bonus);
        public void RemoveStatBonus(object key);
    }
}
