namespace DungeonShooter
{
    /// <summary>
    /// Entity의 입력/스탯/상태/스킬 컨텍스트
    /// </summary>
    public class EntityContext : IEntityContext
    {
        public IEntityInputContext InputContext { get; }
        public IEntityStats Stat { get; }
        public IEntityStatus Status { get; }
        public IEntitySkills Skill { get; }

        public EntityContext(
            IEntityInputContext inputContext,
            IEntityStats stat,
            IEntityStatus status,
            IEntitySkills skill)
        {
            InputContext = inputContext;
            Stat = stat;
            Status = status;
            Skill = skill;
        }
    }
}
