using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class EnemySpawner : MonoBehaviour
    {
        private IStageTimer _stageTimer;
        private IEnemyFactory _enemyFactory;

        private List<EnemySpawnInfo> _sorted;
        private int _nextIndex;
        private SpawnRule _currentRule;
        private int _activeCount;
        private Transform _playerTransform;
        private float _spawnTimer;
        private bool _isSpawning;

        [Inject]
        public void Construct(IStageTimer stageTimer, IEnemyFactory enemyFactory)
        {
            _stageTimer = stageTimer;
            _enemyFactory = enemyFactory;
        }

        public void StartSpawning(IReadOnlyList<EnemySpawnInfo> spawnInfos, Transform playerTransform)
        {
            _sorted = spawnInfos.OrderBy(s => s.TriggerTime).ToList();
            _playerTransform = playerTransform;
            _nextIndex = 0;
            _spawnTimer = 0f;
            _isSpawning = true;
        }

        public void StopSpawning() => _isSpawning = false;

        private void Update()
        {
            if (!_isSpawning) return;

            UpdateScheduler();
            UpdateSpawnLoop();
        }

        private void UpdateScheduler()
        {
            if (_sorted == null) return;

            while (_nextIndex < _sorted.Count && _stageTimer.Elapsed >= _sorted[_nextIndex].TriggerTime)
            {
                _currentRule = _sorted[_nextIndex].Rule;
                _nextIndex++;
                _spawnTimer = 0f;
            }
        }

        private void UpdateSpawnLoop()
        {
            if (_currentRule == null || _activeCount >= _currentRule.MaxActiveCount) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            var origin = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            var enemy = _currentRule.Spawn(origin, _enemyFactory);
            if (enemy != null)
            {
                _activeCount++;
                enemy.GetContext().HealthModel.OnDeath += () => _activeCount--;
            }

            _spawnTimer = _currentRule.SpawnInterval;
        }

        private void OnDestroy() => StopSpawning();
    }
}
