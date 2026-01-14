using UnityEngine;
using System.Collections;
using DungeonShooter;
using VContainer;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Hit,
    Stunned,
    Dead
}

public class Enemy : EntityBase
{
    [Header("스탯 컴포넌트")]
    [SerializeField] private EntityStatsComponent statsComponent;

    [Header("AI 설정")]
    [SerializeField] private float detectionRange = 10f;
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

    [Header("공격 범위 시각화")]
    [Tooltip("공격 범위를 시각적으로 표시할지 여부")]
    [SerializeField] private bool showAttackRange = true;
    [SerializeField] private AttackRangeVisualizer attackRangeVisualizer;

    [Header("코인 드롭 설정")]
    
    [Tooltip("코인 드롭 확률 (0~1)")]
    [SerializeField, Range(0f, 1f)] private float coinDropChance = 1f;
    [Tooltip("드롭할 코인 개수 범위")]
    [SerializeField] private Vector2Int coinDropRange = new Vector2Int(1, 3);
    [Tooltip("코인 드롭 위치 오프셋 범위")]
    [SerializeField] private float coinDropRadius = 1f;

    private Transform _playerTransform;
    private CooldownComponent _cooldownComponent;
    private HealthComponent _healthComponent;
    private MovementComponent _movementComponent;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    // AI 상태
    private EnemyState _currentState = EnemyState.Idle;
    private Vector2 _patrolStartPos;
    private Vector2 _patrolTargetPos;
    private bool _isPatrolling;
    private float _idleTimer;

    // 피격 관련
    private bool _isStunned;
    private Vector2 _knockbackDirection;

    private EntityFactory _entityFactory;

    [Inject]
    private void Construct(EntityFactory entityFactory)
    {
        _entityFactory = entityFactory;
    }
    
    protected override void Start()
    {
        base.Start();
        statsComponent = statsComponent ?? GetComponent<EntityStatsComponent>();
        ApplyStatsFromComponent();
        _playerTransform = FindFirstObjectByType<Player>().transform;

        // 쿨다운 컴포넌트 초기화
        _cooldownComponent = GetComponent<CooldownComponent>();
        _cooldownComponent = _cooldownComponent ?? gameObject.AddComponent<CooldownComponent>();
        _cooldownComponent.RegisterCooldown("attack", GetAttackCooldown());
        _cooldownComponent.RegisterCooldown("hitStun", hitStunDuration);

        // 이동 컴포넌트 초기화
        _movementComponent = GetComponent<MovementComponent>();
        _movementComponent = _movementComponent ?? gameObject.AddComponent<MovementComponent>();
        _movementComponent.MoveSpeed = moveSpeed;

        // SpriteRenderer 초기화
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

        // 순찰 초기화
        _patrolStartPos = transform.position;
        SetNewPatrolTarget();

        // HealthComponent 찾기 및 이벤트 구독
        _healthComponent = GetComponent<HealthComponent>();
        if (_healthComponent != null)
        {
            _healthComponent.OnDeath += HandleDeath;
            _healthComponent.OnDamaged += HandleDamaged;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] HealthComponent가 없습니다. 적이 죽지 않습니다!");
        }

