using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 구르기(회피) 상태. DashComponent가 이동을 담당합니다. 구르기 종료 시 Idle/Move로 전환합니다.
    /// </summary>
    public class DashState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private readonly IDashComponent _dashComponent;

        [Inject]
        public DashState(IDashComponent dashComponent)
        {
            _dashComponent = dashComponent;
        }

        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }
        
        public EntityStates States => EntityStates.Dash;

        public void OnEnter()
        {
            var inputContext = _entityStateMachine.InputContext;
            if (inputContext == null)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
                return;
            }
            _dashComponent?.StartDash(inputContext.LastMoveDirection);
        }

        public void OnExit()
        {
        }

        public void OnUpdate()
        {
            if (_dashComponent == null || !_dashComponent.IsDashing)
            {
                var input = _entityStateMachine.InputContext;
                if (input == null)
                {
                    _entityStateMachine.RequestChangeState(EntityStates.Idle);
                    return;
                }
                if (!input.MoveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
                {
                    _entityStateMachine.RequestChangeState(EntityStates.Move);
                }
                else
                {
                    _entityStateMachine.RequestChangeState(EntityStates.Idle);
                }
            }
        }
    }
}
