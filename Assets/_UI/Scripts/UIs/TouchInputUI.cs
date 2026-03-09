using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class TouchInputUI : HudUI
    {
        [SerializeField] 
        private SkillCooldownSlot _skillCooldownSlot1;
        [SerializeField] 
        private SkillCooldownSlot _skillCooldownSlot2;
        
        private ISkillSlotViewModel _skillSlotViewModel;
        
        [Inject]
        public void Construct(ISkillSlotViewModel skillSlotViewModel)
        {
            _skillSlotViewModel = skillSlotViewModel;
            _skillSlotViewModel.OnSkillSlotChanged += OnSkillSlotChanged; 
        }

        private void OnSkillSlotChanged(int idx, Skill skill)
        {
            var targetSlot = idx == 0 ? _skillCooldownSlot1 : _skillCooldownSlot2;
            targetSlot.SetSkill(skill);
        }
    }
}