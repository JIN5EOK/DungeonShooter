using System.Collections.Generic;
using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 상호작용 기능을 담당하는 MonoBehaviour 컴포넌트
    /// </summary>
    public class InteractComponent : MonoBehaviour, IInteractComponent
    {
        private const float NoticeOffsetY = 1.2f;

        [Header("감지할 트리거")]
        [SerializeField]
        private TriggerDetector2D _triggerDetector2D;
        private HashSet<IInteractable> _nearbyInteractables = new HashSet<IInteractable>();
        private Transform _currentTarget;
        private GameObject _interactNotice;

        /// <summary>
        /// 상호작용 노티스 게임오브젝트를 설정합니다. (플레이어 팩토리 등에서 생성해 주입)
        /// </summary>
        public void SetInteractNotice(GameObject notice)
        {
            _interactNotice = notice;
            if (_interactNotice != null)
                _interactNotice.SetActive(false);
        }

        private void Start()
        {
            if (_triggerDetector2D == null)
            {
                var triggerGo = new GameObject(nameof(TriggerDetector2D));
                var col = triggerGo.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 2f;
                _triggerDetector2D = triggerGo.AddComponent<TriggerDetector2D>();
                triggerGo.transform.SetParent(transform);
                triggerGo.transform.localPosition = Vector3.zero;
                triggerGo.transform.localRotation = Quaternion.identity;
                triggerGo.transform.localScale = Vector3.one;
            }
            _triggerDetector2D.TargetType = typeof(IInteractable);
            _triggerDetector2D.OnTargetEntered += RegisterInteractable;
            _triggerDetector2D.OnTargetExited += UnregisterInteractable;
        }

        /// <summary>
        /// 타겟이 없으면 컬렉션에서 첫 번째 유효 대상을 타겟으로 정한 뒤, 노티스 표시/위치를 갱신합니다.
        /// </summary>
        private void RefreshTarget()
        {
            if (_interactNotice == null)
                return;
            if (_currentTarget == null)
            {
                foreach (var interactable in _nearbyInteractables)
                {
                    if (interactable == null || !interactable.CanInteract)
                        continue;
                    if (interactable is MonoBehaviour mb)
                    {
                        _currentTarget = mb.transform;
                        break;
                    }
                }
            }
            if (_currentTarget == null)
            {
                _interactNotice.SetActive(false);
                return;
            }
            _interactNotice.SetActive(true);
            _interactNotice.transform.position = _currentTarget.position + Vector3.up * NoticeOffsetY;
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트를 등록합니다. 새로 들어온 대상을 타겟으로 설정합니다.
        /// </summary>
        private void RegisterInteractable(Component component)
        {
            var interactable = component as IInteractable;
            if (interactable == null)
                return;
            _nearbyInteractables.Add(interactable);
            _currentTarget = component.transform;
            RefreshTarget();
        }

        /// <summary>
        /// 상호작용 가능한 오브젝트를 제거합니다. 타겟이 빠져나가면 컬렉션에서 첫 번째를 타겟으로 설정합니다.
        /// </summary>
        private void UnregisterInteractable(Component component)
        {
            var interactable = component as IInteractable;
            if (interactable == null)
                return;
            var wasCurrent = _currentTarget == component.transform;
            _nearbyInteractables.Remove(interactable);
            if (wasCurrent)
                _currentTarget = null;
            RefreshTarget();
        }

        /// <summary>
        /// 상호작용을 시도합니다. 현재 타겟과 상호작용합니다.
        /// </summary>
        public void TryInteract()
        {
            RefreshTarget();
            if (_currentTarget == null)
                return;
            if (_currentTarget.TryGetComponent<IInteractable>(out var interactable))
                interactable.Interact();
        }
    }
}
