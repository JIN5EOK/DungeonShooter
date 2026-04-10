using UnityEngine;

namespace DungeonShooter
{
    public class TouchInputUI : HudUI
    {
        [SerializeField] 
        private SkillCooldownSlot _skillCooldownSlot1;
        [SerializeField] 
        private SkillCooldownSlot _skillCooldownSlot2;

        public void SetSkillSlot(int idx, Skill skill)
        {
            var targetSlot = idx == 0 ? _skillCooldownSlot1 : _skillCooldownSlot2;
            targetSlot?.SetSkill(skill);
        }
    }
}