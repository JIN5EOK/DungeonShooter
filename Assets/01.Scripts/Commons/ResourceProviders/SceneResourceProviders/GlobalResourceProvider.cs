using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임이 종료될 때까지 해제되지 않는 전역 리소스를 제공
    /// </summary>
    public class GlobalResourceProvider : ResourceProviderBase, IGlobalResourceProvider
    {
        [Inject]
        public GlobalResourceProvider(IObjectResolver resolver) : base(resolver)
        {
        }
    }
}
