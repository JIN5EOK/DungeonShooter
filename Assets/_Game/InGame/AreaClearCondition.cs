using System.Collections.Generic;
using UnityEngine;
using DungeonShooter;
/// <summary>
/// 구역 클리어 조건을 체크하는 컴포넌트.
/// 조건이 만족되면 연결된 AreaGate에 알림을 보냅니다.
/// </summary>
public class AreaClearCondition : MonoBehaviour
{
    [Header("클리어 조건 설정")]
    [Tooltip("자동으로 영역 내 적을 찾을지 여부 (false면 수동으로 할당)")]
    [SerializeField] private bool autoDetectEnemies = true;
    
    [Tooltip("수동으로 할당할 적 목록 (autoDetectEnemies가 false일 때 사용)")]
    [SerializeField] private List<Enemy> assignedEnemies = new List<Enemy>();
    
    [Tooltip("적 감지 영역 (autoDetectEnemies가 true일 때 사용)")]
    [SerializeField] private Collider2D detectionArea;
    
    [Tooltip("클리어 조건: 모든 적 처치")]
    [SerializeField] private bool requireAllEnemiesKilled = true;
    
    [Header("연결된 게이트")]
    [Tooltip("클리어 시 열릴 AreaGate (비워두면 자동 검색)")]
    [SerializeField] private AreaGate targetGate;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    private List<Enemy> _trackedEnemies = new List<Enemy>();
    private Dictionary<HealthComponent, Enemy> _healthToEnemyMap = new Dictionary<HealthComponent, Enemy>();
    private Dictionary<HealthComponent, System.Action> _healthDeathHandlers = new Dictionary<HealthComponent, System.Action>();
    private int _initialEnemyCount = 0;
    private bool _isCleared = false;


    private void Start()
    {
        // AreaGate 자동 검색
        if (targetGate == null)
        {
            targetGate = GetComponent<AreaGate>();
            if (targetGate == null)
            {
                targetGate = GetComponentInParent<AreaGate>();
            }
        }

        if (targetGate == null && showDebugInfo)
        {
            Debug.LogWarning($"[{nameof(AreaClearCondition)}] {gameObject.name}: 연결된 AreaGate를 찾을 수 없습니다.");
        }

        InitializeEnemies();
    }

    /// <summary>
    /// 적 목록 초기화
    /// </summary>
    private void InitializeEnemies()
    {
        _trackedEnemies.Clear();

        if (autoDetectEnemies)
        {
            // 자동 감지 모드: detectionArea 또는 자신의 Collider 영역 내 적 찾기
            Collider2D searchArea = detectionArea;
            
            if (searchArea == null)
            {
                searchArea = GetComponent<Collider2D>();
            }
            
            if (searchArea != null)
            {
                List<Collider2D> colliders = new List<Collider2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.NoFilter();
                filter.useTriggers = true; // Trigger Collider도 감지하도록 설정
                
                // Overlap 사용 (Trigger Collider도 감지됨)
                int count = searchArea.Overlap(filter, colliders);

                if (showDebugInfo && count > 0)
                {
                    Debug.Log($"[{nameof(AreaClearCondition)}] {gameObject.name}: {count}개의 Collider 감지됨");
                }

                foreach (Collider2D col in colliders)
                {
                    if (col != null && col.CompareTag(GameTags.Enemy))
                    {
                        Enemy enemy = col.GetComponent<Enemy>();
                        if (enemy != null && !_trackedEnemies.Contains(enemy))
                        {
                            RegisterEnemy(enemy);
                        }
                    }
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[{nameof(AreaClearCondition)}] {gameObject.name}: detectionArea와 Collider가 없어 자동 감지가 불가능합니다.");
                }
            }
        }
        else
        {
            // 수동 할당 모드: assignedEnemies 사용
            foreach (Enemy enemy in assignedEnemies)
            {
                if (enemy != null && !_trackedEnemies.Contains(enemy))
                {
                    RegisterEnemy(enemy);
                }
            }
        }

        _initialEnemyCount = _trackedEnemies.Count;

        if (showDebugInfo)
        {
            Debug.Log($"[{nameof(AreaClearCondition)}] {gameObject.name} 초기화 완료. 추적 중인 적: {_trackedEnemies.Count}마리");
        }
    }

