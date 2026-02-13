using System;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class BattleManager : IStartable, IDisposable 
    {
        private IEventBus _eventBus;
        
        [Inject]
        public BattleManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Start()
        {
            _eventBus.Subscribe<EnemyDestroyEvent>(EnemyDestroyed);
        }
        
        private void EnemyDestroyed(EnemyDestroyEvent ev)
        {
            _eventBus.Publish(new ExpUpEvent {exp = ev.enemyConfigTableEntry.Exp});
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<EnemyDestroyEvent>(EnemyDestroyed);
        }
    }
}