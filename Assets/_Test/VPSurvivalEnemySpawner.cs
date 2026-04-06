using System;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    public class VPSurvivalEnemySpawner : MonoBehaviour
    {
        private float _timer;
        private EntityBase _player;
        private IEnemyFactory _enemyFactory;
        [Inject]
        public void Construct(IEventBus eventBus, IEnemyFactory enemyFactory)
        {
            eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerSpawned);
            _enemyFactory = enemyFactory;
        }

        private async void OnPlayerSpawned(PlayerObjectSpawnEvent ev)
        {
            _player = ev.player;

            var radius = 5;
            
            
            for (int i = 0; i < 300; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2);
                
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                await _enemyFactory.GetEnemyByConfigIdAsync(18000000, _player.transform.position + new Vector3(x, y, 1));
            }
        }
    }
}