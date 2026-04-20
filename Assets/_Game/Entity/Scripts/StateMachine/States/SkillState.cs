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
        private readonly EntityAnimationHandler _entityAnimationHandler;

        private Skill _executingSkill;
        private bool _executeFinished;

        [Inject]
        public SkillState(EntityAnimationHandler entityAnimationHandler)
        {
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

            var entity = _entityStateMachine.Entity;
            var skillEntryId = inputContext.SkillInput;
            _executingSkill = entity != null && skillEntryId != 0
                ? entity.GetContext().Skill.GetSkill(skillEntryId)
                : null;
            if (entity == null || _executingSkill == null || _executingSkill.IsCooldown)
            {
                inputContext.SkillInput = 0;
                _executeFinished = true;
                return;
            }

            _entityAnimationHandler?.SetMoving(false);
            ExecuteSkillAsync().Forget();
            inputContext.SkillInput = 0;
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
