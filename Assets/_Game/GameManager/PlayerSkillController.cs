using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 고유 스킬을 담당합니다.
    /// SkillGroup 소유, 엔티티 바인딩, 스킬 쿨다운 HUD·레벨업 UI 연동을 담당합니다.
    /// </summary>
    public class PlayerSkillController
    {
        public EntitySkillGroup SkillGroup { get; private set; } = new EntitySkillGroup();
        public EntityBase PlayerInstance => _playerInstance;

        private ISkillFactory _skillFactory;
        private Inventory _inventory;
        private UIManager _uIManager;
        private PlayerStatusController _playerStatusController;

        private Skill _skill1;
        private Skill _skill2;
        private EntityBase _playerInstance;
        private SkillLevelUpUI _skillLevelUpUI;
        private SkillCooldownSlot _skill1CooldownSlot;
        private SkillCooldownSlot _skill2CooldownSlot;
        private Skill _boundSkill1;
        private Skill _boundSkill2;

        [Inject]
        private void Construct(ISkillFactory skillFactory, Inventory inventory, UIManager uIManager, PlayerStatusController playerStatusController)
        {
            _skillFactory = skillFactory;
            _inventory = inventory;
            _uIManager = uIManager;
            _playerStatusController = playerStatusController;
        }

        /// <summary> 1번 액티브 스킬을 반환합니다. </summary>
        public Skill GetActiveSkill1() => _skill1;

        /// <summary>2번 액티브 스킬을 반환합니다. </summary>
        public Skill GetActiveSkill2() => _skill2;

        /// <summary>
        /// 선택한 플레이어 정보로 스킬 세션을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerSkillController>("PlayerConfigTableEntry가 null입니다.");
                return;
            }
            SkillGroup?.Clear();
            _skill1 = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _skill2 = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_skill1 != null) 
                SkillGroup?.Regist(_skill1);
            if (_skill2 != null) 
                SkillGroup?.Regist(_skill2);
        }

        /// <summary>
        /// 플레이어 엔티티에 SkillGroup만 바인딩합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            if (entity == null) return;

            _inventory.SetSkillGroup(SkillGroup);
            entity.SetSkillGroup(SkillGroup);
            _playerInstance = entity;
            entity.OnDestroyed += UnbindPlayerInstance;
        }

        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase entity)
        {
            if (_playerInstance != entity) return;
            _playerInstance = null;
        }

        /// <summary>
        /// 1번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill1(EntityBase caster)
        {
            if (caster == null) return;
            _skill1?.Execute(caster).Forget();
        }

        /// <summary>
        /// 2번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill2(EntityBase caster)
        {
            if (caster == null) return;
            _skill2?.Execute(caster).Forget();
        }

        /// <summary>
        /// 기존 스킬을 다음 레벨 스킬로 교체합니다. SkillGroup 및 액티브 슬롯(_skill1/_skill2)을 갱신합니다.
        /// </summary>
        public async UniTask ReplaceSkillAsync(Skill oldSkill, SkillTableEntry nextLevelEntry)
        {
            if (oldSkill == null || nextLevelEntry == null)
            {
                LogHandler.LogWarning<PlayerSkillController>("ReplaceSkillAsync: oldSkill 또는 nextLevelEntry가 null입니다.");
                return;
            }

            var newSkill = await _skillFactory.CreateSkillAsync(nextLevelEntry.Id);
            if (newSkill == null)
            {
                LogHandler.LogWarning<PlayerSkillController>($"다음 레벨 스킬 생성 실패. ID: {nextLevelEntry.Id}");
                return;
            }

            SkillGroup.Unregist(oldSkill, true);
            SkillGroup.Regist(newSkill);

            if (oldSkill == _skill1)
                _skill1 = newSkill;
            else if (oldSkill == _skill2)
                _skill2 = newSkill;
        }

        /// <summary>
        /// 스킬 쿨다운 슬롯과 레벨업 UI를 설정합니다. 플레이어 스폰 시 Factory에서 호출합니다.
        /// </summary>
        public async UniTask SetupSkillUIAsync(SkillCooldownSlot skill1Slot, SkillCooldownSlot skill2Slot)
        {
            _skill1CooldownSlot = skill1Slot;
            _skill2CooldownSlot = skill2Slot;
            _playerStatusController.OnLevelChanged += OnPlayerLevelChanged;
            BindSkillCooldownSlots();
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 스킬 UI 구독 및 참조를 해제합니다. 플레이어 디스폰 시 Factory에서 호출합니다.
        /// </summary>
        public void CleanupSkillUI()
        {
            _playerStatusController.OnLevelChanged -= OnPlayerLevelChanged;
            UnbindSkillCooldownSlots();
            _skillLevelUpUI = null;
            _skill1CooldownSlot = null;
            _skill2CooldownSlot = null;
        }

        private void OnPlayerLevelChanged(int level)
        {
            ShowSkillLevelUpUIAsync().Forget();
        }

        private async UniTaskVoid ShowSkillLevelUpUIAsync()
        {
            if (_skillLevelUpUI == null)
                _skillLevelUpUI = await _uIManager.CreateUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp, false);

            await _skillLevelUpUI.ShowSkillLevelUp(
                SkillGroup,
                async (skill, nextEntry) =>
                {
                    await ReplaceSkillAsync(skill, nextEntry);
                    RefreshSkillCooldownSlots();
                });
        }

        private void BindSkillCooldownSlots()
        {
            UnbindSkillCooldownSlots();

            var skill1 = GetActiveSkill1();
            var skill2 = GetActiveSkill2();
            _boundSkill1 = skill1;
            _boundSkill2 = skill2;

            if (skill1 != null && _skill1CooldownSlot != null)
            {
                _skill1CooldownSlot.SetMaxCooldown(skill1.MaxCooldown);
                _skill1CooldownSlot.SetSkillIcon(skill1.Icon);
                skill1.OnCooldownChanged += _skill1CooldownSlot.SetCooldown;
            }
            if (skill2 != null && _skill2CooldownSlot != null)
            {
                _skill2CooldownSlot.SetMaxCooldown(skill2.MaxCooldown);
                _skill2CooldownSlot.SetSkillIcon(skill2.Icon);
                skill2.OnCooldownChanged += _skill2CooldownSlot.SetCooldown;
            }
        }

        private void UnbindSkillCooldownSlots()
        {
            if (_skill1CooldownSlot != null && _boundSkill1 != null)
            {
                _boundSkill1.OnCooldownChanged -= _skill1CooldownSlot.SetCooldown;
                _boundSkill1 = null;
            }
            if (_skill2CooldownSlot != null && _boundSkill2 != null)
            {
                _boundSkill2.OnCooldownChanged -= _skill2CooldownSlot.SetCooldown;
                _boundSkill2 = null;
            }
        }

        private void RefreshSkillCooldownSlots()
        {
            BindSkillCooldownSlots();
        }
    }
}
