using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 레벨업 시 표시되는 스킬 레벨업 선택 UI. 체력/공격력 패시브와 액티브 스킬 2개 중 다음 레벨이 있는 것만 표시합니다.
    /// </summary>
    public class SkillLevelUpUI : PopupUI
    {
        [SerializeField]
        private SkillLevelUpSlot _skillLevelUpSlotPrefab;
        
        private EntitySkillGroup _skillGroup;
        private ITableRepository _tableRepository;
        private ISceneResourceProvider _sceneResourceProvider;
        private List<SkillLevelUpSlot> _slots = new ();
        
        [Inject]
        public void Construct(ITableRepository tableRepository, ISceneResourceProvider sceneResourceProvider)
        {
            _tableRepository = tableRepository;
            _sceneResourceProvider = sceneResourceProvider;
        }
        
        /// <summary>
        /// 지니고 있는 스킬중 레벨업 가능한 스킬을 찾아내어 표시
        /// </summary>
        public async UniTask ShowSkillLevelUp(EntitySkillGroup skillGroup,  Func<Skill, SkillTableEntry, UniTask> onSkillLevelUp)
        {
            _skillGroup = skillGroup;
            
            var skills = _skillGroup.GetRegistedSkills();
            
            var slotIndex = 0;
            foreach (var skill in skills)
            {
                var nextSkillEntry =
                    _tableRepository.GetTableEntry<SkillTableEntry>(skill.SkillTableEntry.CalculateNextLevelSkillId());

                // 스킬이 최대 레벨이면 다음 레벨 ID로 반환 안되므로 표시 X
                if (nextSkillEntry == null)
                    continue;

                if (_slots.Count <= slotIndex)
                {
                    var slotInstance = Instantiate(_skillLevelUpSlotPrefab, transform);
                    _slots.Add(slotInstance);
                }

                var slot = _slots[slotIndex];
                slot.gameObject.SetActive(true);
                
                slot._currentSkillInfo.SetInfo(skill.SkillTableEntry.SkillName
                    , skill.SkillTableEntry.SkillDescription
                    , skill.SkillTableEntry.Cooldown
                    , await _sceneResourceProvider.GetAssetAsync<Sprite>(skill.SkillTableEntry.SkillIconKey,SpriteAtlasAddresses.SkillIconAtlas));
                
                slot._nextSkillInfo.SetInfo(nextSkillEntry.SkillName
                    , nextSkillEntry.SkillDescription
                    , nextSkillEntry.Cooldown
                    , await _sceneResourceProvider.GetAssetAsync<Sprite>(nextSkillEntry.SkillIconKey,SpriteAtlasAddresses.SkillIconAtlas));
                
                slot.SetSelectHandler(async () =>
                {
                    HideSlot();
                    await onSkillLevelUp(skill, nextSkillEntry);
                    Hide();
                });
                
                slotIndex++;
            }

            if (slotIndex > 0)
            {
                base.Show();
                Time.timeScale = 0f; // TODO: 임시코드, 타임스케일 조정은 별도의 시간 매니저로 분리 필요
            }
        }

        private void HideSlot()
        {
            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);
        }
        
        public override void Hide()
        {
            // TODO: 임시코드, 타임스케일 조정은 별도의 시간 매니저로 분리 필요
            Time.timeScale = 1f;
            
            base.Hide();
        }
    }
}
