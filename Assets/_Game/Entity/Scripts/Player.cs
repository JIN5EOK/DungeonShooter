using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace DungeonShooter
{
    public class Player : EntityBase
    {
        private InputManager _inputManager;
        private Inventory _inventory;
        private PlayerConfigTableEntry _playerConfigTableEntry;
        private ISceneResourceProvider _sceneResourceProvider;
        private IItemFactory _itemFactory;
        private ITableRepository _tableRepository;
        private UIManager _uIManager;
        
        private HealthComponent _healthComponent;
        private MovementComponent _movementComponent;
        private Skill _skill1;
        private Skill _skill2;
        private InteractComponent _interactComponent;
        private DashComponent _dashComponent;
        private CameraTrackComponent _cameraTrackComponent;
        
        private HealthBarHudUI _healthBarUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _skill1CooldownUI;
        private SkillCooldownSlot _skill2CooldownUI;

        private bool _isDead;
        
        private ISkillFactory _skillFactory;

        [Inject]
        private void Construct(InputManager inputManager
            , Inventory inventory
            , ISceneResourceProvider sceneResourceProvider
            , IItemFactory itemFactory
            , ITableRepository tableRepository
            , UIManager uIManager
            , ISkillFactory skillFactory)
        {
            _inputManager = inputManager;
            _inventory = inventory;
            _sceneResourceProvider = sceneResourceProvider;
            _itemFactory = itemFactory;
            _tableRepository = tableRepository;
            _uIManager = uIManager;
            _skillFactory = skillFactory;
        }

        public async UniTask Initialize(PlayerConfigTableEntry playerConfigTableEntry)
        {
            if (playerConfigTableEntry == null)
            {
                LogHandler.LogWarning<Player>($"PlayerConfigTableEntry를 찾을 수 없습니다.");
                return;
            }
            _playerConfigTableEntry = playerConfigTableEntry;


            // StatGroup 생성·주입
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(_playerConfigTableEntry.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<Player>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {_playerConfigTableEntry.StatsId}");
                return;
            }

            var statGroup = new EntityStatGroup();
            statGroup.Initialize(statsEntry);
            SetStatGroup(statGroup);


            var skillGroup = new EntitySkillGroup();
            SetSkillGroup(skillGroup);

            _skill1 = await _skillFactory.CreateSkillAsync(_playerConfigTableEntry.Skill1Id);
            _skill2 = await _skillFactory.CreateSkillAsync(_playerConfigTableEntry.Skill2Id);
            if (_skill1 != null)
            {
                skillGroup.Regist(_skill1);
            }
            if (_skill2 != null)
            {
                skillGroup.Regist(_skill2);
            }

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

            _inventory.SetStatGroup(StatGroup);
            _inventory.SetSkillGroup(SkillGroup);
            _inventory.SetOwner(this);
            var weapon = await _itemFactory.CreateItemAsync(_playerConfigTableEntry.StartWeaponId);
            await _inventory.AddItem(weapon);
            await _inventory.EquipItem(weapon);
            var weapon2 = await _itemFactory.CreateItemAsync(15000002);
            await _inventory.AddItem(weapon2);
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _interactComponent = gameObject.AddOrGetComponent<InteractComponent>();
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _dashComponent = gameObject.AddOrGetComponent<DashComponent>();
            _cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(gameObject);
            await _cameraTrackComponent.AttachCameraAsync();
            _healthComponent.FullHeal();
            _healthComponent.OnDeath += HandleDeath;

            _healthBarUI = await _uIManager.CreateUIAsync<HealthBarHudUI>(UIAddresses.UI_HpHud, true);
            _healthBarUI.SetHealth(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
            _healthComponent.OnHealthChanged += _healthBarUI.SetHealth;
            
            SubscribeInputEvent();
        }

        // ==================== 입력 매니저 이벤트 구독/해제 ====================
        /// <summary>
        /// 입력 매니저 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInputEvent()
        {
            if (_inputManager == null) return;

            _inputManager.OnMoveInputChanged += HandleMoveInputChanged;
            _inputManager.OnWeaponAttack += HandleWeaponAttackInput;
            _inputManager.OnSkill1Pressed += HandleSkill1Input;
            _inputManager.OnSkill2Pressed += HandleSkill2Input;
            _inputManager.OnInteractPressed += HandleInteractInput;
            _inputManager.OnDashPressed += HandleDashInput;
            _inputManager.OnEscapePressed += HandleEscapeInput;
        }

        // ==================== 입력 처리 ====================
        private void HandleMoveInputChanged(Vector2 input)
        {
            _movementComponent.Direction = input;
        }

        private void HandleDashInput()
        {
            _dashComponent?.StartDash();
        }

        private void HandleWeaponAttackInput()
        {
            _inventory.EquippedWeapon?.ExecuteActiveSkill(this).Forget();
        }

        private void HandleSkill1Input()
        {
            _skill1?.Execute(this).Forget();
        }

        private void HandleSkill2Input()
        {
            _skill2?.Execute(this).Forget();
        }
        
        private void HandleInteractInput()
        {
            _interactComponent?.TryInteract();
        }

        private async void HandleEscapeInput()
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
        
        /// <summary>
        /// 사망 처리
        /// </summary>
        private void HandleDeath()
        {
            if (_isDead) return; // 중복 호출 방지

            _isDead = true;

            LogHandler.Log<Player>("플레이어 사망!");

            // 모든 입력 및 로직 비활성화
            enabled = false;

            StartCoroutine(GameOverSequence());
        }

        /// <summary>
        /// 게임 오버 시퀀스, 나중에 분리 필요
        /// </summary>
        private IEnumerator GameOverSequence()
        {
            yield return new WaitForSeconds(1f); // 1초 대기

            // 페이드 아웃 효과 (선택사항)
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var fadeTime = 1f;
                var startColor = spriteRenderer.color;

                for (float t = 0; t < fadeTime; t += Time.deltaTime)
                {
                    var alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f); // 추가 대기

            LogHandler.Log<Player>("게임 오버! 씬 재시작 중...");

            // 씬 재시작
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }

        private void OnDestroy()
        {
            _healthComponent.OnHealthChanged -= _healthBarUI.SetHealth;
            _skillCooldownHudUI.RemoveSkillCooldownSlot(_skill1CooldownUI);
            _skillCooldownHudUI.RemoveSkillCooldownSlot(_skill2CooldownUI);
            UnsubscribeInputEvent();
        }

        /// <summary>
        /// 입력 매니저 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeInputEvent()
        {
            if (_inputManager == null) return;

            _inputManager.OnMoveInputChanged -= HandleMoveInputChanged;
            _inputManager.OnWeaponAttack -= HandleWeaponAttackInput;
            _inputManager.OnSkill1Pressed -= HandleSkill1Input;
            _inputManager.OnSkill2Pressed -= HandleSkill2Input;
            _inputManager.OnDashPressed -= HandleDashInput;
            _inputManager.OnInteractPressed -= HandleInteractInput;
            _inputManager.OnEscapePressed -= HandleEscapeInput;
        }
    }
}
