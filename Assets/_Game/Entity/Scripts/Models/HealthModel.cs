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
    }

    public class HealthModel : IHealth
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public int CurrentHealth => _hpStatus?.GetValue() ?? 0;
        public int MaxHealth => _maxHpStat?.GetValue() ?? 0;
        public bool IsDead => CurrentHealth <= 0;

        private readonly IEntityContext _context;
        private readonly IEntityStat _maxHpStat;
        private readonly IEntityStatus _hpStatus;

        public HealthModel(IEntityStat maxHpStat, IEntityStatus hpStatus)
        {
            _maxHpStat = maxHpStat;
            _hpStatus = hpStatus;
            if (_hpStatus != null)
            {
                _hpStatus.OnValueChanged += OnHpStatusChanged;
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            if (damage < 0) damage = 0;

            var newValue = Mathf.Max(0, CurrentHealth - damage);
            _hpStatus?.SetValue(newValue);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            _hpStatus?.SetValue(Mathf.Min(CurrentHealth + amount, MaxHealth));
            if (amount < 0) amount = 0;
        }
        public void SetCurrentHealth(int value)
        {
            _hpStatus?.SetValue(Mathf.Clamp(value, 0, MaxHealth));
        }

        public void ResetState()
        {
            SetCurrentHealth(MaxHealth);
        }

        private void OnHpStatusChanged(int value)
        {
            OnHealthChanged?.Invoke(value);
            if (value <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}