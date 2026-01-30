using System.Diagnostics;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 스테이지 씬에서 사용되는 서비스들을 등록하는 라이프타임 스코프
    /// </summary>
    public class StageSceneLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CoinInventory>(Lifetime.Scoped);
            builder.Register<Inventory>(Lifetime.Scoped);
            builder.Register<SceneResourceProvider>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<EnemyFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<RoomDataRepository>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ItemFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageManager>(Lifetime.Scoped);
            builder.RegisterComponentOnNewGameObject<GameManager>(Lifetime.Scoped);
            base.Configure(builder);
        }

        protected override void Awake()
        {
            base.Awake();
            Container.Resolve<GameManager>();
        }
    }
}