using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{

    public class Enemy : EntityBase
    {
        private AiBTBase _aiBT;
        
        [Header("코인 드롭 설정")]
        [Tooltip("코인 드롭 확률 (0~1)")]
        [SerializeField, Range(0f, 1f)] private float coinDropChance = 1f;
        [Tooltip("드롭할 코인 개수 범위")]
        [SerializeField] private Vector2Int coinDropRange = new Vector2Int(1, 3);
        [Tooltip("코인 드롭 위치 오프셋 범위")]
        [SerializeField] private float coinDropRadius = 1f;

        private Transform _playerTransform;
        private HealthComponent _healthComponent;
        private MovementComponent _movementComponent;

        private Rigidbody2D _rb;
        private Vector2 _patrolStartPos;
        private Vector2 _patrolTargetPos;
        private bool _isPatrolling;
        private float _idleTimer;

        // 피격 관련
        private bool _isStunned;
        private Vector2 _knockbackDirection;

        private ISceneResourceProvider _resourceProvider;
        private ITableRepository _tableRepository;
        private EnemyConfigTableEntry _enemyConfigTableEntry;
        [Inject]
        private void Construct(ISceneResourceProvider resourceProvider, ITableRepository tableRepository)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
        }
        
        public void Initialize(EnemyConfigTableEntry enemyConfigTableEntry)
        {
            _enemyConfigTableEntry = enemyConfigTableEntry;
            Initialize(_tableRepository.GetTableEntry<EntityStatsTableEntry>(enemyConfigTableEntry.StatsId));
            
            _movementComponent = gameObject.AddOrGetComponent<MovementComponent>();
            _healthComponent = gameObject.AddOrGetComponent<HealthComponent>();
            _healthComponent.OnDeath += HandleDeath;
            _healthComponent.OnDamaged += HandleDamaged;
            
            gameObject.AddOrGetComponent<AIComponent>().SetBT(_resourceProvider.GetAssetSync<AiBTBase>(enemyConfigTableEntry.AIType));
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
            _isStunned = true;
        }

        private void HandleDeath()
        {
            return;
        }
        
        /// <summary>
        /// 사망 처리
        /// </summary>
        private async UniTask HandleDeathAsync()
        {
            // AI 및 물리 즉시 중지
            _rb.linearVelocity = Vector2.zero;

            // 코인 드롭
            await DropCoins();

            // TODO: 사망 효과 추가
            // - 사망 애니메이션
            // - 사망 사운드
            // - 파티클 이펙트

            // 일정 시간 후 파괴
            Destroy(gameObject, 1f);
        }
    }
}
