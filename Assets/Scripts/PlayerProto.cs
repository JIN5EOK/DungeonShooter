using UnityEngine;
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

    private CooldownManager cooldownManager;

    protected override void Start()
    {
        base.Start();

        // 쿨다운 매니저 초기화
        cooldownManager = new CooldownManager();
        cooldownManager.RegisterCooldown("dash", dashCooldown);
        cooldownManager.RegisterCooldown("skill1", skill1Cooldown);
        cooldownManager.RegisterCooldown("skill2", skill2Cooldown);
        cooldownManager.RegisterCooldown("skill3", skill3Cooldown);
    }

    private void Update()
    {
        cooldownManager.UpdateCooldowns();
        HandleInput();
        UpdateFacingDirection(moveInput);
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            MovePlayer();
        }
        else
        {
            UpdateDash();
        }
    }

    // ==================== 입력 처리 ====================
    private void HandleInput()
    {
        // WASD 이동 입력
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 구르기 (Space)
        if (Input.GetKeyDown(KeyCode.Space) && cooldownManager.IsReady("dash"))
        {
            StartDash();
        }

        // 스킬 1 (Q)
        if (Input.GetKeyDown(KeyCode.J) && cooldownManager.IsReady("skill1"))
        {
            CastSkill1();
        }

        // 스킬 2 (E)
        if (Input.GetKeyDown(KeyCode.K) && cooldownManager.IsReady("skill2"))
        {
            CastSkill2();
        }

        // 스킬 3 (R)
        if (Input.GetKeyDown(KeyCode.L) && cooldownManager.IsReady("skill3"))
        {
            CastSkill3();
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

        // 스킬 1: 전방 슬래시 공격 (근처 적 범위 공격)
        Debug.Log("스킬 1 - 전방 슬래시 공격");
        PerformRangeAttack(lastFacingDirection, range: 3f, radius: 1f);
    }

    private void CastSkill2()
    {
        cooldownManager.StartCooldown("skill2");

        // 스킬 2: 회전 공격 (주변 360도 공격)
        Debug.Log("스킬 2 - 회전 공격");
        PerformSpinAttack(radius: 2.5f);
    }

    private void CastSkill3()
    {
        cooldownManager.StartCooldown("skill3");
        
        // 스킬 3: 점프 공격 (지정 위치 범위 공격)
        Debug.Log("스킬 3 - 점프 공격");
        PerformGroundSlam(targetDistance: 4f, radius: 2f);
    }

    // 스킬 효과 함수들
    private void PerformRangeAttack(Vector2 direction, float range, float radius)
    {
        Vector2 attackPos = (Vector2)transform.position + direction * (range / 2);
        
        // 디버그: 공격 범위 표시
        Debug.DrawLine(transform.position, attackPos + direction * range, Color.red, 0.5f);
        
        // 실제 게임에서는 여기서 적에게 데미지를 주는 로직 추가
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"적 피격: {hit.name}");
            }
        }
    }

    private void PerformSpinAttack(float radius)
    {
        // 주변 모든 적에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"적 피격: {hit.name}");
            }
        }
        
        // 디버그: 공격 범위 표시
        DebugDrawCircle(transform.position, radius, Color.yellow, 0.5f);
    }

    private void PerformGroundSlam(float targetDistance, float radius)
    {
        Vector2 slamPos = (Vector2)transform.position + lastFacingDirection * targetDistance;
        
        // 착지 지점에 모든 적에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(slamPos, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"적 피격: {hit.name}");
            }
        }
        
        // 디버그: 공격 범위 표시
        DebugDrawCircle(slamPos, radius, Color.magenta, 0.5f);
    }

    // ==================== UI 표시용 (옵션) ====================
    public float GetDashCooldownPercent() => cooldownManager.GetCooldownPercent("dash");
    public float GetSkill1CooldownPercent() => cooldownManager.GetCooldownPercent("skill1");
    public float GetSkill2CooldownPercent() => cooldownManager.GetCooldownPercent("skill2");
    public float GetSkill3CooldownPercent() => cooldownManager.GetCooldownPercent("skill3");
}