        // 공격 범위 시각화 초기화
        if (showAttackRange)
        {
            if (attackRangeVisualizer == null)
            {
                attackRangeVisualizer = GetComponent<AttackRangeVisualizer>();
                if (attackRangeVisualizer == null)
                {
                    // 자동으로 생성 (LineRenderer 먼저 추가 필요)
                    var visualizerObj = new GameObject("AttackRangeVisualizer");
                    visualizerObj.transform.SetParent(transform);
                    visualizerObj.transform.localPosition = Vector3.zero;
                    visualizerObj.AddComponent<LineRenderer>(); // LineRenderer 먼저 추가
                    attackRangeVisualizer = visualizerObj.AddComponent<AttackRangeVisualizer>();
                }
            }
            
            if (attackRangeVisualizer != null)
            {
                attackRangeVisualizer.SetRadius(GetAttackRange());
                attackRangeVisualizer.SetColor(new Color(1f, 0f, 0f, 0.3f)); // 반투명 빨간색
            }
        }
    }

    private void ApplyStatsFromComponent()
    {
        if (statsComponent == null) return;

        moveSpeed = statsComponent.MoveSpeed;

        // MovementComponent와 동기화 (있을 경우)
        if (_movementComponent != null)
        {
            _movementComponent.MoveSpeed = moveSpeed;
        }

        // HealthComponent와 동기화 (있을 경우)
        var health = GetComponent<HealthComponent>();
        if (health != null)
        {
            health.SetMaxHealth(statsComponent.MaxHealth, true);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_healthComponent != null)
        {
            _healthComponent.OnDeath -= HandleDeath;
            _healthComponent.OnDamaged -= HandleDamaged;
        }
    }

    private void Update()
    {
        // 죽었으면 AI 중지
        if (_currentState == EnemyState.Dead) return;

        UpdateAI();
    }

    private void UpdateAI()
    {
        // 스턴 상태 확인
        if (_isStunned && _cooldownComponent.IsReady("hitStun"))
        {
            _isStunned = false;
            _currentState = EnemyState.Idle;
        }

        // 스턴 중이면 AI 중지
        if (_isStunned) return;

        // 플레이어 감지
        var playerDetected = IsPlayerInRange(detectionRange);

        switch (_currentState)
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
                if (_cooldownComponent.IsReady("hitStun"))
                {
                    _currentState = playerDetected ? EnemyState.Chase : EnemyState.Idle;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        // 죽었으면 움직임 중지
        if (_currentState == EnemyState.Dead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 넉백 처리
        if (_isStunned)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
            return;
        }

        // 상태별 움직임
        switch (_currentState)
        {
            case EnemyState.Idle:
                _movementComponent.Direction = Vector2.zero;
                _movementComponent.Move();
                break;
            case EnemyState.Patrol:
                MoveTowardsTarget(_patrolTargetPos, patrolSpeed);
                break;
            case EnemyState.Chase:
                if (_playerTransform != null)
                    MoveTowardsPlayer();
                break;
            case EnemyState.Hit:
                // 피격 시 약간의 넉백
                break;
            default:
                _movementComponent.Direction = Vector2.zero;
                _movementComponent.Move();
                break;
        }
    }

    // ==================== 상태별 AI 처리 ====================
    private void HandleIdleState(bool playerDetected)
    {
        if (playerDetected)
        {
            _currentState = EnemyState.Chase;
            return;
        }

        _idleTimer += Time.deltaTime;
        if (_idleTimer >= idleTime)
        {
            _idleTimer = 0f;
            _currentState = EnemyState.Patrol;
            SetNewPatrolTarget();
        }
    }

    private void HandlePatrolState(bool playerDetected)
    {
        if (playerDetected)
        {
            _currentState = EnemyState.Chase;
            return;
        }

        // 목표 지점에 도달했는지 확인
        if (Vector2.Distance(transform.position, _patrolTargetPos) < 0.5f)
        {
            _currentState = EnemyState.Idle;
            _idleTimer = 0f;
        }
    }

    private void HandleChaseState(bool playerDetected)
    {
        if (!playerDetected)
        {
            _currentState = EnemyState.Idle;
            return;
        }

        // 공격 범위 내인지 확인
        var distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        var range = GetAttackRange();
        if (distanceToPlayer <= range && _cooldownComponent.IsReady("attack"))
        {
            AttackPlayer();
        }
    }

    // ==================== 움직임 ====================
    private void MoveTowardsPlayer()
    {
        var directionToPlayer = (_playerTransform.position - transform.position).normalized;
        _movementComponent.MoveSpeed = moveSpeed;
        _movementComponent.Direction = directionToPlayer;
        _movementComponent.Move();
    }

    private void MoveTowardsTarget(Vector2 target, float speed)
    {
        var direction = (target - (Vector2)transform.position).normalized; ;
        _movementComponent.MoveSpeed = speed;
        _movementComponent.Direction = direction;
        _movementComponent.Move();
    }

    // ==================== 유틸리티 ====================
    private bool IsPlayerInRange(float range)
    {
        if (_playerTransform == null) return false;
        return Vector2.Distance(transform.position, _playerTransform.position) <= range;
    }

    private void SetNewPatrolTarget()
    {
        var randomDirection = Random.insideUnitCircle.normalized;
        _patrolTargetPos = _patrolStartPos + randomDirection * patrolRange;
    }

    // ==================== 공격 ====================
    private void AttackPlayer()
    {
        _cooldownComponent.StartCooldown("attack");

        var damage = GetAttackDamage();
        Debug.Log($"적이 플레이어를 공격! 데미지: {damage}");

        // 플레이어에게 데미지 주기
        if (_playerTransform != null)
        {
            var playerHealth = _playerTransform.GetComponent<HealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // 공격 범위 시각화
        if (showAttackRange && attackRangeVisualizer != null)
        {
            attackRangeVisualizer.Show();
        }
        else
        {
            DebugDrawCircle(transform.position, GetAttackRange(), Color.red, 0.3f);
        }
    }

    // ==================== 검사용 함수 ====================
    public bool IsChasing => _currentState == EnemyState.Chase;
    public bool IsDead => _currentState == EnemyState.Dead;
    public bool IsStunned => _isStunned;
    public EnemyState GetCurrentState() => _currentState;
    public float GetDetectionRange() => detectionRange;
    public float GetAttackRange() => statsComponent != null ? statsComponent.AttackRange : 0f;
    private int GetAttackDamage() => statsComponent != null ? statsComponent.AttackDamage : 0;
    private float GetAttackCooldown() => statsComponent != null ? statsComponent.AttackCooldown : 0f;

    // ==================== 디버그 시각화 ====================
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetAttackRange());

        // 순찰 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_patrolStartPos, patrolRange);

        // 순찰 목표 지점
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_patrolTargetPos, 0.5f);
            Gizmos.DrawLine(transform.position, _patrolTargetPos);
        }
    }

    // ==================== 코인 드롭 ====================

    /// <summary>
    /// 사망 시 코인을 드롭한다.
    /// </summary>
    private async Awaitable DropCoins()
    {
        var coinPickup = await _entityFactory.Create(EntityKey.Coin);
        var coinPickupPrefab = coinPickup.GetComponent<CoinPickup>();
        if (coinPickupPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 코인 프리팹이 설정되지 않아 코인을 드롭하지 않습니다.");
            return;
        }

        // 드롭 확률 체크
        if (Random.value > coinDropChance)
            return;

        // 드롭할 코인 개수 결정
        var coinCount = Random.Range(coinDropRange.x, coinDropRange.y + 1);

        // 코인 생성
        for (int i = 0; i < coinCount; i++)
        {
            // 랜덤 위치 계산 (적 위치 주변)
            var randomOffset = Random.insideUnitCircle * coinDropRadius;
            var dropPosition = transform.position + (Vector3)randomOffset;

            // 코인 인스턴스 생성
            var coin = Instantiate(coinPickupPrefab, dropPosition, Quaternion.identity);
            Debug.Log($"[{gameObject.name}] 코인 {coinCount}개 드롭!");
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
        _currentState = EnemyState.Hit;
        _isStunned = true;
        _cooldownComponent.StartCooldown("hitStun");

        // 넉백 적용
        if (_playerTransform != null)
        {
            _knockbackDirection = ((Vector2)transform.position - (Vector2)_playerTransform.position).normalized;
            rb.linearVelocity = _knockbackDirection * knockbackForce;
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
        if (_spriteRenderer == null) yield break;

        // 빨간색으로 변경
        _spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitFlashDuration);

        // 원래 색상으로 복구
        _spriteRenderer.color = _originalColor;
    }

    private void HandleDeath()
    {
        _ = HandleDeathAsync();
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    private async Awaitable HandleDeathAsync()
    {
        if (_currentState == EnemyState.Dead) return; // 중복 호출 방지

        _currentState = EnemyState.Dead;

        Debug.Log($"[{gameObject.name}] 사망!");

        // AI 및 물리 즉시 중지
        rb.linearVelocity = Vector2.zero;

        // Collider 비활성화 (관통 방지)
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 사망 시각 효과
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.gray;
        }

        // 즉시 비활성화 옵션
        if (disableOnDeath)
        {
            enabled = false; // MonoBehaviour만 비활성화
        }

        // 코인 드롭
        await DropCoins();

        // TODO: 사망 효과 추가
        // - 사망 애니메이션
        // - 사망 사운드
        // - 파티클 이펙트

        // 일정 시간 후 파괴
        Destroy(gameObject, deathDelay);
    }
}
