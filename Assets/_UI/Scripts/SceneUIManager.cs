using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 씬별 UI 생성 및 관리를 담당하는 매니저
    /// </summary>
    public class SceneUIManager : UIManagerBase
    {
        public override int SortingOrder => (int)BaseSortingOrder.SceneUI;
        [Inject]
        public void Construct(SceneResourceProvider resourceProviderTemp)
        {
            Initialize(resourceProviderTemp);
        }
    }
}
