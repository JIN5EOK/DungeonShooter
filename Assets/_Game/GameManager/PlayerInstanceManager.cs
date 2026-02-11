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
        private readonly PlayerStatusController _playerStatusController;
        private readonly PlayerSkillController _playerSkillController;
        private readonly Inventory _inventory;
        private readonly UIManager _uIManager;
        private readonly PlayerInputController _playerInputController;

        private EntityBase _currentPlayerEntity;
        private HealthBarHudUI _healthBarUI;
        private ExpGaugeHudUI _expGaugeHudUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _weaponCooldownSlot;
        private readonly SkillCooldownSlot[] _activeSkillCooldownSlots = new SkillCooldownSlot[PlayerSkillSlots.Count];
        private SkillLevelUpUI _skillLevelUpUI;

        private Skill _boundWeaponActiveSkill;
        private readonly Skill[] _boundActiveSkills = new Skill[PlayerSkillSlots.Count];

        [Inject]
        public PlayerInstanceManager(PlayerStatusController playerStatusController
            , PlayerSkillController playerSkillController
            , Inventory inventory
            , UIManager uIManager
            , PlayerInputController playerInputController)
        {
            _playerStatusController = playerStatusController;
            _playerSkillController = playerSkillController;
            _inventory = inventory;
            _uIManager = uIManager;
            _playerInputController = playerInputController;
        }

        public async UniTask BindAsync(EntityBase entity)
        {
            if (entity == null)
                return;

            if (_currentPlayerEntity != null)
                UnbindAndDestroy();

            _currentPlayerEntity = entity;
            
            _playerStatusController.BindPlayerInstance(entity);
            _playerSkillController.BindPlayerInstance(entity);
            _inventory.BindPlayerInstance(entity);

            await SetupPlayerUIAsync();
            _playerInputController.BindPlayerInstance(entity);
        }

        public void UnbindAndDestroy()
        {
            if (_currentPlayerEntity == null)
                return;

            CleanupPlayerUI();
            _playerInputController.UnbindPlayerInstance();

            Object.Destroy(_currentPlayerEntity.gameObject);
            _currentPlayerEntity = null;
        }

        private void OnMaxHealthChanged(StatType type, int value)
        {
            if (type == StatType.Hp)
            {
                _healthBarUI.SetMaxHealth(value);    
            }
        }
        
        private async UniTask SetupPlayerUIAsync()
        {
            _healthBarUI = await _uIManager.CreateUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud, true);
            _healthBarUI.SetHealthAndMaxHealth(_playerStatusController.Hp, _playerStatusController.StatGroup.GetStat(StatType.Hp));
            _playerStatusController.OnHpChanged += _healthBarUI.SetHealth;
            _playerStatusController.StatGroup.OnStatChanged += OnMaxHealthChanged;

            _expGaugeHudUI = await _uIManager.CreateUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud, true);
            _expGaugeHudUI.SetLevel(_playerStatusController.Level);
            _expGaugeHudUI.SetExp(_playerStatusController.Exp);
            _expGaugeHudUI.SetMaxExp(PlayerStatusController.ExpPerLevel);
            _playerStatusController.OnLevelChanged += _expGaugeHudUI.SetLevel;
            _playerStatusController.OnExpChanged += _expGaugeHudUI.SetExp;
            _playerStatusController.OnLevelChanged += OnPlayerLevelChanged;

            _skillCooldownHudUI = await _uIManager.CreateUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud, true);
            _skillCooldownHudUI.Clear();
            _weaponCooldownSlot = _skillCooldownHudUI.AddSkillCooldownSlot();
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill1Index] = _skillCooldownHudUI.AddSkillCooldownSlot();
            _activeSkillCooldownSlots[PlayerSkillSlots.Skill2Index] = _skillCooldownHudUI.AddSkillCooldownSlot();

            BindWeaponCooldownSlot(_inventory.EquippedWeapon);
            _inventory.OnWeaponEquipped += BindWeaponCooldownSlot;

            _playerSkillController.OnActiveSkillChanged += OnActiveSkillChanged;
            
            BindSkillCooldownSlots();
        }

        private void CleanupPlayerUI()
        {
            _playerStatusController.OnHpChanged -= _healthBarUI.SetHealth;
            _playerStatusController.StatGroup.OnStatChanged -= OnMaxHealthChanged;

            if (_expGaugeHudUI != null)
            {
                _playerStatusController.OnLevelChanged -= _expGaugeHudUI.SetLevel;
                _playerStatusController.OnExpChanged -= _expGaugeHudUI.SetExp;
                _playerStatusController.OnLevelChanged -= OnPlayerLevelChanged;
                _expGaugeHudUI.Destroy();
            }

            _skillLevelUpUI = null;

            if (_skillCooldownHudUI != null)
            {
                _playerSkillController.OnActiveSkillChanged -= OnActiveSkillChanged;
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

            _healthBarUI = null;
            _expGaugeHudUI = null;
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
            HandleActiveSkillChanged(PlayerSkillSlots.Skill1Index, _playerSkillController.GetActiveSkill(PlayerSkillSlots.Skill1Index));
            HandleActiveSkillChanged(PlayerSkillSlots.Skill2Index, _playerSkillController.GetActiveSkill(PlayerSkillSlots.Skill2Index));
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
                _skillLevelUpUI = await _uIManager.CreateUIAsync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp, false);

            await _skillLevelUpUI.ShowSkillLevelUp(
                _playerSkillController.SkillGroup,
                _playerSkillController.ReplaceSkillAsync);
        }
    }
}