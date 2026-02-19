using UnityEngine;
using VContainer;

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

        private IEventBus _eventBus;
        private int _checkIndex;
        private Transform _playerTransform;
        private Transform _cullingObjectsRoot;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(OnPlayerDestroyed);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus?.Unsubscribe<PlayerObjectSpawnEvent>(OnPlayerSpawned);
            _eventBus?.Unsubscribe<PlayerObjectDestroyEvent>(OnPlayerDestroyed);
        }

        private Transform GetOrCreateCullingObjectsRoot()
        {
            if (_cullingObjectsRoot != null)
                return _cullingObjectsRoot;
                
            var go = new GameObject(CullingObjectsName);
            go.transform.SetParent(transform);
            _cullingObjectsRoot = go.transform;
            return _cullingObjectsRoot;
        }

        private void OnPlayerSpawned(PlayerObjectSpawnEvent ev)
        {
            _playerTransform = ev.player != null ? ev.player.transform : null;
        }

        private void OnPlayerDestroyed(PlayerObjectDestroyEvent ev)
        {
            _playerTransform = null;
        }

        private void OnEnemySpawned(EnemySpawnedEvent ev)
        {
            if (ev.enemy == null)
                return;
            ev.enemy.transform.SetParent(GetOrCreateCullingObjectsRoot(), true);
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

                if (shouldBeActive && !entity.gameObject.activeSelf)
                    entity.gameObject.SetActive(true);
                else if (!shouldBeActive && entity.gameObject.activeSelf)
                    entity.gameObject.SetActive(false);
            }

            _checkIndex = (_checkIndex + toCheck) % childCount;
        }
    }
}
