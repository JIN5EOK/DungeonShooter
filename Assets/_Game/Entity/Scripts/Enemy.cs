using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
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
        [Header("AI 설정")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float patrolRange = 5f;
        [SerializeField] private float idleTime = 2f;

        [Header("피격 설정")]
        [SerializeField] private float hitStunDuration = 0.5f;
        [SerializeField] private float knockbackForce = 3f;

        [Header("사망 설정")]
        [SerializeField] private float deathDelay = 1f;
        [SerializeField] private bool disableOnDeath = true;

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

        private Rigidbody2D _rb;
        // AI 상태
        private EnemyState _currentState = EnemyState.Idle;
        private Vector2 _patrolStartPos;
        private Vector2 _patrolTargetPos;
        private bool _isPatrolling;
        private float _idleTimer;

        // 피격 관련
        private bool _isStunned;
        private Vector2 _knockbackDirection;

        private ISceneResourceProvider _resourceProvider;

        [Inject]
        private void Construct(ISceneResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;

            return;
            _rb = gameObject.AddOrGetComponent<Rigidbody2D>();
            statsComponent = gameObject.AddOrGetComponent<EntityStatsComponent>();
            _playerTransform = FindFirstObjectByType<Player>().transform;
            
            _cooldownComponent = gameObject.AddOrGetComponent<CooldownComponent>();
            //_cooldownComponent.RegisterCooldown("attack", GetAttackCooldown());
            _cooldownComponent.RegisterCooldown("hitStun", hitStunDuration);
            
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _movementComponent.MoveSpeed = statsComponent.MoveSpeed;

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
                LogHandler.LogWarning<Enemy>($"{gameObject.name}: HealthComponent가 없습니다. 적이 죽지 않습니다!");
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
            return;
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
            return;
            // 죽었으면 움직임 중지
            if (_currentState == EnemyState.Dead)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            // 넉백 처리
            if (_isStunned)
            {
                _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
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
                    MoveTowardsTarget(_patrolTargetPos);
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
            var range = 0;
            // var range = GetAttackRange();
            if (distanceToPlayer <= range && _cooldownComponent.IsReady("attack"))
            {
                AttackPlayer();
            }
        }

        // ==================== 움직임 ====================
        private void MoveTowardsPlayer()
        {
            var directionToPlayer = (_playerTransform.position - transform.position).normalized;
            _movementComponent.Direction = directionToPlayer;
            _movementComponent.Move();
        }

        private void MoveTowardsTarget(Vector2 target)
        {
            var direction = (target - (Vector2)transform.position).normalized; ;
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
            
            // var damage = GetAttackDamage();
            var damage = 0;
            LogHandler.Log<Enemy>($"적이 플레이어를 공격! 데미지: {damage}");

            // 플레이어에게 데미지 주기
            if (_playerTransform != null)
            {
                var playerHealth = _playerTransform.GetComponent<HealthComponent>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        // ==================== 코인 드롭 ====================

        /// <summary>
        /// 사망 시 코인을 드롭한다.
        /// </summary>
        private async Awaitable DropCoins()
        {
            var coinPickup = await _resourceProvider.GetInstanceAsync(EntityKey.Coin);
            var coinPickupPrefab = coinPickup.GetComponent<CoinPickup>();
            if (coinPickupPrefab == null)
            {
                LogHandler.LogWarning<Enemy>($"{gameObject.name}: 코인 프리팹이 설정되지 않아 코인을 드롭하지 않습니다.");
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
                LogHandler.Log<Enemy>($"{gameObject.name}: 코인 {coinCount}개 드롭!");
            }
        }

        // ==================== HP 이벤트 핸들러 ====================

        /// <summary>
        /// 데미지를 받았을 때 처리
        /// </summary>
        private void HandleDamaged(int damage, int remainingHealth)
        {
            LogHandler.Log<Enemy>($"{gameObject.name}: 피격! 데미지: {damage}, 남은 HP: {remainingHealth}");

            // 상태 변경
            _currentState = EnemyState.Hit;
            _isStunned = true;
            _cooldownComponent.StartCooldown("hitStun");

            // 넉백 적용
            if (_playerTransform != null)
            {
                _knockbackDirection = ((Vector2)transform.position - (Vector2)_playerTransform.position).normalized;
                _rb.linearVelocity = _knockbackDirection * knockbackForce;
            }
        }

        private void HandleDeath()
        {
            HandleDeathAsync().Forget();
        }
        
        /// <summary>
        /// 사망 처리
        /// </summary>
        private async UniTask HandleDeathAsync()
        {
            if (_currentState == EnemyState.Dead) return; // 중복 호출 방지

            _currentState = EnemyState.Dead;

            LogHandler.Log<Enemy>($"{gameObject.name}: 사망!");

            // AI 및 물리 즉시 중지
            _rb.linearVelocity = Vector2.zero;

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
}
