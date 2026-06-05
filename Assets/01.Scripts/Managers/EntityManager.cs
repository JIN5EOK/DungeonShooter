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

        private IPlayerLevelService _playerLevelService;
        private HashSet<EntityBase> _enemies = new HashSet<EntityBase>();

        [Inject]
        public EntityManager(IPlayerLevelService playerLevelService)
        {
            _playerLevelService = playerLevelService;
        }

        public void RegisterSpawnedEnemy(EntityBase enemy)
        {
            if (enemy == null)
                return;

            _enemies.Add(enemy);
            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);
        }


        public void NotifyEnemyDefeated(EntityBase enemy, int experienceReward)
        {
            if (experienceReward > 0)
                _playerLevelService?.AddExp(experienceReward);

            if (enemy != null)
                _enemies.Remove(enemy);

            OnRemainingEnemyCountChanged?.Invoke(_enemies.Count);

            if (_enemies.Count == 0)
            {
                LogHandler.Log<EntityManager>("적이 전멸했습니다!");
                OnAllEnemiesEliminated?.Invoke();
            }
        }
    }
}
