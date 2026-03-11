using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 전역(씬 전환 시 유지되는) 단위 UI 관리를 담당하는 매니저
    /// </summary>
    public class GlobalUIManager : UIManagerBase
    {
        [Inject]
        public void Construct(GlobalResourceProvider resourceProvider)
        {
            Initialize(resourceProvider);
        }
    }
}
