using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 대기 상태. 이동 입력이 있으면 Move, 스킬 입력 시 Skill 상태로 전환합니다.
    /// </summary>
    public class IdleState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private EntityAnimationHandler _entityAnimationHandler;

        [Inject]
        public IdleState(EntityAnimationHandler entityAnimationHandler)
        {
            _entityAnimationHandler = entityAnimationHandler;
        }
        
        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }
        
        public EntityStates States => EntityStates.Idle;

        public void OnEnter()
        {
            _entityAnimationHandler?.SetMoving(false);
            ApplyFacingDirection();
        }

        public void OnExit() { }

        public void OnUpdate()
        {
            var input = _entityStateMachine?.InputContext;
            if (input == null)
                return;

            ApplyFacingDirection();

            if (input.SkillInput != null)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Skill);
                return;
            }

            if (!input.MoveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
            {
                _entityStateMachine.RequestChangeState(EntityStates.Move);
            }
        }

        private void ApplyFacingDirection()
        {
            var lastDir = _entityStateMachine.InputContext?.LastMoveDirection ?? Vector2.zero;
            if (lastDir.ApproximatelyEquals(Vector2.zero, 0.01f))
                return;

            _entityAnimationHandler?.SetMovementFromInput(lastDir, false);
        }
    }
}
