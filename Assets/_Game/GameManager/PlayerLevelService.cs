using System;
using VContainer;

namespace DungeonShooter
{
    public interface IPlayerLevelService
    {
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public event Action<int> OnMaxExpChanged;
        public int Level { get; }
        public int Exp { get; }
        public int MaxExp { get; }
        public void AddExp(int exp);
    }
    
    public class PlayerLevelService : IPlayerLevelService
    {
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public event Action<int> OnMaxExpChanged;
        
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
        public PlayerLevelService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        public void AddExp(int amount)
        {
            Exp += amount;
            while (Exp >= MaxExp)
            {
                Exp -= MaxExp;
                Level++;
                
                // 레벨업 이벤트 발행
                OnLevelChanged?.Invoke(Level);
                MaxExp = (int)(MaxExp * 1.25f);
                
                _eventBus.Publish(new PlayerLevelChangeEvent() {level = Level});
            }
            OnExpChanged?.Invoke(Exp);
        }
    }
}