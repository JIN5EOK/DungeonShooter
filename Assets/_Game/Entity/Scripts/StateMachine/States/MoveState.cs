using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 이동 상태. MovementComponent.Move(input)와 애니메이션을 적용합니다. 입력이 없으면 Idle, 대시/스킬 시 해당 상태로 전환합니다.
    /// </summary>
    public class MoveState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private readonly MovementComponent _movementComponent;
        private readonly EntityAnimationHandler _entityAnimationHandler;
        private readonly DashComponent _dashComponent;

        [Inject]
        public MoveState(MovementComponent movementComponent, EntityAnimationHandler entityAnimationHandler, DashComponent dashComponent)
        {
            _movementComponent = movementComponent;
            _entityAnimationHandler = entityAnimationHandler;
            _dashComponent = dashComponent;
        }

        public EntityStates States => EntityStates.Move;

        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void OnUpdate()
        {
            var input = _entityStateMachine.InputContext;

            _movementComponent?.Move(input.MoveInput);
            _entityAnimationHandler?.SetMovementFromInput(input.MoveInput);

            if (input.DashInput && _dashComponent != null && _dashComponent.IsReady)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Dash);
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

            if (input.MoveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
            {
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
            }
        }
    }
}