    /// <summary>
    /// 적을 등록하고 사망 이벤트 구독
    /// </summary>
    private void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null || _trackedEnemies.Contains(enemy)) return;

        _trackedEnemies.Add(enemy);
        
        HealthComponent health = enemy.GetComponent<HealthComponent>();
        if (health != null)
        {
            _healthToEnemyMap[health] = enemy;
            
            System.Action deathHandler = () => OnEnemyDeath(health);
            _healthDeathHandlers[health] = deathHandler;
            health.OnDeath += deathHandler;
        }
    }

    /// <summary>
    /// 적 등록 해제 및 이벤트 구독 해제
    /// </summary>
    private void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        _trackedEnemies.Remove(enemy);
        
        HealthComponent health = enemy.GetComponent<HealthComponent>();
        if (health != null && _healthToEnemyMap.ContainsKey(health))
        {
            if (_healthDeathHandlers.ContainsKey(health))
            {
                health.OnDeath -= _healthDeathHandlers[health];
                _healthDeathHandlers.Remove(health);
            }
            _healthToEnemyMap.Remove(health);
        }
    }

    /// <summary>
    /// 적이 죽었을 때 호출되는 콜백 (이벤트 기반)
    /// </summary>
    private void OnEnemyDeath(HealthComponent deadHealth)
    {
        if (deadHealth == null || !_healthToEnemyMap.ContainsKey(deadHealth))
            return;

        Enemy deadEnemy = _healthToEnemyMap[deadHealth];
        _trackedEnemies.Remove(deadEnemy);
        _healthToEnemyMap.Remove(deadHealth);
        
        if (_healthDeathHandlers.ContainsKey(deadHealth))
        {
            deadHealth.OnDeath -= _healthDeathHandlers[deadHealth];
            _healthDeathHandlers.Remove(deadHealth);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[{nameof(AreaClearCondition)}] {gameObject.name}: 적 사망 감지. 남은 적: {_trackedEnemies.Count}마리");
        }

        // 클리어 조건 체크
        CheckClearCondition();
    }

    /// <summary>
    /// 클리어 조건 체크
    /// </summary>
    private void CheckClearCondition()
    {
        if (_isCleared) return;

        bool isCleared = false;

        if (requireAllEnemiesKilled)
        {
            // 모든 적이 죽었는지 확인
            isCleared = _trackedEnemies.Count == 0;
        }
        else
        {
            // 적어도 하나의 적이 죽었는지 확인
            isCleared = _trackedEnemies.Count < _initialEnemyCount;
        }

        if (isCleared)
        {
            OnConditionCleared();
        }
    }

    /// <summary>
    /// 조건 클리어 처리
    /// </summary>
    private void OnConditionCleared()
    {
        if (_isCleared) return;

        _isCleared = true;

        if (showDebugInfo)
        {
            Debug.Log($"[{nameof(AreaClearCondition)}] {gameObject.name} 클리어 조건 만족!");
        }

        // AreaGate에 알림
        if (targetGate != null)
        {
            targetGate.Open();
        }
    }

    /// <summary>
    /// 수동으로 적 추가 (런타임에 사용 가능)
    /// </summary>
    public void AddEnemy(Enemy enemy)
    {
        if (_isCleared) return;
        
        RegisterEnemy(enemy);
        _initialEnemyCount = _trackedEnemies.Count;
    }

    /// <summary>
    /// 수동으로 적 제거
    /// </summary>
    public void RemoveEnemy(Enemy enemy)
    {
        UnregisterEnemy(enemy);
    }

    /// <summary>
    /// 현재 추적 중인 적 수 반환
    /// </summary>
    public int GetRemainingEnemyCount()
    {
        return _trackedEnemies.Count;
    }

    /// <summary>
    /// 클리어 상태 반환
    /// </summary>
    public bool IsCleared => _isCleared;

    private void OnDestroy()
    {
        // 모든 이벤트 구독 해제
        foreach (var kvp in _healthDeathHandlers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.OnDeath -= kvp.Value;
            }
        }
        _healthToEnemyMap.Clear();
        _healthDeathHandlers.Clear();
        _trackedEnemies.Clear();
    }

    // ==================== 디버그 시각화 ====================
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // 감지 영역 시각화
        if (autoDetectEnemies)
        {
            Collider2D searchArea = detectionArea != null ? detectionArea : GetComponent<Collider2D>();
            
            if (searchArea != null)
            {
                Gizmos.color = Color.yellow;
                Bounds bounds = searchArea.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        // 추적 중인 적 위치 표시
        if (Application.isPlaying && _trackedEnemies != null)
        {
            Gizmos.color = Color.red;
            foreach (Enemy enemy in _trackedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }
}

