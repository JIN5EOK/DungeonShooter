using System.Collections.Generic;
using UnityEngine;

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

        private readonly List<SkillCooldownSlot> _slots = new();

        /// <summary>
        /// 쿨다운 슬롯을 하나 추가하고 해당 컴포넌트를 반환한다.
        /// </summary>
        public SkillCooldownSlot AddSkillCooldownSlot()
        {
            if (_content == null || _skillCooldownSlotPrefab == null)
                return null;

            var instance = Instantiate(_skillCooldownSlotPrefab, _content, false);
            _slots.Add(instance);
            return instance;
        }
        
        /// <summary>
        /// 지정한 쿨다운 슬롯을 제거한다.
        /// </summary>
        public void RemoveSkillCooldownSlot(SkillCooldownSlot slot)
        {
            if (slot == null)
                return;

            if (_slots.Remove(slot))
                Destroy(slot.gameObject);
        }

        public void Clear()
        {
            foreach (var slot in _slots)
            {
                Destroy(slot.gameObject);
            }
        }
        
        /// <summary>
        /// 현재 등록된 슬롯 수를 반환한다.
        /// </summary>
        public int SlotCount => _slots.Count;

        /// <summary>
        /// 지정한 인덱스의 하위 슬롯을 반환한다. 범위 밖이면 null.
        /// </summary>
        public SkillCooldownSlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
                return null;
            return _slots[index];
        }
    }
}
