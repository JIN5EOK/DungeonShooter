using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace DungeonShooter
{
    public class Player : EntityBase
    {
        [Inject] private ICameraManager _cameraManager;
        protected override void Awake()
        {
            base.Awake();

            var animationHandler = gameObject.AddOrGetComponent<EntityAnimationHandler>();
            var stateMachine = gameObject.AddOrGetComponent<EntityStateMachineComponent>();

            stateMachine.Initialize(
                new IdleState(animationHandler),
                new MoveState(animationHandler),
                new SkillState(animationHandler));
        }

        public override void SetContext(IEntityContext context)
        {
            var beforeContext = GetContext();
            if (beforeContext != null)
            {
                beforeContext.HealthModel.OnDeath -= OnDeath;
            }
            
            _cameraManager?.BindAsync(transform).Forget();
            context.HealthModel.OnDeath += OnDeath;
            base.SetContext(context);
        }

        private void OnDeath()
        {
            gameObject.ReleaseOrDestroy();
        }

        public void OnMove(InputValue context)
        {
            var contextVal = context.Get<Vector2>();
            LogHandler.Log<Player>(contextVal.ToString());
            GetContext().InputContext.MoveInput = contextVal;
        }
    }
}
