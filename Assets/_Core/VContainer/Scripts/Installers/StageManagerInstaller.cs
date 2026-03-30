using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class StageManagerInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<ObjectCullingManager>(Lifetime.Scoped);
            builder.Register<EntityManager>(Lifetime.Scoped);
            builder.Register<PlayerInputManager>(Lifetime.Scoped);
            builder.Register<PlayerContextManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerLevelService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillSlotService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ItemDropService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<GameMessageService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<GameExitService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<GameResultService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<Inventory>(Lifetime.Scoped).AsImplementedInterfaces();
            
            builder.RegisterBuildCallback((resolver) =>
            {
                resolver.Resolve<EntityManager>();
                resolver.Resolve<IItemDropService>();
                resolver.Resolve<PlayerInputManager>();
                resolver.Resolve<ObjectCullingManager>();
            });
        }
    }
}