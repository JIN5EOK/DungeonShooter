using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 특정 기능이 종료될 때까지 해제되지 않는 리소스를 제공
    /// </summary>
    public class FeatureResourceProvider : ResourceProviderBase
    {
        [Inject]
        public FeatureResourceProvider(IObjectResolver resolver) : base(resolver)
        {
        }
    }
}
