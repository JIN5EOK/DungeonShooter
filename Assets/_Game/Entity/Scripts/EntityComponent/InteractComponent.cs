using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 상호작용 기능을 담당하는 MonoBehaviour 컴포넌트
    /// </summary>
    public class InteractComponent : MonoBehaviour
    {
        private HashSet<IInteractable> _nearbyInteractables = new HashSet<IInteractable>();

        /// <summary>
        /// 상호작용 가능한 오브젝트를 등록합니다.
        /// </summary>
        public void RegisterInteractable(IInteractable interactable)
        {
            if (interactable != null)
            {
                _nearbyInteractables.Add(interactable);
            }
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트를 제거합니다.
        /// </summary>
        public void UnregisterInteractable(IInteractable interactable)
        {
            if (interactable != null)
            {
                _nearbyInteractables.Remove(interactable);
            }
        }

        /// <summary>
        /// 상호작용을 시도합니다.
        /// </summary>
        public void TryInteract()
        {
            // 가장 가까운 상호작용 가능한 오브젝트 찾기
            IInteractable closestInteractable = null;
            var closestDistance = float.MaxValue;

            foreach (IInteractable interactable in _nearbyInteractables)
            {
                if (interactable != null && interactable.CanInteract)
                {
                    // MonoBehaviour인 경우 거리 계산
                    if (interactable is MonoBehaviour mb)
                    {
                        var distance = Vector2.Distance(transform.position, mb.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = interactable;
                        }
                    }
                    else
                    {
                        // MonoBehaviour가 아닌 경우 첫 번째로 발견된 것 사용
                        closestInteractable = interactable;
                        break;
                    }
                }
            }

            // 상호작용 수행
            if (closestInteractable != null)
            {
                closestInteractable.Interact();
            }
        }
    }
}
