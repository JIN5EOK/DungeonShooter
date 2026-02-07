using UnityEngine;
using Jin5eok;

namespace DungeonShooter
{
    public abstract class EntityBase : MonoBehaviour
    {
        public EntityStatGroup StatGroup { get; private set; }
        public EntitySkillGroup SkillGroup { get; private set; }

        /// <summary> StatGroup을 주입합니다. </summary>
        public void SetStatGroup(EntityStatGroup statGroup)
        {
            StatGroup = statGroup;
        }

        /// <summary>
        /// SkillGroup을 주입합니다.
        /// 기존 그룹의 스킬은 모두 적용 해제하고, 새 그룹의 스킬은 순회하여 적용합니다.
        /// </summary>
        public void SetSkillGroup(EntitySkillGroup skillGroup)
        {
            if (SkillGroup != null)
            {
                foreach (var s in SkillGroup.GetRegistedSkills())
                {
                    UnapplySkill(s);
                }

                SkillGroup.OnSkillRegisted -= ApplySkill;
                SkillGroup.OnSkillUnregisted -= UnapplySkill;
            }

            SkillGroup = skillGroup;

            if (skillGroup != null)
            {
                skillGroup.OnSkillRegisted += ApplySkill;
                skillGroup.OnSkillUnregisted += UnapplySkill;

                foreach (var s in skillGroup.GetRegistedSkills())
                {
                    ApplySkill(s);
                }
            }
        }
        
        private void ApplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Activate(this);
            }
        }
        
        private void UnapplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Deactivate(this);
            }
        }
    }
}
