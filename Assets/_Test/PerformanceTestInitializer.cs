using System;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    public class PerformanceTestInitializer : MonoBehaviour
    {
        private float _timer;
        private EntityBase _player;
        private IEnemyFactory _enemyFactory;
        private IPlayerFactory _playerFactory;
        
        [Inject]
        public void Construct(IPlayerContextManager playerContextManager, IPlayerFactory playerFactory, IEventBus eventBus, IEnemyFactory enemyFactory)
        {
            playerContextManager.Initialize(12000001);
            _playerFactory = playerFactory;    
            eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerSpawned);
            _enemyFactory = enemyFactory;
        }

        public void Start()
        {
            _playerFactory.GetPlayerAsync(12000001, Vector2.zero);
            
            OnPlayerSpawned(new PlayerObjectSpawnEvent());
        }
        
        private async void OnPlayerSpawned(PlayerObjectSpawnEvent ev)
        {
            _player = ev.player;

            var radius = 5;
            
            
            for (int i = 0; i < 50; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2);
                
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                await _enemyFactory.GetEnemyByConfigIdAsync(18000000, _player.transform.position + new Vector3(x, y, 1));
            }
        }
    }
}