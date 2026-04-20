using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class Player : EntityBase
    {
        [Inject] private ICameraManager _cameraManager;
        [Inject] private PlayerInputManager _inputManager;
        
        protected override void Awake()
        {
            base.Awake();
            gameObject.AddOrGetComponent<Rigidbody2D>();
            gameObject.AddOrGetComponent<SpriteRenderer>();
            gameObject.AddOrGetComponent<Animator>();

            var animationHandler = gameObject.AddOrGetComponent<EntityAnimationHandler>();
            var stateMachine = gameObject.AddOrGetComponent<EntityStateMachineComponent>();

            stateMachine.Initialize(
                new IdleState(animationHandler),
                new MoveState(animationHandler),
                new SkillState(animationHandler));
        }

        private void OnEnable()
        {
            _cameraManager?.BindAsync(transform).Forget();
            _inputManager?.BindControlledEntity(this);
            GetContext().HealthModel.OnDeath += OnDeath;
        }

        private void OnDisable()
        {
            GetContext().HealthModel.OnDeath -= OnDeath;
            _inputManager?.UnbindControlledEntity();
        }

        private void OnDeath()
        {
            gameObject.ReleaseOrDestroy();
        }
    }
}
