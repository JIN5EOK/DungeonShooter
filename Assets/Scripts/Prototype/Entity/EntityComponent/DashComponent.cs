using UnityEngine;

/// <summary>
/// 캐릭터 구르기(회피)를 담당하는 MonoBehaviour 컴포넌트
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DashComponent : MonoBehaviour
{
    [Header("구르기 설정")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.3f;
    [SerializeField] private float _dashCooldown = 0.8f;

    private Rigidbody2D _rigidbody;
    private CooldownManager _cooldownManager;
    
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _moveInput;
    private Vector2 _lastFacingDirection;

    public bool IsDashing => _isDashing;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// CooldownManager를 초기화합니다.
    /// </summary>
    public void Initialize(CooldownManager cooldownManager)
    {
        _cooldownManager = cooldownManager;
        _cooldownManager.RegisterCooldown("dash", _dashCooldown);
    }

    /// <summary>
    /// 이동 입력과 마지막 바라본 방향을 설정합니다.
    /// </summary>
    public void SetInputs(Vector2 moveInput, Vector2 lastFacingDirection)
    {
        _moveInput = moveInput;
        _lastFacingDirection = lastFacingDirection;
    }

    /// <summary>
    /// 구르기를 시작합니다.
    /// </summary>
    public void StartDash()
    {
        if (_cooldownManager == null || !_cooldownManager.IsReady("dash"))
        {
            return;
        }

        _isDashing = true;
        _dashTimer = _dashDuration;
        _cooldownManager.StartCooldown("dash");
        
        Debug.Log("구르기!");
    }

    /// <summary>
    /// 구르기 상태를 업데이트합니다. FixedUpdate에서 호출해야 합니다.
    /// </summary>
    public void UpdateDash()
    {
        if (!_isDashing)
        {
            return;
        }

        _dashTimer -= Time.fixedDeltaTime;
        
        Vector2 dashDirection = _moveInput.magnitude > 0 ? _moveInput.normalized : _lastFacingDirection;
        _rigidbody.linearVelocity = dashDirection * _dashSpeed;

        if (_dashTimer <= 0)
        {
            _isDashing = false;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 구르기 쿨다운 퍼센트를 반환합니다.
    /// </summary>
    public float GetCooldownPercent()
    {
        if (_cooldownManager == null)
        {
            return 0f;
        }
        return _cooldownManager.GetCooldownPercent("dash");
    }
}
