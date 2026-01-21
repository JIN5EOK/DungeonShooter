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

    // ==================== 상호작용 ====================
    /// <summary>
    /// 상호작용 가능한 오브젝트를 등록합니다.
    /// </summary>
    public void RegisterInteractable(IInteractable interactable)
    {
        _interactComponent?.RegisterInteractable(interactable);
    }

    /// <summary>
    /// 상호작용 가능한 오브젝트를 제거합니다.
    /// </summary>
    public void UnregisterInteractable(IInteractable interactable)
    {
        _interactComponent?.UnregisterInteractable(interactable);
    }


    // ==================== 스킬 ====================
    private void CastSkill1()
    {
        // TODO: 스킬 1 비활성화 (쿨다운 시스템 제거 작업 중)
    }

    private void CastSkill2()
    {
        // TODO: 스킬 2 비활성화 (쿨다운 시스템 제거 작업 중)
    }

    private void CastSkill3Async()
    {
        // TODO: 스킬 3 비활성화 (쿨다운 시스템 제거 작업 중)
    }

    // ==================== 스킬 효과 함수들 ====================
    
    private void PerformRangeAttack(Vector2 direction, float range, float radius, int damage)
    {
        var attackPos = (Vector2)transform.position + direction * (range / 2);
        
        Debug.DrawLine(transform.position, attackPos + direction * range, Color.red, 0.5f);
        
        var hits = Physics2D.OverlapCircleAll(attackPos, radius);
        var hitCount = 0;
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(GameTags.Enemy))
            {
                var enemyHealth = hit.GetComponent<HealthComponent>();
                if (enemyHealth != null && !enemyHealth.IsDead)
                {
                    enemyHealth.TakeDamage(damage);
                    hitCount++;
                    Debug.Log($"[스킬1] {hit.name}에게 {damage} 데미지!");
                }
            }
        }
        
        if (hitCount > 0)
        {
            Debug.Log($"[스킬1] 총 {hitCount}마리 적중!");
        }
    }

    private void PerformSpinAttack(float radius, int damage)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        var hitCount = 0;
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(GameTags.Enemy))
            {
                var enemyHealth = hit.GetComponent<HealthComponent>();
                if (enemyHealth != null && !enemyHealth.IsDead)
                {
                    enemyHealth.TakeDamage(damage);
                    hitCount++;
                    Debug.Log($"[스킬2] {hit.name}에게 {damage} 데미지!");
                }
            }
        }
        
        if (hitCount > 0)
        {
            Debug.Log($"[스킬2] 총 {hitCount}마리 적중!");
        }
        
        DebugDrawCircle(transform.position, radius, Color.yellow, 0.5f);
    }

    /// <summary>
    /// 전방으로 점프한 뒤 착지 지점에 범위 공격
    /// </summary>
    private async Awaitable PerformJumpAttackAsync(float targetDistance, float radius, int damage, System.Threading.CancellationToken cancellationToken)
    {
        _isJumping = true;
        
        var startPos = (Vector2)transform.position;
        var targetPos = startPos + _movementComponent.Direction * targetDistance;
        
        Debug.Log($"[스킬3] 점프 시작! {startPos} → {targetPos}");
        
        // 스킬 범위 시각화 (착지 지점에 고정 표시) - 점프 시작 시점에 착지 위치 계산
        if (showSkillRanges && skill3Visualizer != null)
        {
            skill3Visualizer.transform.position = targetPos; // 착지 목표 위치에 고정
            skill3Visualizer.gameObject.SetActive(true);
            skill3Visualizer.Show();
        }
        
        var elapsedTime = 0f;
        
        try
        {
            // 점프 중 이동
            while (elapsedTime < jumpDuration)
            {
                // 취소 확인
                cancellationToken.ThrowIfCancellationRequested();
                
                elapsedTime += Time.deltaTime;
                var progress = elapsedTime / jumpDuration;
                
                // 포물선 움직임
                var currentPos = Vector2.Lerp(startPos, targetPos, progress);
                var height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                
                transform.position = new Vector3(currentPos.x, currentPos.y + height, 0);
                rb.linearVelocity = Vector2.zero;
                
                // 다음 프레임까지 대기
                await Awaitable.NextFrameAsync(cancellationToken);
            }
            
            // 최종 위치로 이동
            transform.position = targetPos;
            
            // 착지 시 데미지 적용
            Debug.Log($"[스킬3] 착지!");
            var hits = Physics2D.OverlapCircleAll(targetPos, radius);
            var hitCount = 0;
            
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag(GameTags.Enemy))
                {
                    var enemyHealth = hit.GetComponent<HealthComponent>();
                    if (enemyHealth != null && !enemyHealth.IsDead)
                    {
                        enemyHealth.TakeDamage(damage);
                        hitCount++;
                        Debug.Log($"[스킬3] {hit.name}에게 {damage} 데미지!");
                    }
                }
            }
            
            if (hitCount > 0)
            {
                Debug.Log($"[스킬3] 총 {hitCount}마리 적중!");
            }
            
            DebugDrawCircle(targetPos, radius, Color.magenta, 0.5f);
        }
        finally
        {
            _isJumping = false;
        }
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
