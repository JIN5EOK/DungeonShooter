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
            new CommonSceneInstaller().Install(builder);
            builder.Register<BattleManager>(Lifetime.Scoped).AsSelf();
            builder.Register<PlayerLevelManager>(Lifetime.Scoped).AsSelf();
            builder.Register<CoinInventory>(Lifetime.Scoped);
            builder.Register<Inventory>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<EnemyFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<RoomDataRepository>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ItemFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageManager>(Lifetime.Scoped);
            builder.RegisterComponentOnNewGameObject<GameManager>(Lifetime.Scoped);
            builder.Register<PlayerStatusManager>(Lifetime.Scoped);
            builder.Register<PlayerSkillManager>(Lifetime.Scoped);
            builder.Register<PlayerInputController>(Lifetime.Scoped);
            builder.Register<PlayerInstanceManager>(Lifetime.Scoped);
            builder.Register<EntitySkillContainer>(Lifetime.Transient);
            base.Configure(builder);
        }

        protected override void Awake()
        {
            base.Awake();
            Container.Resolve<BattleManager>();
            Container.Resolve<GameManager>();
        }
    }
}