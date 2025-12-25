using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 인게임에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// Unity Inspector에서 Parent 필드에 GlobalLifeTimeScope를 할당해야 함
    /// </summary>
    public class GameLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CoinInventory>(Lifetime.Singleton);
            builder.Register<Inventory>(Lifetime.Singleton);
            builder.Register<EntityFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SceneStartInjector>();
            
            base.Configure(builder);
        }
    }
}