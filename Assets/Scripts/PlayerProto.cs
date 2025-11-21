using UnityEngine;
public class PlayerProto : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Vector2 lastFacingDirection = Vector2.right;

    [Header("구르기(회피)")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.8f;
    private float dashCooldownTimer;
    private bool isDashing;
    private float dashTimer;

    [Header("스킬")]
    [SerializeField] private float skill1Cooldown = 2f;
    [SerializeField] private float skill2Cooldown = 3f;
    [SerializeField] private float skill3Cooldown = 5f;
    private float skill1CooldownTimer;
    private float skill2CooldownTimer;
    private float skill3CooldownTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        UpdateCooldowns();
        HandleInput();
        UpdateFacingDirection();
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
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0)
        {
            StartDash();
        }

        // 스킬 1 (Q)
        if (Input.GetKeyDown(KeyCode.J) && skill1CooldownTimer <= 0)
        {
            CastSkill1();
        }

        // 스킬 2 (E)
        if (Input.GetKeyDown(KeyCode.K) && skill2CooldownTimer <= 0)
        {
            CastSkill2();
        }

        // 스킬 3 (R)
        if (Input.GetKeyDown(KeyCode.L) && skill3CooldownTimer <= 0)
        {
            CastSkill3();
        }
    }

    private void UpdateFacingDirection()
    {
        if (moveInput.magnitude > 0)
        {
            lastFacingDirection = moveInput.normalized;
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
        dashCooldownTimer = dashCooldown;
        
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
        skill1CooldownTimer = skill1Cooldown;
        
        // 스킬 1: 전방 슬래시 공격 (근처 적 범위 공격)
        Debug.Log("스킬 1 - 전방 슬래시 공격");
        PerformRangeAttack(lastFacingDirection, range: 3f, radius: 1f);
    }

    private void CastSkill2()
    {
        skill2CooldownTimer = skill2Cooldown;
        
        // 스킬 2: 회전 공격 (주변 360도 공격)
        Debug.Log("스킬 2 - 회전 공격");
        PerformSpinAttack(radius: 2.5f);
    }

    private void CastSkill3()
    {
        skill3CooldownTimer = skill3Cooldown;
        
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

    // ==================== 쿨타임 관리 ====================
    private void UpdateCooldowns()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (skill1CooldownTimer > 0)
            skill1CooldownTimer -= Time.deltaTime;

        if (skill2CooldownTimer > 0)
            skill2CooldownTimer -= Time.deltaTime;

        if (skill3CooldownTimer > 0)
            skill3CooldownTimer -= Time.deltaTime;
    }

    // ==================== 디버그 유틸 ====================
    private void DebugDrawCircle(Vector2 center, float radius, Color color, float duration)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;

            Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(point1, point2, color, duration);
        }
    }

    // ==================== UI 표시용 (옵션) ====================
    public float GetDashCooldownPercent() => Mathf.Max(0, dashCooldownTimer / dashCooldown);
    public float GetSkill1CooldownPercent() => Mathf.Max(0, skill1CooldownTimer / skill1Cooldown);
    public float GetSkill2CooldownPercent() => Mathf.Max(0, skill2CooldownTimer / skill2Cooldown);
    public float GetSkill3CooldownPercent() => Mathf.Max(0, skill3CooldownTimer / skill3Cooldown);
}
