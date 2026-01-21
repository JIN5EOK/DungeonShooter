using System;
using System.Collections.Generic;
using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 상호작용 기능을 담당하는 MonoBehaviour 컴포넌트
    /// </summary>
    public class InteractComponent : MonoBehaviour
    {
        [Header("감지할 트리거")]
        [SerializeField]
        private TriggerDetector2D _triggerDetector2D;
        
        private HashSet<IInteractable> _nearbyInteractables = new HashSet<IInteractable>();

        private void Start()
        {
            // 상호작용 객체만 찾도록
            _triggerDetector2D.TargetType = typeof(IInteractable);
            _triggerDetector2D.OnTargetEntered += RegisterInteractable;
            _triggerDetector2D.OnTargetExited += UnregisterInteractable;
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트를 등록합니다.
        /// </summary>
        private void RegisterInteractable(Component component)
        {
            var interactable = component as IInteractable;
            if (interactable == null)
            {
                return;
            }
            Debug.Log($"[{nameof(InteractComponent)}] Registering interactable {interactable})]");
            _nearbyInteractables.Add(interactable);
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트를 제거합니다.
        /// </summary>
        private void UnregisterInteractable(Component component)
        {
            var interactable = component as IInteractable;
            if (interactable == null)
            {
                return;
            }
            Debug.Log($"[{nameof(InteractComponent)}] UnRegistering interactable {interactable})]");
            _nearbyInteractables.Remove(interactable);
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
