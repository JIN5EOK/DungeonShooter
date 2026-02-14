using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 쿨다운 슬롯들을 HorizontalLayoutGroup으로 배치하고 추가/제거하는 HUD.
    /// </summary>
    public class SkillCooldownHudUI : HudUI
    {
        [Header("레이아웃")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private SkillCooldownSlot _skillCooldownSlotPrefab;
        
        public IReadOnlyDictionary<Skill, SkillCooldownSlot> Slots => _slots;
        private readonly Dictionary<Skill, SkillCooldownSlot> _slots = new();
        
        private IEventBus _eventBus;
        
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<SkillLevelUpEvent>(SkillChanged);
        }

        private void SkillChanged(SkillLevelUpEvent skillLevelUpEvent)
        {
            if (!_slots.TryGetValue(skillLevelUpEvent.beforeSkill, out var slot))
            {
                var beforeSkill = skillLevelUpEvent.beforeSkill;
                beforeSkill.OnCooldownChanged -= slot.SetCooldown;
                var afterSkill = skillLevelUpEvent.afterSkill;
                SetupSlot(slot, afterSkill.Cooldown, afterSkill.MaxCooldown, afterSkill.Icon);
            }
        }
        
        /// <summary>
        /// 쿨다운 슬롯을 하나 추가하고 해당 컴포넌트를 반환한다.
        /// </summary>
        public void AddSkillCooldownSlot(Skill skill)
        {
            if (_content == null || _skillCooldownSlotPrefab == null)
                return;

            var slot = Instantiate(_skillCooldownSlotPrefab, _content, false);
            
            _slots.Add(skill, slot);
                            
            SetupSlot(slot, skill.Cooldown, skill.MaxCooldown, skill.Icon);
            skill.OnCooldownChanged += slot.SetCooldown;
        }

        private void SetupSlot(SkillCooldownSlot slot, float cooldown, float maxCooldown, Sprite icon)
        {
            slot.SetCooldown(cooldown);
            slot.SetMaxCooldown(maxCooldown);
            slot.SetSkillIcon(icon);
        }

        public void Clear()
        {
            foreach (var s in _slots)
            {
                s.Key.OnCooldownChanged -= s.Value.SetCooldown;
                Destroy(s.Value.gameObject);
            }
            
            _slots.Clear();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
            _eventBus.Unsubscribe<SkillLevelUpEvent>(SkillChanged);
        }
    }
}
