using System;

namespace DungeonShooter
{
    /// <summary>
    /// 엔티티의 체력(HP)을 담당하는 컴포넌트 인터페이스.
    /// </summary>
    public interface IHealthComponent
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public int CurrentHealth { get; }
        public float HealthPercent { get; }
        public bool IsDead { get; }
        public void TakeDamage(int damage);
        public void Heal(int amount);
        public void Kill();
        public void FullHeal();
        public void SetCurrentHealth(int value);
        public void ResetState();
    }
}
