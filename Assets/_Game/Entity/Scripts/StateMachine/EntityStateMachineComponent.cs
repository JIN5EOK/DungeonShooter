using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 상태머신을 담당하는 컴포넌트.
    /// </summary>
    [RequireComponent(typeof(EntityBase))]
    public class EntityStateMachineComponent : MonoBehaviour, IEntityStateMachine
    {
        public EntityStates CurrentState => _currentState?.States ?? EntityStates.Idle;
        private IEntityState _currentState;
        private Dictionary<EntityStates, IEntityState> _states;
        public EntityBase Entity { get; private set; }
        public IEntityInputContext InputContext => Entity?.EntityContext?.InputContext;

        [Inject]
        private void Construct(
            EntityBase entityBase,
            IdleState idleState,
            MoveState moveState,
            SkillState skillState,
            DashState dashState,
            InteractState interactState)
        {
            Entity =  entityBase;
            _states = new Dictionary<EntityStates, IEntityState>
            {
                { EntityStates.Idle, idleState },
                { EntityStates.Move, moveState },
                { EntityStates.Skill, skillState },
                { EntityStates.Dash, dashState },
                { EntityStates.Interact, interactState }
            };

            foreach (var state in _states.Values)
            {
                state.Initialize(this);
            }
            _currentState = _states[EntityStates.Idle];
            _currentState.OnEnter();
        }

        private void Update()
        {
            if (Entity?.EntityContext?.InputContext == null)
                return;
            _currentState?.OnUpdate();
        }
        
        public void RequestChangeState(EntityStates nextStates)
        {
            if (!_states.TryGetValue(nextStates, out var nextState) || nextState == _currentState)
            {
                return;
            }

            _currentState.OnExit();
            _currentState = nextState;
            _currentState.OnEnter();
        }
    }
}
