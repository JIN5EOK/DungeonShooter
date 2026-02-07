using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 초기화, 입력 바인딩, 스킬·인벤토리 관리를 담당합니다.
    /// StatGroup, SkillGroup, Inventory를 소유하며 아바타(Player)에 주입합니다.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public EntityStatGroup StatGroup { get; private set; }
        public EntitySkillGroup SkillGroup { get; private set; }
        public Inventory Inventory { get; private set; }

        private InputManager _inputManager;
        private UIManager _uIManager;
        private IItemFactory _itemFactory;
        private ITableRepository _tableRepository;
        private ISkillFactory _skillFactory;
        private ISceneResourceProvider _sceneResourceProvider;

        private Skill _skill1;
        private Skill _skill2;
        private Player _currentAvatar;
        private PlayerConfigTableEntry _playerConfigTableEntry;

        private HealthBarHudUI _healthBarUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _skill1CooldownUI;
        private SkillCooldownSlot _skill2CooldownUI;

        [Inject]
        private void Construct(InputManager inputManager
            , Inventory inventory
            , UIManager uIManager
            , IItemFactory itemFactory
            , ITableRepository tableRepository
            , ISkillFactory skillFactory
            , ISceneResourceProvider sceneResourceProvider)
        {
            _inputManager = inputManager;
            Inventory = inventory;
            _uIManager = uIManager;
            _itemFactory = itemFactory;
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// 플레이어 설정으로 StatGroup, SkillGroup, 스킬, 인벤토리를 초기화하고 입력을 구독합니다.
        /// </summary>
        public async UniTask Initialize(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerManager>("PlayerConfigTableEntry가 null입니다.");
                return;
            }

            _playerConfigTableEntry = config;

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<PlayerManager>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {config.StatsId}");
                return;
            }

            StatGroup = new EntityStatGroup();
            StatGroup.Initialize(statsEntry);

            SkillGroup = new EntitySkillGroup();
            _skill1 = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _skill2 = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            if (_skill1 != null) SkillGroup.Regist(_skill1);
            if (_skill2 != null) SkillGroup.Regist(_skill2);

            Inventory.SetStatGroup(StatGroup);
            Inventory.SetSkillGroup(SkillGroup);

            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            await Inventory.AddItem(weapon);
            await Inventory.EquipItem(weapon);
            var weapon2 = await _itemFactory.CreateItemAsync(15000002);
            await Inventory.AddItem(weapon2);

            SubscribeInput();
        }

        /// <summary>
        /// Player에 StatGroup/SkillGroup/Inventory를 바인딩하고, 씬용 컴포넌트·UI를 설정합니다.
        /// </summary>
        public async UniTask BindPlayerEntity(Player avatar)
        {
            if (avatar == null) return;

            _currentAvatar = avatar;
            avatar.SetStatGroup(StatGroup);
            avatar.SetSkillGroup(SkillGroup);
            Inventory.SetOwner(avatar);

            _skillCooldownHudUI = await _uIManager.CreateUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud, true);
            _skill1CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();
            _skill2CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();
            if (_skill1 != null)
            {
                _skill1CooldownUI.SetMaxCooldown(_skill1.MaxCooldown);
                _skill1CooldownUI.SetSkillIcon(_skill1.Icon);
                _skill1.OnCooldownChanged += _skill1CooldownUI.SetCooldown;
            }
            if (_skill2 != null)
            {
                _skill2CooldownUI.SetMaxCooldown(_skill2.MaxCooldown);
                _skill2CooldownUI.SetSkillIcon(_skill2.Icon);
                _skill2.OnCooldownChanged += _skill2CooldownUI.SetCooldown;
            }

            await avatar.SetupSceneComponents(this);

            var healthComponent = avatar.GetComponent<HealthComponent>();
            if (healthComponent != null)
            {
                _healthBarUI = await _uIManager.CreateUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud, true);
                _healthBarUI.SetHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);
                healthComponent.OnHealthChanged += _healthBarUI.SetHealth;
            }
        }

        /// <summary>
        /// 현재 플레이어 연결을 해제합니다.
        /// </summary>
        public void UnbindPlayerEntity(Player avatar)
        {
            if (_currentAvatar != avatar) return;

            var healthComponent = _currentAvatar != null ? _currentAvatar.GetComponent<HealthComponent>() : null;
            if (healthComponent != null && _healthBarUI != null)
            {
                healthComponent.OnHealthChanged -= _healthBarUI.SetHealth;
            }

            if (_skillCooldownHudUI != null)
            {
                if (_skill1CooldownUI != null)
                {
                    _skillCooldownHudUI.RemoveSkillCooldownSlot(_skill1CooldownUI);
                    if (_skill1 != null) _skill1.OnCooldownChanged -= _skill1CooldownUI.SetCooldown;
                }
                if (_skill2CooldownUI != null)
                {
                    _skillCooldownHudUI.RemoveSkillCooldownSlot(_skill2CooldownUI);
                    if (_skill2 != null) _skill2.OnCooldownChanged -= _skill2CooldownUI.SetCooldown;
                }
            }

            _currentAvatar = null;
        }

        private void SubscribeInput()
        {
            if (_inputManager == null) return;
            _inputManager.OnMoveInputChanged += HandleMoveInputChanged;
            _inputManager.OnWeaponAttack += HandleWeaponAttack;
            _inputManager.OnSkill1Pressed += HandleSkill1Pressed;
            _inputManager.OnSkill2Pressed += HandleSkill2Pressed;
            _inputManager.OnInteractPressed += HandleInteractPressed;
            _inputManager.OnDashPressed += HandleDashPressed;
            _inputManager.OnEscapePressed += HandleEscapePressed;
        }

        private void UnsubscribeInput()
        {
            if (_inputManager == null) return;
            _inputManager.OnMoveInputChanged -= HandleMoveInputChanged;
            _inputManager.OnWeaponAttack -= HandleWeaponAttack;
            _inputManager.OnSkill1Pressed -= HandleSkill1Pressed;
            _inputManager.OnSkill2Pressed -= HandleSkill2Pressed;
            _inputManager.OnInteractPressed -= HandleInteractPressed;
            _inputManager.OnDashPressed -= HandleDashPressed;
            _inputManager.OnEscapePressed -= HandleEscapePressed;
        }

        private void HandleMoveInputChanged(Vector2 input)
        {
            if (_currentAvatar == null) return;
            var movement = _currentAvatar.GetComponent<MovementComponent>();
            if (movement != null) movement.Direction = input;
        }

        private void HandleDashPressed()
        {
            if (_currentAvatar == null) return;
            var dash = _currentAvatar.GetComponent<DashComponent>();
            dash?.StartDash();
        }

        private void HandleWeaponAttack()
        {
            if (_currentAvatar == null) return;
            Inventory.EquippedWeapon?.ExecuteActiveSkill(_currentAvatar).Forget();
        }

        private void HandleSkill1Pressed()
        {
            if (_currentAvatar == null) return;
            _skill1?.Execute(_currentAvatar).Forget();
        }

        private void HandleSkill2Pressed()
        {
            if (_currentAvatar == null) return;
            _skill2?.Execute(_currentAvatar).Forget();
        }

        private void HandleInteractPressed()
        {
            if (_currentAvatar == null) return;
            var interact = _currentAvatar.GetComponent<InteractComponent>();
            interact?.TryInteract();
        }

        private async void HandleEscapePressed()
        {
            var inventoryUI = await _uIManager.CreateUIAsync<InventoryUI>(UIAddresses.UI_Inventory, true);
            if (inventoryUI.gameObject.activeSelf)
                inventoryUI.Hide();
            else
                inventoryUI.Show();
        }

        private void OnDestroy()
        {
            UnsubscribeInput();
        }
    }
}
