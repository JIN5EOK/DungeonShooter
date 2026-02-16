using System;
using VContainer;

namespace DungeonShooter
{
    public class GameManager : IDisposable 
    {
        private IEventBus _eventBus;
        
        [Inject]
        public GameManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<EnemyDeadEvent>(EnemyDestroyed);
        }

        private void PlayerDestroyed(PlayerObjectDestroyEvent ev)
        {
            
        }
        
        private void EnemyDestroyed(EnemyDeadEvent ev)
        {
            _eventBus.Publish(new ExpUpEvent {exp = ev.enemyConfigTableEntry.Exp});
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDeadEvent>(EnemyDestroyed);
        }
    }
}