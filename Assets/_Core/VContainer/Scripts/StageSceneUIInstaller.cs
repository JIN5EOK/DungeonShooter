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
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<HealthBarHudUI>(UIAddresses.UI_HpHud), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<ExpGaugeHudUI>(UIAddresses.UI_ExpHud), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<PlayerStatusHudUI>(UIAddresses.UI_PlayerStatusHud), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<SkillCooldownHudUI>(UIAddresses.UI_SkillCooldownHud), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<GameButtonHudUI>(UIAddresses.UI_GameButtonHud), Lifetime.Scoped);
            
            // 기타 UI 등록
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<SkillLevelUpUI>(UIAddresses.UI_SkillLevelUp, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<AlertMessageUI>(UIAddresses.UI_AlertMessage, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<InventoryUI>(UIAddresses.UI_Inventory, false), Lifetime.Scoped);

            // 팝업 뷰 등록
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<GamePauseView>(UIAddresses.UI_GamePause, false), Lifetime.Scoped);
            builder.Register(resolver => resolver.Resolve<UIManager>().GetSingletonUISync<GameResultView>(UIAddresses.UI_GameResult, false), Lifetime.Scoped);

            // 일괄 관리자
            builder.Register<GameHudGroupUI>(Lifetime.Scoped);

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<SkillLevelUpUI>();
                resolver.Resolve<InventoryUI>();
                resolver.Resolve<AlertMessageUI>();
                resolver.Resolve<GamePauseView>();
                resolver.Resolve<GameResultView>();
            });
        }
    }
}
