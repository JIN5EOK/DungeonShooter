using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 이동 상태. 입력에 따라 애니메이션을 적용합니다. 입력이 없으면 Idle, 스킬 입력 시 Skill 상태로 전환합니다.
    /// </summary>
    public class MoveState : IEntityState
    {
        private IEntityStateMachine _entityStateMachine;
        private readonly EntityAnimationHandler _entityAnimationHandler;

        [Inject]
        public MoveState(EntityAnimationHandler entityAnimationHandler)
        {
            _entityAnimationHandler = entityAnimationHandler;
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
            if (input == null)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
                return;
            }

            ApplyMovement(input.MoveInput);
            _entityAnimationHandler?.SetMovementFromInput(input.MoveInput);

            if (input.SkillInput != 0)
            {
                _entityStateMachine.RequestChangeState(EntityStates.Skill);
                return;
            }

            if (input.MoveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
            {
                _entityStateMachine.RequestChangeState(EntityStates.Idle);
            }
        }

        private void ApplyMovement(Vector2 moveInput)
        {
            if (_entityStateMachine?.Entity?.GetContext()?.Stats == null)
                return;

            if (moveInput.ApproximatelyEquals(Vector2.zero, 0.01f))
                return;

            var speed = _entityStateMachine.Entity.GetContext().Stats.GetStat(StatType.MoveSpeed).GetValue();
            _entityStateMachine.Entity.transform.position += (Vector3)(moveInput.normalized * speed * Time.deltaTime);
        }
    }
}
