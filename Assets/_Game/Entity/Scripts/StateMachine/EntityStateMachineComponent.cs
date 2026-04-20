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
        private Dictionary<EntityStates, IEntityState> _states = new();
        public EntityBase Entity { get; private set; }
        public IEntityInputContext InputContext => Entity?.GetContext()?.InputContext;

        private void Awake()
        {
            Entity = GetComponent<EntityBase>();
        }

        [Inject]
        private void Construct(EntityBase entityBase)
        {
            Entity = entityBase;
        }

        public void Initialize(params IEntityState[] states)
        {
            foreach (var s in states)
            {
                var state = s.States; 
                _states[state] = s;
                s.Initialize(this);
            }
            
            _currentState = _states[EntityStates.Idle];
            _currentState.OnEnter();
        }
        
        private void Update()
        {
            if (Entity?.GetContext()?.InputContext == null)
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
