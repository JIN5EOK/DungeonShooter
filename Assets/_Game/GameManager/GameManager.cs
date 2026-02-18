using System;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class GameManager : IDisposable 
    {
        private IEventBus _eventBus;
        private IPlayerLevelService _playerLevelService;
        [Inject]
        public GameManager(IEventBus eventBus, IPlayerLevelService playerLevelService)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<EnemyDeadEvent>(OnEnemyDestroyed);
            _eventBus.Subscribe<PlayerDeadEvent>(OnPlayerDead);
            _playerLevelService = playerLevelService;
        }
        
        private void OnPlayerDead(PlayerDeadEvent ev)
        {
            LogHandler.Log<GameManager>("게임이 끝났습니다!");
        }
        
        private void OnEnemyDestroyed(EnemyDeadEvent ev)
        {         
            _playerLevelService?.AddExp(ev.enemyConfigTableEntry.Exp);
        }
        
        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDestroyed);
        }
    }
}