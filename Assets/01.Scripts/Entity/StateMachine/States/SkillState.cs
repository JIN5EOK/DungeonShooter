using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 시전 상태. skillInput 스킬을 실행하고 완료 시 Idle/Move로 전환합니다.
    /// </summary>
    public class SkillState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private readonly IMovementComponent _movementComponent;
        private readonly EntityAnimationHandler _entityAnimationHandler;

        private Skill _executingSkill;
        private bool _executeFinished;

        [Inject]
        public SkillState(IMovementComponent movementComponent, EntityAnimationHandler entityAnimationHandler)
        {
            _movementComponent = movementComponent;
            _entityAnimationHandler = entityAnimationHandler;
        }

        public EntityStates States => EntityStates.Skill;

        public void Initialize(IEntityStateMachine stateMachine)
        {
            _entityStateMachine = stateMachine;
        }

        public void OnEnter()
        {
            _executeFinished = false;
            var inputContext = _entityStateMachine.InputContext;
            if (inputContext == null)
            {
                _executeFinished = true;
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
                return;
            }

            _executingSkill = inputContext.SkillInput;
            if (_executingSkill == null || _entityStateMachine.Entity == null || _executingSkill.IsCooldown == true)
            {
                _executeFinished = true;
                return;
            }

            _movementComponent?.Move(Vector2.zero);
            _entityAnimationHandler?.SetMoving(false);
            ExecuteSkillAsync().Forget();
            inputContext.SkillInput = null;
        }

        private async UniTaskVoid ExecuteSkillAsync()
        {
            await _executingSkill.Execute(_entityStateMachine.Entity);
            _executeFinished = true;
        }

        public void OnExit()
        {
            _executingSkill = null;
        }

        public void OnUpdate()
        {
            if (!_executeFinished)
            {
                return;
            }

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
