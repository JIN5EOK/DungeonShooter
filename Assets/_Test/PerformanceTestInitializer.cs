using System;
using Cysharp.Threading.Tasks;
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
        private IPlayerContextManager _playerContextManager;
        private IItemFactory _itemFactory;
        private IInventory _inventory;
        [Inject]
        public void Construct(IPlayerContextManager playerContextManager, IPlayerFactory playerFactory, IEventBus eventBus, IEnemyFactory enemyFactory, IItemFactory itemFactory,  IInventory inventory)
        {
            _itemFactory = itemFactory;
            _inventory = inventory;
            _playerContextManager = playerContextManager;
            playerContextManager.Initialize(12000001);
            _playerFactory = playerFactory;    
            eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerSpawned);
            _enemyFactory = enemyFactory;
        }

        public async void Start()
        {
            await _playerFactory.GetPlayerAsync(12000001, Vector2.zero);
            await _playerContextManager.InitializeSkillsAsync();
            
            var weapon = await _itemFactory.CreateItemAsync(15000001);
            _inventory.AddItem(weapon);
            _inventory.EquipItem(weapon);
            await SpawnEnemy();
        }
        
        private void OnPlayerSpawned(PlayerObjectSpawnEvent ev)
        {
            _player = ev.player;
            
            

             
        }

        private async UniTask SpawnEnemy()
        {
            var radius = 10;
            
            
            for (int i = 0; i < 300; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2);
                
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                await _enemyFactory.GetEnemyByConfigIdAsync(18000002, _player.transform.position + new Vector3(x, y, 1));
            }
        }
    }
}