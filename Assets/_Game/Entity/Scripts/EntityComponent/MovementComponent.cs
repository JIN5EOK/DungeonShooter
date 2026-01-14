using System;
using UnityEngine;

/// <summary>
/// 캐릭터 이동을 담당하는 MonoBehaviour 컴포넌트
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementComponent : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 5f;

    public Vector2 Direction { get; set; }
    public float MoveSpeed { get; set; }
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    
    /// <summary>
    /// 캐릭터를 이동시킵니다.
    /// </summary>
    public void Move()
    {
        var velocity = Direction.normalized * _moveSpeed;
        _rigidbody.linearVelocity = velocity;
    }

    public void Update()
    {
        Move();
    }
}
