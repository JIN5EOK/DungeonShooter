using System;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// Stage 씬의 UI 객체들의 생성과 의존성 바인딩을 담당하는 인스톨러
    /// </summary>
    public class StageSceneUIInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            // HUD 뷰 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<GameHudGroupUI>(UIAddresses.UI_InGameHud), Lifetime.Scoped);
            
            // 기타 UI 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<AlertMessageView>(UIAddresses.UI_AlertMessage, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<InventoryView>(UIAddresses.UI_Inventory, false), Lifetime.Scoped);

            // 팝업 뷰 등록
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<GamePauseView>(UIAddresses.UI_GamePause, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<ISceneUIManager>().GetSingletonUISync<GameResultView>(UIAddresses.UI_GameResult, false), Lifetime.Scoped);

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<SkillLevelUpUI>();
                resolver.Resolve<InventoryView>();
                resolver.Resolve<AlertMessageView>();
                resolver.Resolve<GamePauseView>();
                resolver.Resolve<GameResultView>();
            });
        }
    }
}
