using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 캐릭터 이동을 담당하는 MonoBehaviour 컴포넌트.
    /// 상태에서 Move(Vector2 input)를 호출하여 이동을 적용합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        public float MoveSpeed =>
            _entityBase?.EntityContext?.Stat != null
                ? _entityBase.EntityContext.Stat.GetStat(StatType.MoveSpeed).GetValue()
                : 0;

        private Rigidbody2D _rigidbody;
        private EntityBase _entityBase;

        [Inject]
        private void Construct(EntityBase entityBase, Rigidbody2D rigidbody2D)
        {
            _entityBase = entityBase;
            _rigidbody = rigidbody2D;
        }

        /// <summary>
        /// 입력 벡터에 따라 이동을 적용합니다. 호출 측(상태)에서만 사용합니다.
        /// </summary>
        /// <param name="input">이동 입력 (정규화되지 않아도 됨)</param>
        public void Move(Vector2 input)
        {
            if (!input.ApproximatelyEquals(Vector2.zero, 0.01f))
            {
                _rigidbody.linearVelocity = input.normalized * MoveSpeed;
            }
            else
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}
