using Jin5eok;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class EntityLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IdleState>(Lifetime.Scoped).AsSelf();
            builder.Register<MoveState>(Lifetime.Scoped).AsSelf();
            builder.Register<SkillState>(Lifetime.Scoped).AsSelf();
            builder.Register<DashState>(Lifetime.Scoped).AsSelf();
            
            builder.Register(container => AddComponentAndInject<EntityBase>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<EntityAnimationHandler>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<MovementComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<HealthComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<InteractComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<DashComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<CameraTrackComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<AIComponent>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<Animator>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<Rigidbody2D>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<SpriteRenderer>(), Lifetime.Scoped);
            builder.Register(container => AddComponentAndInject<EntityStateMachineComponent>(), Lifetime.Scoped).AsImplementedInterfaces();

            base.Configure(builder);
        }

        private T AddComponentAndInject<T>() where T : Component
        {
            var comp = gameObject.AddOrGetComponent<T>();
            Container.Inject(comp);
            return comp;
        }
    }
}