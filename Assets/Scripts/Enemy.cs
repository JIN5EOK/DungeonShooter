using UnityEngine;
using System.Collections;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Hit,
    Stunned,
    Dead
}

public class Enemy : BaseEntity
{
    [Header("AI 설정")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float idleTime = 2f;

    [Header("피격 설정")]
    [SerializeField] private float hitStunDuration = 0.5f;
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.2f;

    [Header("사망 설정")]
    [SerializeField] private float deathDelay = 1f;
    [SerializeField] private bool disableOnDeath = true;

    private Transform playerTransform;
    private CooldownManager cooldownManager;
    private HealthComponent healthComponent;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // AI 상태
    private EnemyState currentState = EnemyState.Idle;
    private Vector2 patrolStartPos;
    private Vector2 patrolTargetPos;
    private bool isPatrolling;
    private float idleTimer;

    // 피격 관련
    private bool isStunned;
    private Vector2 knockbackDirection;

    protected override void Start()
    {
        base.Start();
        playerTransform = FindFirstObjectByType<PlayerProto>().transform;

        cooldownManager = new CooldownManager();
        cooldownManager.RegisterCooldown("attack", attackCooldown);
        cooldownManager.RegisterCooldown("hitStun", hitStunDuration);

        // SpriteRenderer 초기화
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 순찰 초기화
        patrolStartPos = transform.position;
        SetNewPatrolTarget();

        // HealthComponent 찾기 및 이벤트 구독
        healthComponent = GetComponent<HealthComponent>();
        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandleDeath;
            healthComponent.OnDamaged += HandleDamaged;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] HealthComponent가 없습니다. 적이 죽지 않습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= HandleDeath;
            healthComponent.OnDamaged -= HandleDamaged;
        }
    }

    private void Update()
    {
        // 죽었으면 AI 중지
        if (currentState == EnemyState.Dead) return;

        cooldownManager.UpdateCooldowns();
        UpdateAI();
    }

    private void UpdateAI()
    {
        // 스턴 상태 확인
        if (isStunned && cooldownManager.IsReady("hitStun"))
        {
            isStunned = false;
            currentState = EnemyState.Idle;
        }

        // 스턴 중이면 AI 중지
        if (isStunned) return;

        // 플레이어 감지
        bool playerDetected = IsPlayerInRange(detectionRange);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(playerDetected);
                break;
            case EnemyState.Patrol:
                HandlePatrolState(playerDetected);
                break;
            case EnemyState.Chase:
                HandleChaseState(playerDetected);
                break;
            case EnemyState.Hit:
                // 피격 후 잠시 대기
                if (cooldownManager.IsReady("hitStun"))
                {
                    currentState = playerDetected ? EnemyState.Chase : EnemyState.Idle;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        // 죽었으면 움직임 중지
        if (currentState == EnemyState.Dead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 넉백 처리
        if (isStunned)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
            return;
        }

        // 상태별 움직임
        switch (currentState)
        {
            case EnemyState.Idle:
                rb.linearVelocity = Vector2.zero;
                break;
            case EnemyState.Patrol:
                MoveTowardsTarget(patrolTargetPos, patrolSpeed);
                break;
            case EnemyState.Chase:
                if (playerTransform != null)
                    MoveTowardsPlayer();
                break;
            case EnemyState.Hit:
                // 피격 시 약간의 넉백
                break;
            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    // ==================== 상태별 AI 처리 ====================
    private void HandleIdleState(bool playerDetected)
    {
        if (playerDetected)
        {
            currentState = EnemyState.Chase;
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            currentState = EnemyState.Patrol;
            SetNewPatrolTarget();
        }
    }

    private void HandlePatrolState(bool playerDetected)
    {
        if (playerDetected)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // 목표 지점에 도달했는지 확인
        if (Vector2.Distance(transform.position, patrolTargetPos) < 0.5f)
        {
            currentState = EnemyState.Idle;
            idleTimer = 0f;
        }
    }

    private void HandleChaseState(bool playerDetected)
    {
        if (!playerDetected)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // 공격 범위 내인지 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackRange && cooldownManager.IsReady("attack"))
        {
            AttackPlayer();
        }
    }

    // ==================== 움직임 ====================
    private void MoveTowardsPlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        UpdateFacingDirection(directionToPlayer);
        rb.linearVelocity = directionToPlayer * moveSpeed;
    }

    private void MoveTowardsTarget(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        UpdateFacingDirection(direction);
        rb.linearVelocity = direction * speed;
    }

    // ==================== 유틸리티 ====================
    private bool IsPlayerInRange(float range)
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= range;
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        patrolTargetPos = patrolStartPos + randomDirection * patrolRange;
    }

    // ==================== 공격 ====================
    private void AttackPlayer()
    {
        cooldownManager.StartCooldown("attack");

        Debug.Log($"적이 플레이어를 공격! 데미지: {attackDamage}");

        // 플레이어에게 데미지 주기
        if (playerTransform != null)
        {
            HealthComponent playerHealth = playerTransform.GetComponent<HealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        // 공격 범위 시각화
        DebugDrawCircle(transform.position, attackRange, Color.red, 0.3f);
    }

    // ==================== 검사용 함수 ====================
    public bool IsChasing => currentState == EnemyState.Chase;
    public bool IsDead => currentState == EnemyState.Dead;
    public bool IsStunned => isStunned;
    public EnemyState GetCurrentState() => currentState;
    public float GetDetectionRange() => detectionRange;
    public float GetAttackRange() => attackRange;

    // ==================== 디버그 시각화 ====================
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 순찰 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolStartPos, patrolRange);

        // 순찰 목표 지점
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolTargetPos, 0.5f);
            Gizmos.DrawLine(transform.position, patrolTargetPos);
        }
    }

    // ==================== HP 이벤트 핸들러 ====================

    /// <summary>
    /// 데미지를 받았을 때 처리
    /// </summary>
    private void HandleDamaged(int damage, int remainingHealth)
    {
        Debug.Log($"[{gameObject.name}] 피격! 데미지: {damage}, 남은 HP: {remainingHealth}");

        // 상태 변경
        currentState = EnemyState.Hit;
        isStunned = true;
        cooldownManager.StartCooldown("hitStun");

        // 넉백 적용
        if (playerTransform != null)
        {
            knockbackDirection = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            rb.linearVelocity = knockbackDirection * knockbackForce;
        }

        // 시각적 피드백
        StartCoroutine(HitFlashEffect());

        // TODO: 추가 피격 효과
        // - 피격 사운드
        // - 파티클 이펙트
    }

    /// <summary>
    /// 피격 시 색상 변경 효과
    /// </summary>
    private System.Collections.IEnumerator HitFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        // 빨간색으로 변경
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitFlashDuration);

        // 원래 색상으로 복구
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void HandleDeath()
    {
        if (currentState == EnemyState.Dead) return; // 중복 호출 방지

        currentState = EnemyState.Dead;

        Debug.Log($"[{gameObject.name}] 사망!");

        // AI 및 물리 즉시 중지
        rb.linearVelocity = Vector2.zero;

        // Collider 비활성화 (관통 방지)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 사망 시각 효과
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        // 즉시 비활성화 옵션
        if (disableOnDeath)
        {
            enabled = false; // MonoBehaviour만 비활성화
        }

        // TODO: 사망 효과 추가
        // - 사망 애니메이션
        // - 사망 사운드
        // - 파티클 이펙트
        // - 아이템 드롭

        // 일정 시간 후 파괴
        Destroy(gameObject, deathDelay);
    }
}
