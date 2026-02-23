using DG.Tweening;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 쿨다운 슬롯들을 표시하는 HUD
    /// </summary>
    public class SkillCooldownHudUI : HudUI
    {
        [Header("레이아웃")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private SkillCooldownSlot _skillCooldownSlotPrefab;

        private readonly SkillCooldownSlot[] _idxToSlots = new SkillCooldownSlot[Constants.SkillSlotMaxCount];
        private ISkillSlotViewModel _skillSlotViewModel;
        
        [Inject]
        public void Construct(ISkillSlotViewModel skillSlotViewModel)
        {
            _skillSlotViewModel = skillSlotViewModel;
            _skillSlotViewModel.OnSkillSlotChanged += SkillSlotViewModelChanged;
        }
        
        public override void Show()
        {
            base.Show();
            for (int i = 0; i < Constants.SkillSlotMaxCount; i++)
            {
                var skill = _skillSlotViewModel.GetActiveSkill(i);

                if (skill == null)
                    RemoveSkillCooldownSlot(i);
                else
                    SetSkillCooldownSlot(i, skill);
            }
        }

        private SkillCooldownSlot GetOrCreateSlot(int idx)
        {
            if (_idxToSlots[idx] == null)
            {
                _idxToSlots[idx] = Instantiate(_skillCooldownSlotPrefab, _content);
            }
            
            return _idxToSlots[idx];
        }
        
        private void SkillSlotViewModelChanged(int idx, Skill skill)
        {
            if (idx < 0 || idx > _idxToSlots.Length)
            {
                LogHandler.LogWarning<SkillCooldownHudUI>("스킬 슬롯 범위를 벗어났습니다");
                return;
            }

            SetSkillCooldownSlot(idx, skill);
        }
        
        /// <summary>
        /// 쿨다운 슬롯에 스킬을 추가한다.
        /// </summary>
        private void SetSkillCooldownSlot(int idx, Skill skill)
        {
            if (idx < 0 || idx > _idxToSlots.Length)
            {
                LogHandler.LogWarning<SkillCooldownHudUI>("슬롯 범위를 벗어났습니다");
                return;
            }

            var slot = GetOrCreateSlot(idx);
            slot.gameObject.SetActive(true);
            slot.SetSkill(skill);
        }

        private void RemoveSkillCooldownSlot(int idx)
        {
            if (idx < 0 || idx > _idxToSlots.Length)
            {
                LogHandler.LogWarning<SkillCooldownHudUI>("슬롯 범위를 벗어났습니다");
                return;
            }
            
            GetOrCreateSlot(idx).gameObject.SetActive(false);
        }

        private void Clear()
        {
            for (int i = 0; i < Constants.SkillSlotMaxCount; i++)
            {
                RemoveSkillCooldownSlot(i);
            }
        }
        
        protected override void OnDestroy()
        {
            _skillSlotViewModel.OnSkillSlotChanged -= SkillSlotViewModelChanged;
            base.OnDestroy();
            Clear();
        }
    }
}
