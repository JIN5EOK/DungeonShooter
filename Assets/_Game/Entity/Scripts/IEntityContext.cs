namespace DungeonShooter
{
    /// <summary>
    /// Entity의 입력/스탯/상태/스킬 등 행동에 필요한 컨텍스트.
    /// 플레이어/적별로 팩토리에서 구체 컨텍스트를 주입한다.
    /// </summary>
    public interface IEntityContext
    {
        public IEntityInputContext InputContext { get; }
        public IEntityStats Stat { get; }
        public IEntityStatus Status { get; }
        public IEntitySkills Skill { get; }
    }
}
