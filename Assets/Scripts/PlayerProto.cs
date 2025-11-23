using UnityEngine;
using System.Threading;

public class PlayerProto : BaseEntity
{
    private Vector2 moveInput;

    [Header("구르기(회피)")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.8f;
    private bool isDashing;
    private float dashTimer;

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

    private CooldownManager cooldownManager;
    private bool isJumping;
    private CancellationTokenSource jumpCancellationTokenSource;

    protected override void Start()
    {
        base.Start();

        cooldownManager = new CooldownManager();
        cooldownManager.RegisterCooldown("dash", dashCooldown);
        cooldownManager.RegisterCooldown("skill1", skill1Cooldown);
        cooldownManager.RegisterCooldown("skill2", skill2Cooldown);
        cooldownManager.RegisterCooldown("skill3", skill3Cooldown);
    }

    private void OnDestroy()
    {
        // 취소 토큰 정리
        jumpCancellationTokenSource?.Cancel();
        jumpCancellationTokenSource?.Dispose();
    }

    private void Update()
    {
        cooldownManager.UpdateCooldowns();
        
        if (!isJumping)
        {
            HandleInput();
            UpdateFacingDirection(moveInput);
        }
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            return;
        }
        else if (isDashing)
        {
            UpdateDash();
        }
        else
        {
            MovePlayer();
        }
    }

    // ==================== 입력 처리 ====================
    private void HandleInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && cooldownManager.IsReady("dash"))
        {
            StartDash();
        }

        if (Input.GetKeyDown(KeyCode.J) && cooldownManager.IsReady("skill1"))
        {
            CastSkill1();
        }

        if (Input.GetKeyDown(KeyCode.K) && cooldownManager.IsReady("skill2"))
        {
            CastSkill2();
        }

        if (Input.GetKeyDown(KeyCode.L) && cooldownManager.IsReady("skill3"))
        {
            CastSkill3Async();
        }
    }

    // ==================== 이동 ====================
    private void MovePlayer()
    {
        Vector2 velocity = moveInput.normalized * moveSpeed;
        rb.linearVelocity = velocity;
    }

    // ==================== 구르기 (회피) ====================
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownManager.StartCooldown("dash");
        
        Debug.Log("구르기!");
    }

    private void UpdateDash()
    {
        dashTimer -= Time.fixedDeltaTime;
        
        Vector2 dashDirection = moveInput.magnitude > 0 ? moveInput.normalized : lastFacingDirection;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (dashTimer <= 0)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ==================== 스킬 ====================
    private void CastSkill1()
    {
        cooldownManager.StartCooldown("skill1");

        Debug.Log("스킬 1 - 전방 슬래시 공격");
        PerformRangeAttack(lastFacingDirection, range: 3f, radius: 1f, damage: skill1Damage);
    }

    private void CastSkill2()
    {
        cooldownManager.StartCooldown("skill2");

        Debug.Log("스킬 2 - 회전 공격");
        PerformSpinAttack(radius: 2.5f, damage: skill2Damage);
    }

    private async void CastSkill3Async()
    {
        cooldownManager.StartCooldown("skill3");
        
        Debug.Log("스킬 3 - 점프 공격");
        
        // 기존 점프 취소
        jumpCancellationTokenSource?.Cancel();
        jumpCancellationTokenSource?.Dispose();
        jumpCancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            await PerformJumpAttackAsync(
                targetDistance: 4f, 
                radius: 2f, 
                damage: skill3Damage,
                jumpCancellationTokenSource.Token
            );
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[스킬3] 점프 취소됨");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[스킬3] 오류: {ex.Message}");
        }
    }

    // ==================== 스킬 효과 함수들 ====================
    
    private void PerformRangeAttack(Vector2 direction, float range, float radius, int damage)
    {
        Vector2 attackPos = (Vector2)transform.position + direction * (range / 2);
        
        Debug.DrawLine(transform.position, attackPos + direction * range, Color.red, 0.5f);
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, radius);
        int hitCount = 0;
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                HealthComponent enemyHealth = hit.GetComponent<HealthComponent>();
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        int hitCount = 0;
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                HealthComponent enemyHealth = hit.GetComponent<HealthComponent>();
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
    private async Awaitable PerformJumpAttackAsync(float targetDistance, float radius, int damage, CancellationToken cancellationToken)
    {
        isJumping = true;
        
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + lastFacingDirection * targetDistance;
        
        Debug.Log($"[스킬3] 점프 시작! {startPos} → {targetPos}");
        
        float elapsedTime = 0f;
        
        try
        {
            // 점프 중 이동
            while (elapsedTime < jumpDuration)
            {
                // 취소 확인
                cancellationToken.ThrowIfCancellationRequested();
                
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / jumpDuration;
                
                // 포물선 움직임
                Vector2 currentPos = Vector2.Lerp(startPos, targetPos, progress);
                float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                
                transform.position = new Vector3(currentPos.x, currentPos.y + height, 0);
                rb.linearVelocity = Vector2.zero;
                
                // 다음 프레임까지 대기
                await Awaitable.NextFrameAsync(cancellationToken);
            }
            
            // 최종 위치로 이동
            transform.position = targetPos;
            
            // 착지 시 데미지 적용
            Debug.Log($"[스킬3] 착지!");
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius);
            int hitCount = 0;
            
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    HealthComponent enemyHealth = hit.GetComponent<HealthComponent>();
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
            isJumping = false;
        }
    }

    // ==================== UI 표시용 (옵션) ====================
    public float GetDashCooldownPercent() => cooldownManager.GetCooldownPercent("dash");
    public float GetSkill1CooldownPercent() => cooldownManager.GetCooldownPercent("skill1");
    public float GetSkill2CooldownPercent() => cooldownManager.GetCooldownPercent("skill2");
    public float GetSkill3CooldownPercent() => cooldownManager.GetCooldownPercent("skill3");
}
