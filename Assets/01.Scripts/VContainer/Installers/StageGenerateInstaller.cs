using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class StageGenerateInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<RoomDataRepository>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageGenerator>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<StageInstantiator>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<RoomInstantiator>(Lifetime.Scoped);
        }
    }
}