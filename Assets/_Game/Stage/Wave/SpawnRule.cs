using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    [Serializable]
    public class SpawnRule
    {
        [Serializable]
        public struct WeightedEnemy
        {
            public EnemyConfigSo Config;
            public int Weight;
        }

        [field: SerializeField] public float SpawnInterval { get; private set; } = 3f;
        [field: SerializeField] public int MaxActiveCount { get; private set; } = 20;
        [SerializeField] private List<WeightedEnemy> _enemies;
        private float _spawnRadius = 10f;
        public EntityBase Spawn(Vector3 origin, IEnemyFactory factory)
        {
            if (_enemies == null || _enemies.Count == 0) return null;

            var totalWeight = _enemies.Sum(e => e.Weight);
            if (totalWeight <= 0) return null;

            var rand = Random.Range(0, totalWeight);
            var accumulated = 0;

            foreach (var entry in _enemies)
            {
                accumulated += entry.Weight;
                if (rand < accumulated)
                    return factory.GetEnemyByConfigSync(entry.Config, GetSpawnPosition(origin));
            }

            return null;
        }

        private Vector3 GetSpawnPosition(Vector3 origin)
        {
            var randomDir = Random.insideUnitCircle.normalized;
            return origin + new Vector3(randomDir.x, randomDir.y) * _spawnRadius;
        }
    }
}
