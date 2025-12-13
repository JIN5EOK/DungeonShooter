using System.Collections.Generic;
using UnityEngine;
using DungeonShooter;
public class PlayerProto : BaseEntity
{
    private Vector2 _moveInput;

    [Header("구르기(회피)")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.8f;
    private bool _isDashing;
    private float _dashTimer;

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

    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.2f;

    [Header("스킬 범위 시각화")]
    [Tooltip("스킬 범위를 시각적으로 표시할지 여부")]
    [SerializeField] private bool showSkillRanges = true;
    [SerializeField] private AttackRangeVisualizer skill1Visualizer; // 전방 슬래시
    [SerializeField] private AttackRangeVisualizer skill2Visualizer; // 회전 공격
    [SerializeField] private AttackRangeVisualizer skill3Visualizer; // 점프 공격

    [Header("상호작용 설정")]
    [Tooltip("상호작용 키 (기본값: E)")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private CooldownManager _cooldownManager;
    private HealthComponent _healthComponent;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private bool _isJumping;
    private bool _isDead;
    private System.Threading.CancellationTokenSource _jumpCancellationTokenSource;
    private HashSet<IInteractable> _nearbyInteractables = new HashSet<IInteractable>();

    protected override void Start()
    {
        base.Start();

        _cooldownManager = new CooldownManager();
        _cooldownManager.RegisterCooldown("dash", dashCooldown);
        _cooldownManager.RegisterCooldown("skill1", skill1Cooldown);
        _cooldownManager.RegisterCooldown("skill2", skill2Cooldown);
        _cooldownManager.RegisterCooldown("skill3", skill3Cooldown);

        // 체력 컴포넌트 초기화
        _healthComponent = GetComponent<HealthComponent>();
        if (_healthComponent == null)
        {
            _healthComponent = gameObject.AddComponent<HealthComponent>();
        }

        // maxHealth 설정
        _healthComponent.SetMaxHealth(maxHealth, true); // healToFull = true로 체력 가득 채움

        // 체력 이벤트 구독
        _healthComponent.OnDamaged += HandleDamaged;
        _healthComponent.OnDeath += HandleDeath;

        // SpriteRenderer 초기화
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

        Debug.Log($"[PlayerProto] 체력 시스템 초기화 완료: {_healthComponent.CurrentHealth}/{_healthComponent.MaxHealth}");

        // 스킬 범위 시각화 초기화
        if (showSkillRanges)
        {
            InitializeSkillVisualizers();
        }
    }

    /// <summary>
    /// 스킬 범위 시각화 초기화
    /// </summary>
    private void InitializeSkillVisualizers()
    {
        // 스킬1 (전방 슬래시) 시각화 - 독립 GameObject (공격 위치에 표시)
        if (skill1Visualizer == null)
        {
            GameObject skill1Obj = new GameObject("Skill1RangeVisualizer");
            skill1Obj.transform.SetParent(null); // 부모 없이 독립적으로
            skill1Obj.AddComponent<LineRenderer>();
            skill1Visualizer = skill1Obj.AddComponent<AttackRangeVisualizer>();
        }
        if (skill1Visualizer != null)
        {
            skill1Visualizer.SetRadius(1f); // 스킬1 반경
            skill1Visualizer.SetColor(new Color(1f, 0.5f, 0f, 0.3f)); // 반투명 주황색
            skill1Visualizer.gameObject.SetActive(false); // 기본적으로 비활성화
        }

        // 스킬2 (회전 공격) 시각화 - 플레이어 중심
        if (skill2Visualizer == null)
        {
            GameObject skill2Obj = new GameObject("Skill2RangeVisualizer");
            skill2Obj.transform.SetParent(transform);
            skill2Obj.transform.localPosition = Vector3.zero;
            skill2Obj.AddComponent<LineRenderer>();
            skill2Visualizer = skill2Obj.AddComponent<AttackRangeVisualizer>();
        }
        if (skill2Visualizer != null)
        {
            skill2Visualizer.SetRadius(2.5f); // 스킬2 반경
            skill2Visualizer.SetColor(new Color(1f, 1f, 0f, 0.3f)); // 반투명 노란색
        }

        // 스킬3 (점프 공격) 시각화 - 독립 GameObject (착지 위치에 고정)
        if (skill3Visualizer == null)
        {
            GameObject skill3Obj = new GameObject("Skill3RangeVisualizer");
            skill3Obj.transform.SetParent(null); // 부모 없이 독립적으로
            skill3Obj.AddComponent<LineRenderer>();
            skill3Visualizer = skill3Obj.AddComponent<AttackRangeVisualizer>();
        }
        if (skill3Visualizer != null)
        {
            skill3Visualizer.SetRadius(2f); // 스킬3 반경
            skill3Visualizer.SetColor(new Color(1f, 0f, 1f, 0.3f)); // 반투명 마젠타
            skill3Visualizer.gameObject.SetActive(false); // 기본적으로 비활성화
        }
    }

    private void OnDestroy()
    {
        // 취소 토큰 정리
        _jumpCancellationTokenSource?.Cancel();
        _jumpCancellationTokenSource?.Dispose();

        // 이벤트 구독 해제
        if (_healthComponent != null)
        {
            _healthComponent.OnDamaged -= HandleDamaged;
            _healthComponent.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        // 죽었으면 입력 처리 중지
        if (_isDead) return;

        _cooldownManager.UpdateCooldowns();
        
        if (!_isJumping)
        {
            HandleInput();
            UpdateFacingDirection(_moveInput);
        }
    }

    private void FixedUpdate()
    {
        // 죽었으면 물리 처리 중지
        if (_isDead) return;

        if (_isJumping)
        {
            return;
        }
        else if (_isDashing)
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
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && _cooldownManager.IsReady("dash"))
        {
            StartDash();
        }

        if (Input.GetKeyDown(KeyCode.J) && _cooldownManager.IsReady("skill1"))
        {
            CastSkill1();
        }

        if (Input.GetKeyDown(KeyCode.K) && _cooldownManager.IsReady("skill2"))
        {
            CastSkill2();
        }

        if (Input.GetKeyDown(KeyCode.L) && _cooldownManager.IsReady("skill3"))
        {
            CastSkill3Async();
        }

        // 상호작용 키 입력 처리
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    // ==================== 상호작용 ====================
    /// <summary>
    /// 상호작용 가능한 오브젝트를 등록합니다.
    /// </summary>
    public void RegisterInteractable(IInteractable interactable)
    {
        if (interactable != null)
        {
            _nearbyInteractables.Add(interactable);
        }
    }

    /// <summary>
    /// 상호작용 가능한 오브젝트를 제거합니다.
    /// </summary>
    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactable != null)
        {
            _nearbyInteractables.Remove(interactable);
        }
    }

    /// <summary>
    /// 상호작용을 시도합니다.
    /// </summary>
    private void TryInteract()
    {
        // 가장 가까운 상호작용 가능한 오브젝트 찾기
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (IInteractable interactable in _nearbyInteractables)
        {
            if (interactable != null && interactable.CanInteract)
            {
                // MonoBehaviour인 경우 거리 계산
                if (interactable is MonoBehaviour mb)
                {
                    float distance = Vector2.Distance(transform.position, mb.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
                else
                {
                    // MonoBehaviour가 아닌 경우 첫 번째로 발견된 것 사용
                    closestInteractable = interactable;
                    break;
                }
            }
        }

        // 상호작용 수행
        if (closestInteractable != null)
        {
            closestInteractable.Interact();
        }
    }

    // ==================== 이동 ====================
    private void MovePlayer()
    {
        Vector2 velocity = _moveInput.normalized * moveSpeed;
        rb.linearVelocity = velocity;
    }

    // ==================== 구르기 (회피) ====================
    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _cooldownManager.StartCooldown("dash");
        
        Debug.Log("구르기!");
    }

    private void UpdateDash()
    {
        _dashTimer -= Time.fixedDeltaTime;
        
        Vector2 dashDirection = _moveInput.magnitude > 0 ? _moveInput.normalized : lastFacingDirection;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (_dashTimer <= 0)
        {
            _isDashing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ==================== 스킬 ====================
    private void CastSkill1()
    {
        _cooldownManager.StartCooldown("skill1");

        Debug.Log("스킬 1 - 전방 슬래시 공격");
        
        // 실제 공격 위치 계산 (PerformRangeAttack과 동일한 로직)
        Vector2 attackPos = (Vector2)transform.position + lastFacingDirection * (3f / 2f);
        
        // 스킬 범위 시각화 (실제 공격 위치에 정확히 표시)
        if (showSkillRanges && skill1Visualizer != null)
        {
            skill1Visualizer.transform.position = attackPos;
            skill1Visualizer.gameObject.SetActive(true);
            skill1Visualizer.UpdatePosition(); // 위치 변경 후 원 다시 그리기
            skill1Visualizer.Show();
        }
        
        PerformRangeAttack(lastFacingDirection, range: 3f, radius: 1f, damage: skill1Damage);
    }

    private void CastSkill2()
    {
        _cooldownManager.StartCooldown("skill2");

        Debug.Log("스킬 2 - 회전 공격");
        
        // 스킬 범위 시각화
        if (showSkillRanges && skill2Visualizer != null)
        {
            skill2Visualizer.Show();
        }
        
        PerformSpinAttack(radius: 2.5f, damage: skill2Damage);
    }

    private async void CastSkill3Async()
    {
        _cooldownManager.StartCooldown("skill3");
        
        Debug.Log("스킬 3 - 점프 공격");
        
        // 기존 점프 취소
        _jumpCancellationTokenSource?.Cancel();
        _jumpCancellationTokenSource?.Dispose();
        _jumpCancellationTokenSource = new System.Threading.CancellationTokenSource();
        
        try
        {
            await PerformJumpAttackAsync(
                targetDistance: 4f, 
                radius: 2f, 
                damage: skill3Damage,
                _jumpCancellationTokenSource.Token
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
            if (hit.CompareTag(GameTags.Enemy))
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
            if (hit.CompareTag(GameTags.Enemy))
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
    private async Awaitable PerformJumpAttackAsync(float targetDistance, float radius, int damage, System.Threading.CancellationToken cancellationToken)
    {
        _isJumping = true;
        
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + lastFacingDirection * targetDistance;
        
        Debug.Log($"[스킬3] 점프 시작! {startPos} → {targetPos}");
        
        // 스킬 범위 시각화 (착지 지점에 고정 표시) - 점프 시작 시점에 착지 위치 계산
        if (showSkillRanges && skill3Visualizer != null)
        {
            skill3Visualizer.transform.position = targetPos; // 착지 목표 위치에 고정
            skill3Visualizer.gameObject.SetActive(true);
            skill3Visualizer.Show();
        }
        
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
                if (hit.CompareTag(GameTags.Enemy))
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
            _isJumping = false;
        }
    }

    // ==================== UI 표시용 (옵션) ====================
    public float GetDashCooldownPercent() => _cooldownManager.GetCooldownPercent("dash");
    public float GetSkill1CooldownPercent() => _cooldownManager.GetCooldownPercent("skill1");
    public float GetSkill2CooldownPercent() => _cooldownManager.GetCooldownPercent("skill2");
    public float GetSkill3CooldownPercent() => _cooldownManager.GetCooldownPercent("skill3");

    /// <summary>
    /// UI에서 쿨다운 매니저 접근용
    /// </summary>
    public CooldownManager GetCooldownManager() => _cooldownManager;

    // ==================== 체력 이벤트 핸들러 ====================

    /// <summary>
    /// 데미지를 받았을 때 처리
    /// </summary>
    private void HandleDamaged(int damage, int remainingHealth)
    {
        Debug.Log($"[PlayerProto] 피격! 데미지: {damage}, 남은 HP: {remainingHealth}");

        // 시각적 피드백
        StartCoroutine(HitFlashEffect());

        // TODO: 추가 피격 효과
        // - 피격 사운드
        // - 화면 흔들림
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

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void HandleDeath()
    {
        if (_isDead) return; // 중복 호출 방지

        _isDead = true;

        Debug.Log("[PlayerProto] 플레이어 사망!");

        // 물리 완전 중지
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // 모든 움직임 고정

        // 모든 입력 및 로직 비활성화
        enabled = false; // MonoBehaviour 비활성화 (Update, FixedUpdate 중지)

        // Collider 비활성화 (적과 충돌 방지)
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 사망 시각 효과
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.gray;
        }

        // TODO: 사망 효과 추가
        // - 사망 애니메이션
        // - 사망 사운드
        // - 게임 오버 UI 표시
        // - 리스폰 시스템

        // 게임 오버 처리
        StartCoroutine(GameOverSequence());
    }

    /// <summary>
    /// 게임 오버 시퀀스
    /// </summary>
    private System.Collections.IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1f); // 1초 대기

        // 페이드 아웃 효과 (선택사항)
        if (_spriteRenderer != null)
        {
            float fadeTime = 1f;
            Color startColor = _spriteRenderer.color;

            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f); // 추가 대기

        Debug.Log("[PlayerProto] 게임 오버! 씬 재시작 중...");

        // 씬 재시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // ==================== 체력 관련 Public 메서드 ====================

    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public int GetCurrentHealth()
    {
        return _healthComponent != null ? _healthComponent.CurrentHealth : 0;
    }

    /// <summary>
    /// 최대 체력 반환
    /// </summary>
    public int GetMaxHealth()
    {
        return _healthComponent != null ? _healthComponent.MaxHealth : maxHealth;
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        if (_healthComponent != null && !_isDead)
        {
            _healthComponent.Heal(amount);
            Debug.Log($"[PlayerProto] 체력 회복: +{amount}");
        }
    }
}
