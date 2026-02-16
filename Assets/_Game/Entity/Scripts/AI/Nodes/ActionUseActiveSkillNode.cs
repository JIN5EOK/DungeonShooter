using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// 지정한 인덱스의 액티브 스킬을 사용하도록 지시하는 Leaf 노드입니다.
    /// </summary>
    public class ActionUseActiveSkillNode : LeafNode<AiBTContext>
    {
        private readonly int _skillIndex;

        public ActionUseActiveSkillNode(int skillIndex)
        {
            _skillIndex = skillIndex;
        }

        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            if (context.Self == null || 
                context.ActiveSkills == null || 
                _skillIndex < 0 || 
                _skillIndex >= context.ActiveSkills.Count)
            {
                return BTStatus.Failure;
            }
            
            var skill = context.ActiveSkills[_skillIndex];
            
            if (skill == null || skill.IsCooldown)
            {
                return BTStatus.Failure;
            }

            context.Self.EntityInputContext.SkillInput = skill;
            return BTStatus.Success;
        }
    }
}
