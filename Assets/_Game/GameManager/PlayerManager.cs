using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 초기화, 입력 바인딩, 스킬·인벤토리 관리를 담당합니다.
    /// StatGroup, SkillGroup, Inventory를 소유하며 플레이어 캐릭터(EntityBase)에 주입합니다.
    /// </summary>
    public class PlayerManager
    {
        private const int ExpPerLevel = 100;

        public EntityStatGroup StatGroup { get; private set; }
        public EntitySkillGroup SkillGroup { get; private set; }
        public Inventory Inventory { get; private set; }

        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;

        public int Level => _level;
        public int Exp => _exp;

        private int _level = 1;
        private int _exp;

        private InputManager _inputManager;
        private UIManager _uIManager;
        private IItemFactory _itemFactory;
        private ITableRepository _tableRepository;
        private ISkillFactory _skillFactory;
        private ISceneResourceProvider _sceneResourceProvider;

        private Skill _skill1;
        private Skill _skill2;
        
        private EntityBase _playerEntity;
        private PlayerConfigTableEntry _playerConfigTableEntry;
        private MovementComponent _movementComponent;
        private DashComponent _dashComponent;
        private InteractComponent _interactComponent;
        private HealthComponent _healthComponent;
        
        private HealthBarHudUI _healthBarUI;
        private ExpGaugeHudUI _expGaugeHudUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        
        private SkillCooldownSlot _weaponCooldownUI;
        private SkillCooldownSlot _skill1CooldownUI;
        private SkillCooldownSlot _skill2CooldownUI;
        private Skill _boundWeaponActiveSkill;

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
            Inventory.OnItemUse += HandleItemUseRequested;
        }

        private async void HandleItemUseRequested(Item item)
        {
            await item.ExecuteUseSkill(_playerEntity);
        }

        /// <summary>
        /// 경험치를 추가합니다.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount <= 0) return;
            var levelBefore = _level;
            _exp += amount;
            while (_exp >= ExpPerLevel)
            {
                _exp -= ExpPerLevel;
                _level++;
            }
            OnExpChanged?.Invoke(_exp);
            if (_level != levelBefore)
                OnLevelChanged?.Invoke(_level);
        }

        /// <summary>
        /// 플레이어 설정으로 StatGroup, SkillGroup, 스킬, 인벤토리를 초기화하고 플레이어 캐릭터에 바인딩합니다.
        /// </summary>
        public async UniTask InitializePlayer(PlayerConfigTableEntry config, EntityBase entity)
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
            
            Inventory.Clear();
            
            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            await Inventory.AddItem(weapon);
            await Inventory.EquipItem(weapon);
            
            await BindPlayerEntity(entity);
        }

        /// <summary>
        /// 기존에 갖고있던 스킬,스탯,인벤토리 데이터를 기반으로 새로운 플레이어 인스턴스를 바인딩합니다.
        /// </summary>
        public async UniTask BindPlayerEntity(EntityBase player)
        {
            if (player == null) 
                return;

            Inventory.SetStatGroup(StatGroup);
            Inventory.SetSkillGroup(SkillGroup);
            player.SetStatGroup(StatGroup);
            player.SetSkillGroup(SkillGroup);
            
            _movementComponent = player.gameObject.AddOrGetComponent<MovementComponent>();
            _interactComponent = player.gameObject.AddOrGetComponent<InteractComponent>();
            var healthComponent = player.gameObject.AddOrGetComponent<HealthComponent>();
            _dashComponent = player.gameObject.AddOrGetComponent<DashComponent>();
            var cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(player.gameObject);
            await cameraTrackComponent.AttachCameraAsync();
            healthComponent.FullHeal();
            healthComponent.OnDeath += () => Object.Destroy(player.gameObject);
            
            _playerEntity = player;

            _skillCooldownHudUI = await _uIManager.CreateUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud, true);
            _skillCooldownHudUI.Clear();

            _weaponCooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();
            _skill1CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();
            _skill2CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();

            OnWeaponEquipped(Inventory.EquippedWeapon);
            Inventory.OnWeaponEquipped += OnWeaponEquipped;

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

            player.OnDestroyed += UnbindPlayerEntity;

            _healthBarUI = await _uIManager.CreateUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud, true);
            _healthBarUI.SetHealthAndMaxHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);
            healthComponent.OnHealthChanged += _healthBarUI.SetHealthAndMaxHealth;
            StatGroup.OnStatChanged += HandleMaxHpChanged;

            _expGaugeHudUI = await _uIManager.CreateUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud, true);
            _expGaugeHudUI.SetLevel(Level);
            _expGaugeHudUI.SetExp(Exp);
            _expGaugeHudUI.SetMaxExp(ExpPerLevel);
            OnLevelChanged += _expGaugeHudUI.SetLevel;
            OnExpChanged += _expGaugeHudUI.SetExp;

            SubscribeInput();
        }

        /// <summary>
        /// 현재 플레이어 인스턴스 연결을 해제합니다.
        /// </summary>
        public void UnbindPlayerEntity(EntityBase player)
        {
            if (_playerEntity != player) 
                return;
            
            if (_playerEntity.TryGetComponent(out HealthComponent healthComponent) && _healthBarUI != null)
            {
                healthComponent.OnHealthChanged -= _healthBarUI.SetHealthAndMaxHealth;
                StatGroup.OnStatChanged -= HandleMaxHpChanged;
                _healthBarUI.Hide();
            }

            if (_skillCooldownHudUI != null)
            {
                Inventory.OnWeaponEquipped -= OnWeaponEquipped;
                _boundWeaponActiveSkill.OnCooldownChanged -= _weaponCooldownUI.SetCooldown;
                _boundWeaponActiveSkill = null;
                _skillCooldownHudUI.Clear();
                _skillCooldownHudUI.Hide();
            }
            
            if (_expGaugeHudUI != null)
            {
                OnLevelChanged -= _expGaugeHudUI.SetLevel;
                OnExpChanged -= _expGaugeHudUI.SetExp;
                _expGaugeHudUI.Hide();
            }

            _interactComponent = null;
            _playerEntity = null;
        }

        private void OnWeaponEquipped(Item weapon)
        {
            if (_weaponCooldownUI == null)
            {
                return;
            }

            if (_boundWeaponActiveSkill != null)
            {
                _boundWeaponActiveSkill.OnCooldownChanged -= _weaponCooldownUI.SetCooldown;
                _boundWeaponActiveSkill = null;
            }

            if (weapon?.ActiveSkill == null)
            {
                _weaponCooldownUI.SetMaxCooldown(0f);
                _weaponCooldownUI.SetCooldown(0f);
                return;
            }

            _boundWeaponActiveSkill = weapon.ActiveSkill;
            _weaponCooldownUI.SetMaxCooldown(_boundWeaponActiveSkill.MaxCooldown);
            _weaponCooldownUI.SetSkillIcon(_boundWeaponActiveSkill.Icon);
            _weaponCooldownUI.SetCooldown(_boundWeaponActiveSkill.Cooldown);
            _boundWeaponActiveSkill.OnCooldownChanged += _weaponCooldownUI.SetCooldown;
        }

        private void HandleMaxHpChanged(StatType type, int newValue)
        {
            if (type != StatType.Hp || _healthBarUI == null || _playerEntity == null)
            {
                return;
            }

            _healthBarUI.SetMaxHealth(newValue);
        }

        private void SubscribeInput()
        {
            if (_inputManager == null) 
                return;
            
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
            if (_inputManager == null) 
                return;
            
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
            if (_playerEntity == null) 
                return;
            
            if (_movementComponent != null)
            {
                _movementComponent.Direction = input;
            }
        }

        private void HandleDashPressed()
        {
            if (_playerEntity == null)
                return;
            
            _dashComponent?.StartDash();
        }

        private void HandleWeaponAttack()
        {
            if (_playerEntity == null) 
                return;
            
            Inventory.EquippedWeapon?.ExecuteActiveSkill(_playerEntity).Forget();
        }

        private void HandleSkill1Pressed()
        {
            if (_playerEntity == null) 
                return;
            
            _skill1?.Execute(_playerEntity).Forget();
        }

        private void HandleSkill2Pressed()
        {
            if (_playerEntity == null) 
                return;
            
            _skill2?.Execute(_playerEntity).Forget();
        }

        private void HandleInteractPressed()
        {
            if (_playerEntity == null) 
                return;
            
            _interactComponent = _interactComponent == null ? _playerEntity.GetComponent<InteractComponent>() : _interactComponent; 
            _interactComponent?.TryInteract();
        }

        private async void HandleEscapePressed()
        {
            var inventoryUI = await _uIManager.CreateUIAsync<InventoryUI>(UIAddresses.UI_Inventory, true);

            if (inventoryUI.gameObject.activeSelf)
            {
                inventoryUI.Hide();
            }
            else
            {
                inventoryUI.Show();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeInput();
            SkillGroup.Clear();
        }
    }
}
