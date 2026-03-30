using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 전역 UI 객체들의 생성과 의존성 바인딩을 담당하는 인스톨러
    /// </summary>
    public class GlobalUIInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register(resolver => resolver.Resolve<IGlobalUIManager>().GetSingletonUISync<LoadingView>(UIAddresses.UI_Loading), Lifetime.Singleton);

            builder.RegisterBuildCallback(resolver =>
            {
                resolver.Resolve<LoadingView>();
            });
        }
    }
}
