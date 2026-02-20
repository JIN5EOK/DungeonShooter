using System;
using System.Collections.Generic;
using VContainer;

namespace DungeonShooter
{
    public class EntityManager : IDisposable
    {
        public int RemainingEnemyCount => _enemies.Count;
        public event Action<int> OnRemainingEnemyCountChanged;

        private readonly IEventBus _eventBus;
        private readonly IPlayerLevelService _playerLevelService;
        private readonly HashSet<EntityBase> _enemies = new HashSet<EntityBase>();

        [Inject]
        public EntityManager(IEventBus eventBus, IPlayerLevelService playerLevelService)
        {
            _eventBus = eventBus;
            _playerLevelService = playerLevelService;
            _eventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus.Subscribe<EnemyDeadEvent>(OnEnemyDestroyed);
            _eventBus.Subscribe<PlayerDeadEvent>(OnPlayerDead);
        }

        private void OnEnemySpawned(EnemySpawnedEvent ev)
        {
            _enemies.Add(ev.enemy);
            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);
        }

        private void OnPlayerDead(PlayerDeadEvent ev)
        {
            LogHandler.Log<EntityManager>("게임이 끝났습니다!");
        }

        private void OnEnemyDestroyed(EnemyDeadEvent ev)
        {
            _playerLevelService?.AddExp(ev.enemyConfigTableEntry.Exp);
            _enemies.Remove(ev.enemy);
            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);

            // 적 전멸 이벤트
            if (_enemies.Count == 0)
            {
                LogHandler.Log<EntityManager>("적이 전멸했습니다!");
                _eventBus.Publish(new AllEnemiesEliminatedEvent());
            }
                
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDestroyed);
            _eventBus.Unsubscribe<PlayerDeadEvent>(OnPlayerDead);
        }
    }
}
