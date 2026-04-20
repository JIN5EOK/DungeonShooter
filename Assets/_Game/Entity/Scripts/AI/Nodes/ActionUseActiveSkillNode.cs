using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// 지정한 인덱스의 액티브 스킬을 사용하도록 지시하는 Leaf 노드입니다.
    /// </summary>
    public class ActionUseActiveSkillNode : LeafNode<AiBTContext>
    {
        private int _skillIdx;
        public ActionUseActiveSkillNode(int skillIdx)
        {
            _skillIdx = skillIdx;
        }

        /// <inheritdoc />
        public override BTStatus Execute(AiBTContext context)
        {
            var skills = context.Self.GetContext().Skills;
            var skill = skills?.GetSkills()[_skillIdx];

            if (skill != null && !skill.IsCooldown)
            {
                context.Self.GetContext().InputContext.SkillInput = skill.SkillTableEntrySo.Id;
                return BTStatus.Success;
            }
            else
            {
                return BTStatus.Failure;
            }
        }
    }
}
