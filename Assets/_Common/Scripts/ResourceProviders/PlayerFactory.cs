using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IPlayerFactory
    {
        UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    // TODO: 팩토리의 책임이 너무 비대해져서(UI 바인딩 및 컴포넌트까지 들고 있게 됨, 분리가 필요함)
    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성·이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;
        
        private readonly PlayerStatusController _playerStatusController;
        private readonly PlayerSkillController _playerSkillController;
        private readonly Inventory _inventory;
        private readonly UIManager _uIManager;
        private readonly PlayerInputController _playerInputController;

        private HealthComponent _boundHealthComponent;
        private HealthBarHudUI _healthBarUI;
        private ExpGaugeHudUI _expGaugeHudUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _weaponCooldownSlot;
        private Skill _boundWeaponActiveSkill;

        private EntityBase _currentPlayerEntity;

        [Inject]
        public PlayerFactory(StageContext context
            , ISceneResourceProvider sceneResourceProvider
            , ITableRepository tableRepository
            , PlayerStatusController playerStatusController
            , PlayerSkillController playerSkillController
            , Inventory inventory
            , UIManager uIManager
            , PlayerInputController playerInputController)
        {
            _stageContext = context;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
            _playerStatusController = playerStatusController;
            _playerSkillController = playerSkillController;
            _inventory = inventory;
            _uIManager = uIManager;
            _playerInputController = playerInputController;
        }

        /// <summary>
        /// 플레이어 캐릭터를 가져옵니다. 이미 플레이어가 있으면 파괴한 뒤 새로 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                DestroyCurrentPlayerIfExists();
                var playerAddress = GetPlayerAddress();
                var playerInstance = await _sceneResourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = await InitializePlayerInstance(playerInstance);
                _currentPlayerEntity = entity;
                return entity;
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 가져옵니다. 이미 플레이어가 있으면 파괴한 뒤 새로 생성합니다.
        /// </summary>
        public EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                DestroyCurrentPlayerIfExists();
                var playerAddress = GetPlayerAddress();
                var playerInstance = _sceneResourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = InitializePlayerInstance(playerInstance).GetAwaiter().GetResult();
                _currentPlayerEntity = entity;
                return entity;
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 현재 플레이어 인스턴스가 있으면 UI/입력 정리 후 파괴합니다.
        /// </summary>
        private void DestroyCurrentPlayerIfExists()
        {
            if (_currentPlayerEntity == null) 
                return;

            var toDestroy = _currentPlayerEntity;
            _currentPlayerEntity = null;
            toDestroy.OnDestroyed -= CleanupPlayerUIAndInput;
            CleanupPlayerUIAndInput(toDestroy);
            Object.Destroy(toDestroy.gameObject);
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            if (string.IsNullOrEmpty(config.GameObjectKey))
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 게임오브젝트 키가 설정되지 않았습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            return config.GameObjectKey;
        }

        /// <summary>
        /// Player 게임오브젝트 초기화, 컴포넌트 부착, 바인딩, UI 연동
        /// </summary>
        private async UniTask<EntityBase> InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;
            var entity = playerInstance.AddComponent<EntityBase>();

            _playerStatusController.BindPlayerInstance(entity);
            _playerSkillController.BindPlayerInstance(entity);
            _inventory.BindOwner(entity);

            entity.gameObject.AddOrGetComponent<MovementComponent>();
            entity.gameObject.AddOrGetComponent<InteractComponent>();
            var healthComponent = entity.gameObject.AddOrGetComponent<HealthComponent>();
            entity.gameObject.AddOrGetComponent<DashComponent>();
            var cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(entity.gameObject);
            await cameraTrackComponent.AttachCameraAsync();
            healthComponent.FullHeal();
            healthComponent.OnDeath += () => Object.Destroy(entity.gameObject);

            _playerStatusController.SyncFromHealthComponent(healthComponent);
            _boundHealthComponent = healthComponent;
            entity.OnDestroyed += CleanupPlayerUIAndInput;

            await SetupPlayerUIAsync();

            _playerInputController.BindPlayerInstance(entity);

            return entity;
        }

        private async UniTask SetupPlayerUIAsync()
        {
            _healthBarUI = await _uIManager.CreateUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud, true);
            _healthBarUI.SetHealthAndMaxHealth(_boundHealthComponent.CurrentHealth, _boundHealthComponent.MaxHealth);
            _boundHealthComponent.OnHealthChanged += _healthBarUI.SetHealthAndMaxHealth;

            _expGaugeHudUI = await _uIManager.CreateUIAsync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud, true);
            _expGaugeHudUI.SetLevel(_playerStatusController.Level);
            _expGaugeHudUI.SetExp(_playerStatusController.Exp);
            _expGaugeHudUI.SetMaxExp(PlayerStatusController.ExpPerLevel);
            _playerStatusController.OnLevelChanged += _expGaugeHudUI.SetLevel;
            _playerStatusController.OnExpChanged += _expGaugeHudUI.SetExp;

            _skillCooldownHudUI = await _uIManager.CreateUIAsync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud, true);
            _skillCooldownHudUI.Clear();
            _weaponCooldownSlot = _skillCooldownHudUI.AddSkillCooldownSlot();
            var skill1CooldownSlot = _skillCooldownHudUI.AddSkillCooldownSlot();
            var skill2CooldownSlot = _skillCooldownHudUI.AddSkillCooldownSlot();

            BindWeaponCooldownSlot(_inventory.EquippedWeapon);
            _inventory.OnWeaponEquipped += BindWeaponCooldownSlot;

            await _playerSkillController.SetupSkillUIAsync(skill1CooldownSlot, skill2CooldownSlot);
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

        private void CleanupPlayerUIAndInput(EntityBase entity)
        {
            if (entity != _currentPlayerEntity && _currentPlayerEntity != null)
                return;

            if (_boundHealthComponent != null && _healthBarUI != null)
            {
                _boundHealthComponent.OnHealthChanged -= _healthBarUI.SetHealthAndMaxHealth;
                _healthBarUI.Hide();
            }

            if (_expGaugeHudUI != null)
            {
                _playerStatusController.OnLevelChanged -= _expGaugeHudUI.SetLevel;
                _playerStatusController.OnExpChanged -= _expGaugeHudUI.SetExp;
                _expGaugeHudUI.Hide();
            }

            _playerSkillController.CleanupSkillUI();

            if (_skillCooldownHudUI != null)
            {
                _inventory.OnWeaponEquipped -= BindWeaponCooldownSlot;
                if (_boundWeaponActiveSkill != null)
                {
                    _boundWeaponActiveSkill.OnCooldownChanged -= _weaponCooldownSlot.SetCooldown;
                    _boundWeaponActiveSkill = null;
                }
                _skillCooldownHudUI.Clear();
                _skillCooldownHudUI.Hide();
            }

            _boundHealthComponent = null;
            _healthBarUI = null;
            _expGaugeHudUI = null;
            _skillCooldownHudUI = null;
            _weaponCooldownSlot = null;

            _playerInputController.UnbindPlayerInstance();

            _currentPlayerEntity = null;
        }
    }
}
