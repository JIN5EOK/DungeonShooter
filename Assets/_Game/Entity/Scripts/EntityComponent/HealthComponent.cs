using UnityEngine;
using System;
using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// HP를 가진 엔티티에 부착하는 독립적인 컴포넌트
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action<int, int> OnDamaged; // (damage, currentHealth)
        public event Action<int, int> OnHealed; // (amount, currentHealth)
        public event Action OnDeath;

        public int MaxHealth
        {
            get => statsComponent.Stats.MaxHealth;
        }
        public int CurrentHealth { get; private set; }
        public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsDead => CurrentHealth <= 0;
        private Color hitColor = Color.red;
        private float hitFlashDuration = 0.2f;
        private Color deathColor = Color.gray;
        private EntityStatsComponent statsComponent;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        private void Awake()
        {
            statsComponent = gameObject.AddOrGetComponent<EntityStatsComponent>();
            CurrentHealth = MaxHealth;
            _spriteRenderer = gameObject.AddOrGetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _rigidbody = gameObject.AddOrGetComponent<Rigidbody2D>();
            _collider = gameObject.AddOrGetComponent<Collider2D>();
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            if (damage < 0) damage = 0;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);

            OnDamaged?.Invoke(damage, CurrentHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            // 피격 이펙트 실행
            if (damage > 0)
            {
                StartCoroutine(HitFlashEffect());
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 체력을 회복합니다
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead) return;
            if (amount < 0) amount = 0;

            var oldHealth = CurrentHealth;
            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

            var actualHealed = CurrentHealth - oldHealth;
            if (actualHealed > 0)
            {
                OnHealed?.Invoke(actualHealed, CurrentHealth);
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        /// <summary>
        /// 즉시 사망 처리
        /// </summary>
        public void Kill()
        {
            if (IsDead) return;
            
            CurrentHealth = 0;
            Die();
        }

        private void Die()
        {
            OnDeath?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            // 사망 효과 실행
            ApplyDeathEffects();
        }

        /// <summary>
        /// 사망 시 시각적 효과 적용
        /// </summary>
        private void ApplyDeathEffects()
        {
            // 색상 변경
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = deathColor;
            }

            // Rigidbody 정지
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            // Collider 비활성화
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        /// <summary>
        /// 체력을 완전 회복합니다
        /// </summary>
        public void FullHeal()
        {
            Heal(MaxHealth);
        }

        /// <summary>
        /// 피격 시 색상 변경 효과
        /// </summary>
        private System.Collections.IEnumerator HitFlashEffect()
        {
            if (_spriteRenderer == null) yield break;

            // 빨간색으로 변경
            _spriteRenderer.color = hitColor;

            yield return new WaitForSeconds(hitFlashDuration);

            // 원래 색상으로 복구
            _spriteRenderer.color = _originalColor;
        }
    }
}
