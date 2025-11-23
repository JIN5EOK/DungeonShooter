using UnityEngine;

public class Enemy : BaseEntity
{
    [Header("AI 설정")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;

    private Transform playerTransform;
    private CooldownManager cooldownManager;
    private bool isChasing;

    protected override void Start()
    {
        base.Start();
        playerTransform = FindFirstObjectByType<PlayerProto>().transform;

        cooldownManager = new CooldownManager();
        cooldownManager.RegisterCooldown("attack", attackCooldown);
    }

    private void Update()
    {
        cooldownManager.UpdateCooldowns();
        DetectAndChasePlayer();
    }

    private void FixedUpdate()
    {
        if (isChasing && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ==================== 플레이어 감지 및 추격 ====================
    private void DetectAndChasePlayer()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;

            // 공격 범위 내인지 확인
            if (distanceToPlayer <= attackRange && cooldownManager.IsReady("attack"))
            {
                AttackPlayer();
            }
        }
        else
        {
            isChasing = false;
        }
    }

    // ==================== 플레이어 추격 ====================
    private void MoveTowardsPlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        UpdateFacingDirection(directionToPlayer);
        rb.linearVelocity = directionToPlayer * moveSpeed;
    }

    // ==================== 공격 ====================
    private void AttackPlayer()
    {
        cooldownManager.StartCooldown("attack");

        Debug.Log($"적이 플레이어를 공격! 데미지: {attackDamage}");

        // 플레이어에게 데미지 주기 (PlayerProto에 Health 컴포넌트가 있다면)
        PlayerProto player = playerTransform.GetComponent<PlayerProto>();
        if (player != null)
        {
            // Health 시스템이 있으면 데미지 주기
            // player.TakeDamage(attackDamage);
        }

        // 공격 범위 시각화
        DebugDrawCircle(transform.position, attackRange, Color.red, 0.3f);
    }

    // ==================== 검사용 함수 ====================
    public bool IsChasing => isChasing;
    public float GetDetectionRange() => detectionRange;
    public float GetAttackRange() => attackRange;
}
