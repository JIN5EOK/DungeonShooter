using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 애니메이터를 감싸서 방향(Direction)과 이동(Walk) 파라미터를 설정하는 컴포넌트.
    /// </summary>
    public class EntityAnimationHandler : MonoBehaviour
    {
        private const string DirectionParamName = "Direction";
        private const string WalkParamName = "IsWalk";

        private static readonly int DirectionParamId = Animator.StringToHash(DirectionParamName);
        private static readonly int WalkParamId = Animator.StringToHash(WalkParamName);

        private static readonly int DirectionUp = 2;
        private static readonly int DirectionDown = 8;
        private static readonly int DirectionLeft = 4;
        private static readonly int DirectionRight = 6;

        private Animator _animator;
        
        [Inject]
        private void Construct(Animator animator)
        {
            _animator = animator;
        }

        /// <summary>
        /// 애니메이터의 Direction Int 파라미터를 설정한다.
        /// Up=2, Down=8, Left=4, Right=6
        /// </summary>
        public void SetDirection(Direction direction)
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
                return;

            var value = direction switch
            {
                Direction.Up => DirectionUp,
                Direction.Down => DirectionDown,
                Direction.Left => DirectionLeft,
                Direction.Right => DirectionRight,
                _ => DirectionUp
            };
            
            _animator.SetInteger(DirectionParamId, value);
        }

        /// <summary>
        /// 애니메이터의 Walk Bool 파라미터를 설정한다.
        /// </summary>
        public void SetMoving(bool value)
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
                return;

            _animator.SetBool(WalkParamId, value);
        }
    }
}
