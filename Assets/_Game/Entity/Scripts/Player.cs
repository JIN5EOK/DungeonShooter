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
        private StageUIManager _stageUIManager;
        
        private HealthComponent _healthComponent;
        private MovementComponent _movementComponent;
        private SkillComponent _skillComponent;
        private InteractComponent _interactComponent;
        private DashComponent _dashComponent;
        private CameraTrackComponent _cameraTrackComponent;
        
        private HealthBarHudUI _healthBarUI;
        private SkillCooldownHudUI _skillCooldownHudUI;
        private SkillCooldownSlot _skill1CooldownUI;
        private SkillCooldownSlot _skill2CooldownUI;
        
        private bool _isDead;
        
        [Inject]
        private void Construct(InputManager inputManager
            , Inventory inventory
            , ISceneResourceProvider sceneResourceProvider
            , IItemFactory itemFactory
            , ITableRepository tableRepository
            , StageUIManager stageUIManager)
        {
            _inputManager = inputManager;
            _inventory = inventory;
            _sceneResourceProvider = sceneResourceProvider;
            _itemFactory = itemFactory;
            _tableRepository = tableRepository;
            _stageUIManager = stageUIManager;
        }

        public async UniTask Initialize(PlayerConfigTableEntry playerConfigTableEntry)
        {
            if (playerConfigTableEntry == null)
            {
                LogHandler.LogWarning<Player>($"PlayerConfigTableEntry를 찾을 수 없습니다.");
                return;
            }
            _playerConfigTableEntry = playerConfigTableEntry;


            // 스탯 테이블 엔트리 로드 및 EntityBase 초기화
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(_playerConfigTableEntry.StatsId);
            if (statsEntry == null)
            {
                LogHandler.LogWarning<Player>($"EntityStatsTableEntry를 찾을 수 없습니다. ID: {_playerConfigTableEntry.StatsId}");
                return;
            }


            base.Initialize(statsEntry);

            _skillComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<SkillComponent>(gameObject);
            var skill1 = await _skillComponent.GetOrRegistSkill(_playerConfigTableEntry.Skill1Id);
            var skill2 = await _skillComponent.GetOrRegistSkill(_playerConfigTableEntry.Skill2Id);

            _skillCooldownHudUI = await _stageUIManager.GetSkillCooldownHudUI();
            _skill1CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();
            _skill2CooldownUI = _skillCooldownHudUI.AddSkillCooldownSlot();

            _skill1CooldownUI.SetMaxCooldown(skill1.MaxCooldown);
            _skill2CooldownUI.SetMaxCooldown(skill2.MaxCooldown);
            _skill1CooldownUI.SetSkillIcon(await _sceneResourceProvider.GetAssetAsync<Sprite>(skill1.SkillTableEntry.SkillIconKey));
            _skill2CooldownUI.SetSkillIcon(await _sceneResourceProvider.GetAssetAsync<Sprite>(skill2.SkillTableEntry.SkillIconKey));
            skill1.OnCooldownChanged += _skill1CooldownUI.SetCooldown;
            skill2.OnCooldownChanged += _skill2CooldownUI.SetCooldown;
            
            _inventory.SetOwner(this);
            var weapon = await _itemFactory.CreateItemAsync(_playerConfigTableEntry.StartWeaponId);
            await _inventory.AddItem(weapon);
            await _inventory.EquipItem(weapon);

            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _interactComponent = gameObject.AddOrGetComponent<InteractComponent>();
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _dashComponent = gameObject.AddOrGetComponent<DashComponent>();
            _cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(gameObject);
            await _cameraTrackComponent.AttachCameraAsync();
            _healthComponent.FullHeal();
            _healthComponent.OnDeath += HandleDeath;

            _healthBarUI = await _stageUIManager.GetHealthBarUI();
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
            _skillComponent?.UseSkill(_playerConfigTableEntry.Skill1Id).Forget();
        }
        
        private void HandleSkill2Input()
        {
            _skillComponent?.UseSkill(_playerConfigTableEntry.Skill2Id).Forget();
        }
        
        private void HandleInteractInput()
        {
            _interactComponent?.TryInteract();
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
        }
    }
}
