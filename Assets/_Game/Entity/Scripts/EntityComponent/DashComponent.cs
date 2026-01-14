using System;
using Unity.VisualScripting;
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

    public float DashSpeed => _dashSpeed;
    public float DashDuration => _dashDuration;
    public float DashCooldown => _dashCooldown;
    
    private Rigidbody2D _rigidbody;
    private CooldownComponent _cooldownComponent;
    
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _moveInput;
    private Vector2 _lastFacingDirection;

    public bool IsDashing => _isDashing;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateDash();
    }

    /// <summary>
    /// CooldownComponent를 초기화합니다.
    /// </summary>
    public void Initialize(CooldownComponent cooldownComponent)
    {
        _cooldownComponent = cooldownComponent;
        _cooldownComponent.RegisterCooldown("dash", _dashCooldown);
    }

    /// <summary>
    /// 이동 입력과 마지막 바라본 방향을 설정합니다.
    /// </summary>
    public void SetInputs(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }

    /// <summary>
    /// 구르기를 시작합니다.
    /// </summary>
    public void StartDash()
    {
        if (_cooldownComponent == null || !_cooldownComponent.IsReady("dash"))
        {
            return;
        }

        _isDashing = true;
        _dashTimer = _dashDuration;
        _cooldownComponent.StartCooldown("dash");
        
        Debug.Log("구르기!");
    }

    /// <summary>
    /// 구르기 상태를 업데이트합니다.
    /// </summary>
    public void UpdateDash()
    {
        if (!_isDashing)
        {
            return;
        }

        _dashTimer -= Time.fixedDeltaTime;
        
        var dashDirection = _moveInput.normalized;
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
        if (_cooldownComponent == null)
        {
            return 0f;
        }
        return _cooldownComponent.GetCooldownPercent("dash");
    }
}
