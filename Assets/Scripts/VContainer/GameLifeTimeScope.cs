using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class GameLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CoinInventory>(Lifetime.Singleton);
            builder.Register<EntityFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SceneStartInjector>();
            base.Configure(builder);
        }
    }
}