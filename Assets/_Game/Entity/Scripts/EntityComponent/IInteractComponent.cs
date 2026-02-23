using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 상호작용 기능을 담당하는 컴포넌트 인터페이스.
    /// </summary>
    public interface IInteractComponent
    {
        public void SetInteractNotice(GameObject notice);
        public void TryInteract();
    }
}
