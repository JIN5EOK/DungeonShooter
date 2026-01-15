using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 스테이지 씬에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// Bootstrap에서 EnqueueParent로 등록된 GlobalLifeTimeScope를 자동으로 부모로 사용
    /// </summary>
    public class StageSceneLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SceneStartInjector>();
            builder.Register<CoinInventory>(Lifetime.Scoped);
            builder.Register<Inventory>(Lifetime.Scoped);
            builder.Register<StageResourceProvider>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<RoomDataRepository>(Lifetime.Scoped).AsImplementedInterfaces();
            base.Configure(builder);
        }
    }
}