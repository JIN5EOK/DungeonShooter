using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class StageFactoryInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<EnemyFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ItemFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<FieldItemFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillObjectFactory>(Lifetime.Scoped).AsImplementedInterfaces();

        }
    }
}