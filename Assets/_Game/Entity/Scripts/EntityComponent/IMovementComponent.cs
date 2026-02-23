using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 이동을 담당하는 컴포넌트 인터페이스.
    /// </summary>
    public interface IMovementComponent
    {
        public float MoveSpeed { get; }
        public void Move(Vector2 input);
    }
}
