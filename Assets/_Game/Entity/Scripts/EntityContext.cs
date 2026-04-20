namespace DungeonShooter
{
    /// <summary>
    /// Entity의 입력/스탯/상태/스킬 등 행동에 필요한 컨텍스트.
    /// </summary>
    public interface IEntityContext
    {
        public HealthModel HealthModel { get; }
        public IEntityStats Stat { get; }
        public IEntitySkills Skill { get; }
        public IEntityInputContext InputContext { get; }
    }
    public class EntityContext : IEntityContext
    {
        public IEntityInputContext InputContext { get; }
        public IEntityStats Stat { get; }
        public HealthModel HealthModel { get; }
        public IEntitySkills Skill { get; }

        public EntityContext(
            IEntityInputContext inputContext,
            IEntityStats stat,
            HealthModel healthModel,
            IEntitySkills skill)
        {
            InputContext = inputContext;
            Stat = stat;
            HealthModel = healthModel;
            Skill = skill;
        }
    }
}
