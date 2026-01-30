using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 캐릭터 이동을 담당하는 MonoBehaviour 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        private float _moveSpeed;
        private Vector2 _direction;
        public Vector2 Direction
        {
            get => _direction;
            set
            {
                _direction = value;

                if (value != Vector2.zero)
                {
                    LookDirection =  value.normalized;
                }
            }
        }

        public Vector2 LookDirection
        {
            get;
            private set;
        }
        
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }
        private Rigidbody2D _rigidbody;
        private EntityStatsComponent _statsComponent;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _statsComponent = GetComponent<EntityStatsComponent>();
            _moveSpeed = _statsComponent.GetStat(StatType.MoveSpeed);
        }

        private void Update()
        {
            Move();
        }
        
        /// <summary>
        /// 캐릭터를 이동시킵니다.
        /// </summary>
        private void Move()
        {
            var velocity = Direction.normalized * _moveSpeed;
            _rigidbody.linearVelocity = velocity;
        }
    }
}
