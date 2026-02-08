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
            get => _entityBase != null && _entityBase.StatGroup != null ? _entityBase.StatGroup.GetStat(StatType.MoveSpeed) : 0;
        }
        private Rigidbody2D _rigidbody;
        private EntityBase _entityBase;

        private void Awake()
        {
            _entityBase = GetComponent<EntityBase>();
            _rigidbody = GetComponent<Rigidbody2D>();
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
            var velocity = Direction.normalized * MoveSpeed;
            _rigidbody.linearVelocity = velocity;
        }
    }
}
