using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Jin5eok;

namespace DungeonShooter
{
    public class Player : EntityBase
    {
        [Header("스탯 컴포넌트")]
        [SerializeField] private EntityStatsComponent statsComponent;

        private InputManager _inputManager;
        private HealthComponent _healthComponent;
        private bool _isDead;

        private MovementComponent _movementComponent;
        private SkillComponent _skillComponent;
        private InteractComponent _interactComponent;
        private IStageResourceProvider _resourceProvider;
        private Inventory _inventory;
        
        [Inject]
        private async UniTask Construct(
            IStageResourceProvider resourceProvider, 
            InputManager inputManager,
            Inventory inventory)
        {
            _resourceProvider = resourceProvider;
            _inputManager = inputManager;
            _inventory = inventory;
            SubscribeInputEvent();
            
            _skillComponent = _resourceProvider.AddOrGetComponentWithInejct<SkillComponent>(gameObject);
            await _skillComponent.RegistSkill(0); // 수정 필요
            
            // Inventory 이벤트 구독
            SubscribeInventoryEvents();
        }

        protected override async UniTask Start()
        {
            await base.Start();
            
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _interactComponent = gameObject.AddOrGetComponent<InteractComponent>();
            
            // 체력 이벤트 구독
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _healthComponent.OnDeath += HandleDeath;
        }

        // ==================== 입력 매니저 이벤트 구독/해제 ====================
        /// <summary>
        /// 입력 매니저 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInputEvent()
        {
            if (_inputManager == null) return;

            _inputManager.OnMoveInputChanged += HandleMoveInputChanged;
            _inputManager.OnSkill1Pressed += HandleSkill1Input;
            _inputManager.OnInteractPressed += HandleInteractInput;
        }

        // ==================== 인벤토리 이벤트 구독/해제 ====================
        /// <summary>
        /// 인벤토리 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInventoryEvents()
        {
            if (_inventory == null) return;

            _inventory.OnItemSkillRegister += HandleSkillRegister;
            _inventory.OnItemSkillUnregister += HandleSkillUnregister;
        }

        /// <summary>
        /// 인벤토리 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeInventoryEvents()
        {
            if (_inventory == null) return;

            _inventory.OnItemSkillRegister -= HandleSkillRegister;
            _inventory.OnItemSkillUnregister -= HandleSkillUnregister;
        }

        /// <summary>
        /// 스킬 등록 처리
        /// </summary>
        private async void HandleSkillRegister(int skillEntryId)
        {
            if (_skillComponent == null) return;
            await _skillComponent.RegistSkill(skillEntryId);
        }

        /// <summary>
        /// 스킬 해제 처리
        /// </summary>
        private void HandleSkillUnregister(int skillEntryId)
        {
            if (_skillComponent == null) return;
            _skillComponent.UnregistSkill(skillEntryId);
        }
        
        // ==================== 입력 처리 ====================
        private void HandleMoveInputChanged(Vector2 input)
        {
            _movementComponent.Direction = input;
        }

        private void HandleSkill1Input()
        {
            _skillComponent.UseSkill(0, this).Forget();
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
        private System.Collections.IEnumerator GameOverSequence()
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        
        private void OnDestroy()
        {
            UnsubscribeInputEvent();
            UnsubscribeInventoryEvents();
        }

        /// <summary>
        /// 입력 매니저 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeInputEvent()
        {
            if (_inputManager == null) return;

            _inputManager.OnMoveInputChanged -= HandleMoveInputChanged;
            _inputManager.OnSkill1Pressed -= HandleSkill1Input;
            _inputManager.OnInteractPressed -= HandleInteractInput;
        }

    }
}
