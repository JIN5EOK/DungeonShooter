using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 씬에서 사용할 리소스를 제공하는 기본 구현 클래스
    /// </summary>
    public class SceneResourceProvider : ResourceProviderBase, ISceneResourceProvider
    {
        [Inject]
        public SceneResourceProvider(IObjectResolver resolver) : base(resolver)
        {
        }
    }
}
