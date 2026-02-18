using System;
using VContainer;

namespace DungeonShooter
{
    public class PlayerLevelManager : IDisposable
    {
        public Action<int> OnLevelChanged;
        public Action<int> OnExpChanged;
        public Action<int> OnMaxExpChanged;
        
        public int Level { get; private set; } = 1;
        public int Exp {get; private set; }
        private int _maxExp = 100;
        public int MaxExp
        {
            get => _maxExp;
            private set 
            {
                _maxExp = value;
                OnMaxExpChanged?.Invoke(_maxExp);
            } 
        }
        
        private IEventBus _eventBus;

        [Inject]
        public PlayerLevelManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ExpUpEvent>(ExpUpped);
        }
        
        private void ExpUpped(ExpUpEvent ev)
        {
            AddExp(ev.exp);
        }
        
        public void AddExp(int amount)
        {
            Exp += amount;
            while (Exp >= MaxExp)
            {
                Exp -= MaxExp;
                Level++;
                
                // 레벨업 이벤트 발행
                _eventBus.Publish(new PlayerLevelChangeEvent() {level = Level});
                OnLevelChanged?.Invoke(Level);
                MaxExp = (int)(MaxExp * 1.25f);
            }
            OnExpChanged?.Invoke(Exp);
        }
        
        public void Dispose()
        {
            _eventBus.Unsubscribe<ExpUpEvent>(ExpUpped);
        }
    }
}