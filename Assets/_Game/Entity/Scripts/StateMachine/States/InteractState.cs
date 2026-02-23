using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 상호작용 상태. 진입 시 한 번 상호작용 시도 후 Idle/Move로 전환합니다.
    /// </summary>
    public class InteractState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private readonly InteractComponent _interactComponent;

        [Inject]
        public InteractState(InteractComponent interactComponent)
        {
            _interactComponent = interactComponent;
        }

        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }

        public EntityStates States => EntityStates.Interact;

        public void OnEnter()
        {
            _interactComponent?.TryInteract();

            var inputContext = _entityStateMachine.InputContext;
            if (inputContext == null)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
                return;
            }
            if (!inputContext.MoveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
                _entityStateMachine.RequestChangeState(EntityStates.Move);
            else
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
        }

        public void OnExit() { }

        public void OnUpdate() { }
    }
}
