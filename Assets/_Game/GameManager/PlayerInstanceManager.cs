using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
/// <summary>
/// 플레이어 엔티티 게임오브젝트 인스턴스에 대한 UI/입력/이벤트 연동 담당
/// </summary>
    public class PlayerInstanceManager
    {
        private readonly PlayerStatusManager _playerStatusManager;
        private readonly PlayerSkillSession _playerSkillSession;
        private readonly Inventory _inventory;
        private readonly UIManager _uIManager;
        private readonly PlayerInputSession _playerInputSession;

        private EntityBase _currentPlayerEntity;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _weaponCooldownSlot;
        private readonly SkillCooldownSlot[] _activeSkillCooldownSlots = new SkillCooldownSlot[PlayerSkillSlots.Count];
        private SkillLevelUpUI _skillLevelUpUI;

        private Skill _boundWeaponActiveSkill;
        private readonly Skill[] _boundActiveSkills = new Skill[PlayerSkillSlots.Count];

        [Inject]
        public PlayerInstanceManager(PlayerStatusManager playerStatusManager
            , PlayerSkillSession playerSkillSession
            , Inventory inventory
            , UIManager uIManager
            , PlayerInputSession playerInputSession)
        {
            _playerStatusManager = playerStatusManager;
            _playerSkillSession = playerSkillSession;
            _inventory = inventory;
            _uIManager = uIManager;
            _playerInputSession = playerInputSession;
        }

        public async UniTask BindAsync(EntityBase entity)
        {
            if (entity == null)
                return;

            if (_currentPlayerEntity != null)
                UnbindAndDestroy();

            _currentPlayerEntity = entity;
            
            _playerSkillSession.BindPlayerInstance(entity);
            _inventory.BindPlayerInstance(entity);

            await SetupPlayerUIAsync();
            _playerInputSession.BindPlayerInstance(entity);
        }

        public void UnbindAndDestroy()
        {
            if (_currentPlayerEntity == null)
                return;

            CleanupPlayerUI();
            _playerInputSession.UnbindPlayerInstance();

            Object.Destroy(_currentPlayerEntity.gameObject);
            _currentPlayerEntity = null;
        }
        
        
        private async UniTask SetupPlayerUIAsync()
        {
            _skillCooldownHudUI = await _uIManager.GetSingletonUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud);
            _skillCooldownHudUI.Clear();
            _weaponCooldownSlot = _skillCooldownHudUI.AddSkillCooldownSlot();
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill1Index] = _skillCooldownHudUI.AddSkillCooldownSlot();
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill2Index] = _skillCooldownHudUI.AddSkillCooldownSlot();

            BindWeaponCooldownSlot(_inventory.EquippedWeapon);
            _inventory.OnWeaponEquipped += BindWeaponCooldownSlot;

            _playerSkillSession.OnActiveSkillChanged += OnActiveSkillChanged;
            
            BindSkillCooldownSlots();
        }

        private void CleanupPlayerUI()
        {
            _skillLevelUpUI = null;

            if (_skillCooldownHudUI != null)
            {
                _playerSkillSession.OnActiveSkillChanged -= OnActiveSkillChanged;
                _inventory.OnWeaponEquipped -= BindWeaponCooldownSlot;

                if (_boundWeaponActiveSkill != null)
                {
                    _boundWeaponActiveSkill.OnCooldownChanged -= _weaponCooldownSlot.SetCooldown;
                    _boundWeaponActiveSkill = null;
                }

                UnbindSkillCooldownSlots();

                _skillCooldownHudUI.Clear();
                _skillCooldownHudUI.Destroy();
            }
            _skillCooldownHudUI = null;
            _weaponCooldownSlot = null;
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill1Index] = null;
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill2Index] = null;
            _boundActiveSkills[PlayerSkillSlots.Skill1Index] = null;
            _boundActiveSkills[PlayerSkillSlots.Skill2Index] = null;
        }

        private void BindWeaponCooldownSlot(Item weapon)
        {
            if (_weaponCooldownSlot == null) return;

            if (_boundWeaponActiveSkill != null)
            {
                _boundWeaponActiveSkill.OnCooldownChanged -= _weaponCooldownSlot.SetCooldown;
                _boundWeaponActiveSkill = null;
            }

            if (weapon?.ActiveSkill == null)
            {
                _weaponCooldownSlot.SetMaxCooldown(0f);
                _weaponCooldownSlot.SetCooldown(0f);
                return;
            }

            _boundWeaponActiveSkill = weapon.ActiveSkill;
            _weaponCooldownSlot.SetMaxCooldown(_boundWeaponActiveSkill.MaxCooldown);
            _weaponCooldownSlot.SetSkillIcon(_boundWeaponActiveSkill.Icon);
            _weaponCooldownSlot.SetCooldown(_boundWeaponActiveSkill.Cooldown);
            _boundWeaponActiveSkill.OnCooldownChanged += _weaponCooldownSlot.SetCooldown;
        }
        
        private void OnActiveSkillChanged(int index, Skill skill)
        {
            HandleActiveSkillChanged(index, skill);
        }

        private void HandleActiveSkillChanged(int index, Skill skill)
        {
            if (index < 0 || index >= PlayerSkillSlots.Count) 
                return;

            var cooldownSlot = _activeSkillCooldownSlots[index];
            
            if (cooldownSlot == null) 
                return;

            var bound = _boundActiveSkills[index];
            
            if (bound != null)
                bound.OnCooldownChanged -= cooldownSlot.SetCooldown;

            _boundActiveSkills[index] = skill;
            
            if (_boundActiveSkills[index] == null)
            {
                cooldownSlot.SetMaxCooldown(0f);
                cooldownSlot.SetCooldown(0f);
            }
            else
            {
                cooldownSlot.SetCooldown(_boundActiveSkills[index].Cooldown);
                cooldownSlot.SetMaxCooldown(_boundActiveSkills[index].MaxCooldown);
                cooldownSlot.SetSkillIcon(_boundActiveSkills[index].Icon);
                _boundActiveSkills[index].OnCooldownChanged += cooldownSlot.SetCooldown;    
            }
        }

        private void BindSkillCooldownSlots()
        {
            HandleActiveSkillChanged(PlayerSkillSlots.Skill1Index, _playerSkillSession.GetActiveSkill(PlayerSkillSlots.Skill1Index));
            HandleActiveSkillChanged(PlayerSkillSlots.Skill2Index, _playerSkillSession.GetActiveSkill(PlayerSkillSlots.Skill2Index));
        }
        
        private void UnbindSkillCooldownSlots()
        {
            if (_activeSkillCooldownSlots[PlayerSkillSlots.Skill1Index] != null && _boundActiveSkills[PlayerSkillSlots.Skill1Index] != null)
            {
                _boundActiveSkills[PlayerSkillSlots.Skill1Index].OnCooldownChanged -= _activeSkillCooldownSlots[PlayerSkillSlots.Skill1Index].SetCooldown;
                _boundActiveSkills[PlayerSkillSlots.Skill1Index] = null;
            }
            if (_activeSkillCooldownSlots[PlayerSkillSlots.Skill2Index] != null && _boundActiveSkills[PlayerSkillSlots.Skill2Index] != null)
            {
                _boundActiveSkills[PlayerSkillSlots.Skill2Index].OnCooldownChanged -= _activeSkillCooldownSlots[PlayerSkillSlots.Skill2Index].SetCooldown;
                _boundActiveSkills[PlayerSkillSlots.Skill2Index] = null;
            }
        }
        
        private void OnPlayerLevelChanged(int level)
        {
            ShowSkillLevelUpUIAsync().Forget();
        }

        private async UniTaskVoid ShowSkillLevelUpUIAsync()
        {
            if (_skillLevelUpUI == null)
                _skillLevelUpUI = await _uIManager.GetSingletonUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp);

            await _skillLevelUpUI.ShowSkillLevelUp(
                _playerSkillSession.SkillContainer,
                _playerSkillSession.ReplaceSkillAsync);
        }
    }
}