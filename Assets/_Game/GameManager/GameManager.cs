using System;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class GameResultData
    {
        public bool isClear { get; set; }
        public int killCount { get; set; }
    }
    
    public class GameManager : IDisposable 
    {
        private GameResultData _gameResult = new ();
        private IEventBus _eventBus;
        private UIManager _uiManager;
        [Inject]
        public GameManager(IEventBus eventBus, UIManager uiManager)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<EnemyDeadEvent>(OnEnemyDestroyed);
            _eventBus.Subscribe<PlayerDeadEvent>(OnPlayerDead);
            
            _uiManager = uiManager;
        }
        
        private void OnPlayerDead(PlayerDeadEvent ev)
        {
            LogHandler.Log<GameManager>("게임이 끝났습니다!");
        }
        
        private void OnEnemyDestroyed(EnemyDeadEvent ev)
        {
            _gameResult.killCount++;            
            _eventBus.Publish(new ExpUpEvent {exp = ev.enemyConfigTableEntry.Exp});
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDestroyed);
        }
    }
}