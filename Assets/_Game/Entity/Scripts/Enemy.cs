using Jin5eok;

namespace DungeonShooter
{
    public class Enemy : EntityBase
    {
        private IEntityAI _ai;
        
        protected override void Awake()
        {
            base.Awake();
            gameObject.AddOrGetComponent<EntityAnimationHandler>();
            gameObject.AddOrGetComponent<EntityStateMachineComponent>();
        }

        public override void SetContext(IEntityContext context)
        {
            var beforeContext = GetContext();
            if (beforeContext != null)
                beforeContext.HealthModel.OnDeath -= OnDeath;

            var animationHandler = GetComponent<EntityAnimationHandler>();
            GetComponent<IEntityStateMachine>()?.Initialize(
                new IdleState(animationHandler),
                new MoveState(animationHandler),
                new SkillState(animationHandler));

            context.HealthModel.OnDeath += OnDeath;
            base.SetContext(context);
            context.HealthModel.ResetState();
        }

        public void SetAI(IEntityAI ai)
        {
            _ai = ai;
        }

        private void Update()
        {
            _ai?.Tick();
        }

        private void OnDeath()
        {
            gameObject.ReleaseOrDestroy();
        }
    }
}
