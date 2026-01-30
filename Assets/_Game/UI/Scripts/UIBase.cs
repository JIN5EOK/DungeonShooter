using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// UI 공통 베이스. Show/Hide/Destroy와 생명주기 이벤트를 제공한다.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnDestroyEvent;

        public abstract UIType Type { get; }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow?.Invoke();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        /// <summary>
        /// UI를 제거한다. UIManager에서 관리 중이면 RemoveUI를 사용하는 것이 좋다.
        /// </summary>
        public virtual void Destroy()
        {
            OnDestroyEvent?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
}
