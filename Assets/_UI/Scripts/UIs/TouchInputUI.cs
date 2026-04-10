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
        
        private IPlayerContextManager _playerContextManager;
        
        [Inject]
        public void Construct(IPlayerContextManager playerContextManager)
        {
            _playerContextManager = playerContextManager;
            _playerContextManager.OnActiveSkillSlotChanged += OnSkillSlotChanged; 
        }

        private void OnSkillSlotChanged(int idx, Skill skill)
        {
            var targetSlot = idx == 0 ? _skillCooldownSlot1 : _skillCooldownSlot2;
            targetSlot.SetSkill(skill);
        }
    }
}