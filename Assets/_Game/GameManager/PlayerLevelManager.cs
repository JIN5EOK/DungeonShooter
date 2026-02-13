using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class PlayerLevelManager : IStartable, IDisposable
    {
        public Action<int> OnLevelChanged;
        public Action<int> OnExpChanged;
        public Action<int> OnMaxExpChanged;
        
        public int Level { get; private set; } = 1;
        public int Exp {get; private set; }
        public int MaxExp => 100; // 일단 경험치통은 레벨 상관없이 100으로 고정
        
        private IEventBus _eventBus;
        
        [Inject]
        public PlayerLevelManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        public void Start()
        {
            _eventBus.Subscribe<ExpUpEvent>(ExpUped);
        }
        
        private void ExpUped(ExpUpEvent ev)
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
            }
            OnExpChanged?.Invoke(Exp);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<ExpUpEvent>(ExpUped);
        }
    }
}