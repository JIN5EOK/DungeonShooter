using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class StageLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneResourceProvider>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<CameraManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<EnemyFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillObjectFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerLevelManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SkillHelper>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<GameMessageService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<GameExitService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterComponentOnNewGameObject<SceneUIManager>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<SoundSfxService>(Lifetime.Scoped).AsImplementedInterfaces();
            
            // HUD 뷰 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<GameHudGroupUI>(UIAddresses.UI_InGameHud), Lifetime.Scoped);
            
            // 기타 UI 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<AlertMessageUI>(UIAddresses.UI_AlertMessage), Lifetime.Scoped);

            // 팝업 뷰 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<PauseMenuUI>(UIAddresses.UI_GamePause, false), Lifetime.Scoped);
            
            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<GameHudGroupUI>();
                resolver.Resolve<SkillLevelUpUI>();
                resolver.Resolve<AlertMessageUI>();
                resolver.Resolve<PauseMenuUI>();
            });
            base.Configure(builder);
        }
    }
}