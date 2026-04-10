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
            new StageManagerInstaller().Install(builder);
            new StageFactoryInstaller().Install(builder);
            new StageGenerateInstaller().Install(builder);
            new StageSceneUIInstaller().Install(builder);
            
            // 씬 초기화 로직

            builder.RegisterComponentOnNewGameObject<StageSceneInitializer>(Lifetime.Scoped);
            
            builder.RegisterBuildCallback((resolver) =>
            {
                resolver.Resolve<StageSceneInitializer>();
            });

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<StageSceneInteractionMediator>().Register();
            });
            
            base.Configure(builder);
        }
    }
}