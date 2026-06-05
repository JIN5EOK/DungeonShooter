using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어와의 거리에 따라 오브젝트를 활성/비활성화하는 매니저.
    /// CullingObjects 루트의 자식을 순회해 적용한다. 풀 릴리즈 시 부모가 바뀌면 자동으로 관리 대상에서 제외된다.
    /// </summary>
    public class ObjectCullingManager : MonoBehaviour
    {
        private const string CullingObjectsName = "CullingObjects";
        /// <summary>활성화 거리</summary>
        private readonly float _activationDistance = 20f;
        /// <summary>비활성화 거리</summary>
        private readonly float _deactivationDistance = 25f;

        /// <summary>프레임마다 체크할 대상 갯수</summary>
        private readonly int _checksPerFrame = 8;

        private int _checkIndex;
        private Transform _playerTransform;
        private Transform _cullingObjectsRoot;

        private Transform GetOrCreateCullingObjectsRoot()
        {
            if (_cullingObjectsRoot != null)
                return _cullingObjectsRoot;
                
            var go = new GameObject(CullingObjectsName);
            go.transform.SetParent(transform);
            _cullingObjectsRoot = go.transform;
            return _cullingObjectsRoot;
        }

        /// <summary>거리 컬링 기준으로 삼을 플레이어 엔티티를 설정합니다.</summary>
        public void SetPlayerDistanceReference(EntityBase player)
        {
            _playerTransform = player != null ? player.transform : null;
        }

        /// <summary>플레이어 거리 기준을 해제합니다.</summary>
        public void ClearPlayerDistanceReference()
        {
            _playerTransform = null;
        }

        /// <summary>엔티티를 컬링 루트 하위로 붙여 거리 기반 활성/비활성 대상으로 둡니다.</summary>
        public void AttachEntityToDistanceCullingRoot(EntityBase entity)
        {
            if (entity == null)
                return;

            entity.transform.SetParent(GetOrCreateCullingObjectsRoot(), true);
        }

        private void Update()
        {
            if (_playerTransform == null || _cullingObjectsRoot == null)
                return;

            var childCount = _cullingObjectsRoot.childCount;
            if (childCount == 0)
            {
                _checkIndex = 0;
                return;
            }
            if (_checkIndex >= childCount)
                _checkIndex = 0;

            var playerPos = (Vector2)_playerTransform.position;
            var toCheck = Mathf.Min(_checksPerFrame, childCount);

            for (var i = 0; i < toCheck; i++)
            {
                var idx = (_checkIndex + i) % childCount;
                var child = _cullingObjectsRoot.GetChild(idx);
                if (!child.TryGetComponent<EntityBase>(out var entity))
                    continue;

                var sqrDist = ((Vector2)entity.transform.position - playerPos).sqrMagnitude;
                var shouldBeActive = sqrDist <= _activationDistance * _activationDistance;
                var shouldBeDeactive = sqrDist >= _deactivationDistance * _deactivationDistance;

                if (shouldBeActive && !entity.gameObject.activeSelf)
                    entity.gameObject.SetActive(true);
                else if (shouldBeDeactive && entity.gameObject.activeSelf)
                    entity.gameObject.SetActive(false);
            }

            _checkIndex = (_checkIndex + toCheck) % childCount;
        }
    }
}
