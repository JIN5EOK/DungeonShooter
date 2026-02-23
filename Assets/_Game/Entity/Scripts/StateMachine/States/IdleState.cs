using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 대기 상태. 이동 입력이 있으면 Move, 대시/스킬 입력 시 해당 상태로 전환합니다.
    /// </summary>
    public class IdleState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private IDashComponent _dashComponent;
        private EntityAnimationHandler _entityAnimationHandler;
        private IMovementComponent _movementComponent;

        [Inject]
        public IdleState(IDashComponent dashComponent, EntityAnimationHandler entityAnimationHandler, IMovementComponent movementComponent)
        {
            _dashComponent = dashComponent;
            _entityAnimationHandler = entityAnimationHandler;
            _movementComponent = movementComponent;
        }
        
        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }
        
        public EntityStates States => EntityStates.Idle;

        public void OnEnter()
        {
            _movementComponent?.Move(Vector2.zero);
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
            
            if (input.DashInput && _dashComponent != null && _dashComponent.IsReady)
            {
                _entityStateMachine?.RequestChangeState(EntityStates.Dash);
                return;
            }

            if (input.InteractInput)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Interact);
                return;
            }

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
