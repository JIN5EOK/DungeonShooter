using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 캐릭터 구르기(회피)를 담당하는 MonoBehaviour 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DashComponent : MonoBehaviour
    {
        [Header("구르기 설정")]
        [SerializeField] private float _dashSpeed = 15f;
        [SerializeField] private float _dashDuration = 0.3f;
        [SerializeField] private float _dashCooldown = 1.0f;

        public float DashSpeed => _dashSpeed;
        public float DashDuration => _dashDuration;
        public float DashCooldown => _dashCooldown;
        
        private Rigidbody2D _rigidbody;
        private MovementComponent _movementComponent;
        private bool _isDashing;
        private float _dashTimer;
        private float _cooldownRemaining;
        private Vector2 _moveInput;
        
        public bool IsDashing => _isDashing;
        public bool IsReady => _cooldownRemaining <= 0f;
        
        [Inject]
        private void Construct(Rigidbody2D rigidbody2D,  MovementComponent movementComponent)
        {

            _rigidbody = rigidbody2D;
            _movementComponent = movementComponent;
        }

        private void Update()
        {
            UpdateCooldown(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            UpdateDash();
        }

        /// <summary>
        /// 구르기를 시작합니다.
        /// </summary>
        public void StartDash()
        {
            if (!IsReady)
            {
                return;
            }

            _isDashing = true;
            _dashTimer = _dashDuration;
            _cooldownRemaining = _dashCooldown;
            _moveInput = _movementComponent.LookDirection;
            LogHandler.Log<DashComponent>("구르기!");
        }

        /// <summary>
        /// 구르기 상태를 업데이트합니다.
        /// </summary>
        private void UpdateDash()
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
            if (_dashCooldown <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_cooldownRemaining / _dashCooldown);
        }

        public float GetRemainingCooldown()
        {
            return Mathf.Max(0f, _cooldownRemaining);
        }

        private void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                _cooldownRemaining = 0f;
                return;
            }

            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining < 0f)
            {
                _cooldownRemaining = 0f;
            }
        }
    }
}
