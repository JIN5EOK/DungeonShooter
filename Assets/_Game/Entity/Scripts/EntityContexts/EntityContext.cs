namespace DungeonShooter
{
    /// <summary>
    /// Entity의 입력/스탯/상태/스킬 등 행동에 필요한 컨텍스트.
    /// </summary>
    public interface IEntityContext
    {
        public IHealth HealthModel { get; }
        public IEntityStats Stats { get; }
        public IEntitySkills Skills { get; }
        public IEntityInputContext InputContext { get; }
    }
    public class EntityContext : IEntityContext
    {
        public IEntityInputContext InputContext { get; }
        public IEntityStats Stats { get; }
        public IHealth HealthModel { get; }
        public IEntitySkills Skills { get; }

        public EntityContext(
            IEntityInputContext inputContext,
            IEntityStats stats,
            IHealth healthModel,
            IEntitySkills skills)
        {
            InputContext = inputContext;
            Stats = stats;
            HealthModel = healthModel;
            Skills = skills;

            if (Skills != null)
            {
                Skills.OnSkillRegisted += ActivatePassive;
                Skills.OnSkillUnregisted += DeactivatePassive;
                
                foreach (var skill in Skills.GetSkills())
                {
                    ActivatePassive(skill);
                }
            }
        }

        private void ActivatePassive(Skill skill)
        {
            if (skill?.SkillData?.IsPassiveSkill == true)
                skill.Activate(this);
        }

        private void DeactivatePassive(Skill skill)
        {
            if (skill?.SkillData?.IsPassiveSkill == true)
                skill.Deactivate(this);
        }
    }
}
