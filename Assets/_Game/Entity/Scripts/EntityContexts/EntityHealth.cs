using System;
using UnityEngine;

namespace DungeonShooter
{

    public interface IHealth
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public bool IsDead { get; }
        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        public void TakeDamage(int damage);
        public void Heal(int amount);
        public void SetCurrentHealth(int value);
        public void ResetState();
    }

    public class EntityHealth : IHealth
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = value;
                OnHealthChanged?.Invoke(value);
                if (value <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        private int _currentHealth;
        public int MaxHealth => _maxHpStat?.GetValue() ?? 0;
        public bool IsDead => CurrentHealth <= 0;

        private readonly IEntityContext _context;
        private readonly IEntityStat _maxHpStat;

        public EntityHealth(IEntityStat maxHpStat)
        {
            _maxHpStat = maxHpStat;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) 
                return;
            if (damage < 0) 
                damage = 0;

            var newValue = Mathf.Max(0, CurrentHealth - damage);
            CurrentHealth = newValue;
        }

        public void Heal(int amount)
        {
            if (IsDead) 
                return;
            if (amount < 0) 
                amount = 0;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }
        public void SetCurrentHealth(int value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, MaxHealth); 
        }

        public void ResetState()
        {
            SetCurrentHealth(MaxHealth);
        }
    }
}