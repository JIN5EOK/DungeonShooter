using System;
using UnityEngine;
using Jin5eok;

namespace DungeonShooter
{
    public class EntityBase : MonoBehaviour
    {
        public event Action<EntityBase> OnDestroyed;

        public EntityStatGroup StatGroup { get; private set; }
        public EntitySkillContainer SkillContainer { get; private set; }

        /// <summary>
        /// 엔티티를 제거합니다. 실제 제거 시점에 OnDestroyed가 호출됩니다.
        /// </summary>
        public void Destroy()
        {
            UnityEngine.Object.Destroy(gameObject);
        }

        /// <summary> StatGroup을 주입합니다. </summary>
        public void SetStatGroup(EntityStatGroup statGroup)
        {
            StatGroup = statGroup;
        }

        /// <summary>
        /// SkillGroup을 주입합니다.
        /// 기존 그룹의 스킬은 모두 적용 해제하고, 새 그룹의 스킬은 순회하여 적용합니다.
        /// </summary>
        public void SetSkillGroup(EntitySkillContainer skillContainer)
        {
            if (SkillContainer != null)
            {
                foreach (var s in SkillContainer.GetRegistedSkills())
                {
                    UnapplySkill(s);
                }

                SkillContainer.OnSkillRegisted -= ApplySkill;
                SkillContainer.OnSkillUnregisted -= UnapplySkill;
            }

            SkillContainer = skillContainer;

            if (skillContainer != null)
            {
                skillContainer.OnSkillRegisted += ApplySkill;
                skillContainer.OnSkillUnregisted += UnapplySkill;

                foreach (var s in skillContainer.GetRegistedSkills())
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

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
    }
}
