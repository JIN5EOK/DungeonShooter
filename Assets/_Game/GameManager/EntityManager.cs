using System;
using System.Collections.Generic;
using VContainer;

namespace DungeonShooter
{
    public class EntityManager
    {
        public int RemainingEnemyCount => _enemies.Count;
        public event Action<int> OnRemainingEnemyCountChanged;
        public event Action OnAllEnemiesEliminated;

        private IGameResultService _gameResultService;
        private IPlayerLevelService _playerLevelService;
        private HashSet<EntityBase> _enemies = new HashSet<EntityBase>();

        [Inject]
        public EntityManager(IPlayerLevelService playerLevelService, IGameResultService gameResultService)
        {
            _playerLevelService = playerLevelService;
            _gameResultService = gameResultService;
        }

        internal void OnEnemySpawned(EnemySpawnedEvent ev)
        {
            _enemies.Add(ev.enemy);
            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);
        }

        internal void OnPlayerDead(PlayerDeadEvent ev)
        {
            _gameResultService.ExecuteGameResult(GameResult.Dead);
        }

        internal void OnEnemyDestroyed(EnemyDeadEvent ev)
        {
            _playerLevelService?.AddExp(ev.enemyConfigTableEntry.Exp);
            _enemies.Remove(ev.enemy);
            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);

            // 적 전멸 이벤트
            if (_enemies.Count == 0)
            {
                LogHandler.Log<EntityManager>("적이 전멸했습니다!");
                OnAllEnemiesEliminated?.Invoke();
            }
                
        }
    }
}
