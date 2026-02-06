using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    /// <summary>
    /// 대부분의 씬에서 필요한 기본 의존성 등록 작업 수행
    /// </summary>
    public class CommonSceneInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<SceneResourceProvider>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterComponentOnNewGameObject<UIManager>(Lifetime.Scoped);
        }
    }
}