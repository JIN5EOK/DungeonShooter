using System.Diagnostics;
using UnityEngine.InputSystem;
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
            
            builder.Register<GameManager>(Lifetime.Scoped);
            builder.Register<PlayerStatusManager>(Lifetime.Scoped);
            builder.Register<PlayerInputController>(Lifetime.Scoped);
            builder.Register<PlayerSkillManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerLevelService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<Inventory>(Lifetime.Scoped);
            
            builder.Register<PlayerFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<EnemyFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ItemFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillObjectFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            
            builder.Register<StageManager>(Lifetime.Scoped);
            builder.Register<RoomDataRepository>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageGenerator>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageInstantiator>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<RoomInstantiator>(Lifetime.Scoped);
            
            builder.Register<StageSceneUIController>(Lifetime.Scoped);
            
            builder.Register<EntitySkillContainer>(Lifetime.Transient);
            
            builder.RegisterComponentOnNewGameObject<StageSceneInitializer>(Lifetime.Scoped);
            base.Configure(builder);
            
        }

        protected override void Awake()
        {
            base.Awake();
            Container.Resolve<GameManager>();
            Container.Resolve<PlayerInputController>();
            Container.Resolve<StageSceneInitializer>();
        }
    }
}