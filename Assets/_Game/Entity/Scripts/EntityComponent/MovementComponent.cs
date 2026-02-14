using Jin5eok;
using UnityEngine;
using VContainer;

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
            get => _entityBase != null && _entityBase.StatGroup != null ? _entityBase.StatGroup.GetStat(StatType.MoveSpeed).GetValue() : 0;
        }
        private Rigidbody2D _rigidbody;
        private EntityBase _entityBase;
        private EntityAnimationHandler _animationHandler;

        [Inject]
        private void Construct(EntityBase entityBase, Rigidbody2D rigidbody2D, EntityAnimationHandler animationHandler)
        {
            _animationHandler = animationHandler;
            _entityBase = entityBase;
            _rigidbody = rigidbody2D;
        }
        
        private void Update()
        {
            Move();
            UpdateAnimation();
        }

        /// <summary>
        /// 이동 방향과 이동 여부에 따라 애니메이션을 갱신한다.
        /// </summary>
        private void UpdateAnimation()
        {
            if (_animationHandler == null)
                return;

            var isMoving = !Direction.ApproximatelyEquals(Vector2.zero, 0.01f);
            _animationHandler.SetMoving(isMoving);

            if (isMoving)
            {
                var dir = Vector2ToDirection(LookDirection);
                _animationHandler.SetDirection(dir);
            }
        }

        private static Direction Vector2ToDirection(Vector2 v)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
                return v.x > 0 ? DungeonShooter.Direction.Right : DungeonShooter.Direction.Left;

            return v.y > 0 ? DungeonShooter.Direction.Up : DungeonShooter.Direction.Down;
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
