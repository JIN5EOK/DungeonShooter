using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 레벨업 시 표시되는 스킬 레벨업 선택 UI. 체력/공격력 패시브와 액티브 스킬 2개 중 다음 레벨이 있는 것만 표시합니다.
    /// 레벨업 가능 스킬이 3개를 넘으면 3개만 랜덤으로 골라 표시합니다.
    /// </summary>
    public class SkillLevelUpUI : PopupUI
    {
        private const int MaxDisplayCount = 3;

        [SerializeField]
        private SkillLevelUpSlot _skillLevelUpSlotPrefab;
        private List<SkillLevelUpSlot> _slots = new();

        private IEventBus _eventBus;
        private IPlayerDataService _playerDataService;
        private ISkillService _skillService;
        private IPauseManager _pauseManager;
        private ITableRepository _tableRepository;

        [Inject]
        public void Construct(IPlayerDataService playerDataService, ISkillService skillService, IEventBus eventBus, IPauseManager pauseManager, ITableRepository tableRepository)
        {
            _playerDataService = playerDataService;
            _skillService = skillService;
            _eventBus = eventBus;
            _pauseManager = pauseManager;
            _tableRepository = tableRepository;
            _eventBus.Subscribe<PlayerLevelChangeEvent>(PlayerLevelChanged);
        }

        private void PlayerLevelChanged(PlayerLevelChangeEvent playerLevelChangeEvent)
        {
            SetLevelUpSkillAndShow();
        }
        
        /// <summary>
        /// 지니고 있는 스킬중 레벨업 가능한 스킬을 찾아내어 표시
        /// </summary>
        public void SetLevelUpSkillAndShow()
        {
            var skills = _playerDataService?.EntityContext?.Skill?.GetRegistedSkills();
            var levelUpableList = _skillService.GetLevelUpableSkills(skills);

            IReadOnlyList<LevelUpableSkillInfo> toDisplay = levelUpableList;
            if (levelUpableList.Count > MaxDisplayCount)
            {
                var shuffled = new List<LevelUpableSkillInfo>(levelUpableList);
                for (var i = shuffled.Count - 1; i > 0; i--)
                {
                    var j = Random.Range(0, i + 1);
                    (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
                }
                shuffled.RemoveRange(MaxDisplayCount, shuffled.Count - MaxDisplayCount);
                toDisplay = shuffled;
            }

            var slotIndex = 0;
            foreach (var info in toDisplay)
            {
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
                    _skillService.TrySkillLevelUp(_playerDataService?.EntityContext?.Skill, info.CurrentSkill);
                    Hide();
                });

                slotIndex++;
            }

            // 레벨업 가능 스킬 슬롯이 1개 이상일때만 UI 표시
            if (slotIndex > 0)
            {
                Show();
            }
        }

        public override void Show()
        {
            base.Show();
            _pauseManager?.PauseRequest(this);
        }

        public override void Hide()
        {
            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);

            _pauseManager?.ResumeRequest(this);
            base.Hide();
        }
    }
}
