using System.Collections.Generic;
using System;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 레벨업 시 표시되는 스킬 레벨업 선택 UI
    /// </summary>
    public class SkillLevelUpUI : PopupUI
    {
        private const int MaxDisplayCount = 3;

        [SerializeField]
        private SkillLevelUpSlot _skillLevelUpSlotPrefab;
        private List<SkillLevelUpSlot> _slots = new();

        private ITableRepository _tableRepository;

        [Inject]
        public void Construct(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 레벨업 가능 스킬 목록을 표시하고, 선택 시 콜백을 호출합니다.
        /// </summary>
        public void ShowLevelUpSkillOptions(IReadOnlyList<LevelUpableSkillInfo> levelUpableList, Action<Skill> onSelected)
        {
            if (levelUpableList == null || levelUpableList.Count == 0)
                return;

            var toShowCount = Mathf.Min(levelUpableList.Count, MaxDisplayCount);
            for (var slotIndex = 0; slotIndex < toShowCount; slotIndex++)
            {
                var info = levelUpableList[slotIndex];
                if (_slots.Count <= slotIndex)
                {
                    var slotInstance = Instantiate(_skillLevelUpSlotPrefab, transform);
                    _slots.Add(slotInstance);
                }

                var slot = _slots[slotIndex];
                slot.gameObject.SetActive(true);

                var currentEntry = info.CurrentSkill.SkillTableEntry;
                slot._currentSkillInfo.SetInfo(_tableRepository.GetStringText(currentEntry.SkillNameId), _tableRepository.GetStringText(currentEntry.SkillDescriptionId), currentEntry.Cooldown, info.CurrentIcon);

                var nextEntry = info.NextLevelEntry;
                slot._nextSkillInfo.SetInfo(_tableRepository.GetStringText(nextEntry.SkillNameId), _tableRepository.GetStringText(nextEntry.SkillDescriptionId), nextEntry.Cooldown, info.NextLevelIcon);

                slot.SetSelectHandler(() =>
                {
                    onSelected?.Invoke(info.CurrentSkill);
                    Hide();
                });
            }
        }

        public override void Hide()
        {
            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);
            
            base.Hide();
        }
    }
}
