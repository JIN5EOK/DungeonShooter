using UnityEngine;

/// <summary>
/// 캐릭터 이동을 담당하는 MonoBehaviour 컴포넌트
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementComponent : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rigidbody;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 이동 입력을 설정합니다.
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    /// <summary>
    /// 캐릭터를 이동시킵니다.
    /// </summary>
    public void Move()
    {
        Vector2 velocity = _moveInput.normalized * _moveSpeed;
        _rigidbody.linearVelocity = velocity;
    }

    /// <summary>
    /// 현재 이동 입력을 반환합니다.
    /// </summary>
    public Vector2 GetMoveInput() => _moveInput;

    /// <summary>
    /// 이동 속도를 설정합니다.
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    /// <summary>
    /// 이동 속도를 반환합니다.
    /// </summary>
    public float GetMoveSpeed() => _moveSpeed;
}
