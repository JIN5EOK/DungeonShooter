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
        private readonly DashComponent _dashComponent;
        private readonly EntityAnimationHandler _entityAnimationHandler;
        private readonly MovementComponent _movementComponent;

        [Inject]
        public IdleState(DashComponent dashComponent, EntityAnimationHandler entityAnimationHandler, MovementComponent movementComponent)
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
        }

        public void OnExit() { }

        public void OnUpdate()
        {
            var input = _entityStateMachine.InputContext;

            if (input.DashInput && _dashComponent != null && _dashComponent.IsReady)
            {
                _entityStateMachine?.RequestChangeState(EntityStates.Dash);
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
    }
}
