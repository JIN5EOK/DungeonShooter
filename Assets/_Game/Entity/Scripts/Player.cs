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

    [Header("스킬")]
    [SerializeField] private float skill1Cooldown = 2f;
    [SerializeField] private float skill2Cooldown = 3f;
    [SerializeField] private float skill3Cooldown = 5f;

    [Header("스킬 데미지")]
    [SerializeField] private int skill1Damage = 20;
    [SerializeField] private int skill2Damage = 15;
    [SerializeField] private int skill3Damage = 35;

    [Header("점프 공격 설정")]
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("스킬 범위 시각화")]
    [Tooltip("스킬 범위를 시각적으로 표시할지 여부")]
    [SerializeField] private bool showSkillRanges = true;
    [SerializeField] private AttackRangeVisualizer skill1Visualizer; // 전방 슬래시
    [SerializeField] private AttackRangeVisualizer skill2Visualizer; // 회전 공격
    [SerializeField] private AttackRangeVisualizer skill3Visualizer; // 점프 공격
    
    private HealthComponent _healthComponent;
    private bool _isJumping;
    private bool _isDead;
    private System.Threading.CancellationTokenSource _jumpCancellationTokenSource;
    private int _maxHealthCache = 0;

    private MovementComponent _movementComponent;
    private DashComponent _dashComponent;
    private SkillComponent _skillComponent;
    private InteractComponent _interactComponent;
    private IStageResourceProvider _resourceProvider;
    [Inject]
    private async UniTask Construct(IStageResourceProvider resourceProvider, InputManager inputManager)
    {
        _resourceProvider = resourceProvider;
        _inputManager = inputManager;
        SubscribeInputEvent();
        
        _skillComponent = _resourceProvider.AddOrGetComponentWithInejct<SkillComponent>(gameObject);
        await _skillComponent.RegistSkill("FireballSkillData");
    }

    protected override async UniTask Start()
    {
        await base.Start();
        
        _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
        
        _dashComponent = gameObject.AddOrGetComponent<DashComponent>();
        
        _interactComponent = gameObject.AddOrGetComponent<InteractComponent>();
        
        // 체력 이벤트 구독
        _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
        _healthComponent.OnDeath += HandleDeath;

        Debug.Log($"[{nameof(Player)}] 체력 시스템 초기화 완료: {_healthComponent.CurrentHealth}/{_healthComponent.MaxHealth}");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            _skillComponent.UseSkill("FireballSkillData", this);
        }
    }

    private void OnDestroy()
    {
        // 취소 토큰 정리
        _jumpCancellationTokenSource?.Cancel();
        _jumpCancellationTokenSource?.Dispose();

        UnsubscribeInputEvent();

        // 이벤트 구독 해제
        if (_healthComponent != null)
        {
            _healthComponent.OnDeath -= HandleDeath;
        }
    }

    // ==================== 입력 매니저 이벤트 구독/해제 ====================
    /// <summary>
    /// 입력 매니저 이벤트를 구독합니다.
    /// </summary>
    private void SubscribeInputEvent()
    {
        if (_inputManager == null) return;

        _inputManager.OnMoveInputChanged += HandleMoveInputChanged;
        _inputManager.OnInteractPressed += HandleInteractInput;
    }

    /// <summary>
    /// 입력 매니저 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnsubscribeInputEvent()
    {
        if (_inputManager == null) return;

        _inputManager.OnMoveInputChanged -= HandleMoveInputChanged;
        _inputManager.OnDashPressed -= HandleDashInput;
        _inputManager.OnSkill1Pressed -= HandleSkill1Input;
        _inputManager.OnSkill2Pressed -= HandleSkill2Input;
        _inputManager.OnSkill3Pressed -= HandleSkill3Input;
        _inputManager.OnInteractPressed -= HandleInteractInput;
    }

    // ==================== 입력 처리 ====================
    private void HandleMoveInputChanged(Vector2 input)
    {
        _movementComponent.Direction = input;
    }
    
    private void HandleDashInput()
    {
        _dashComponent.SetInputs(_movementComponent.Direction);
        _dashComponent.StartDash();
    }

    private void HandleSkill1Input()
    {
        // TODO: 스킬 1 비활성화 (쿨다운 시스템 제거 작업 중)
        // CastSkill1();
    }

    private void HandleSkill2Input()
    {
        // TODO: 스킬 2 비활성화 (쿨다운 시스템 제거 작업 중)
        // CastSkill2();
    }

    private void HandleSkill3Input()
    {
        // TODO: 스킬 3 비활성화 (쿨다운 시스템 제거 작업 중)
        // CastSkill3Async();
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

        Debug.Log($"[{nameof(Player)}] 플레이어 사망!");

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

        Debug.Log($"[{nameof(Player)}] 게임 오버! 씬 재시작 중...");

        // 씬 재시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    }
}
