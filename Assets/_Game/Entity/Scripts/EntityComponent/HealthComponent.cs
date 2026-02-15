using System;
using System.Collections;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// HP를 가진 엔티티에 부착하는 독립적인 컴포넌트
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        private int MaxHealth
        {
            get => _entityBase != null && _entityBase.StatContainer != null ? _entityBase.StatContainer.GetStat(StatType.Hp).GetValue() : 0;
        }

        public int CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = value;
                OnHealthChanged?.Invoke(value);
            }
        }
        private int _currentHealth;
        public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsDead => CurrentHealth <= 0;
        private Color hitColor = Color.red;
        private float hitFlashDuration = 0.2f;
        private Color deathColor = Color.gray;
        private Color _originalColor;
        private EntityBase _entityBase;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;

        [Inject]
        private void Construct(EntityBase entityBase, SpriteRenderer spriteRenderer, Rigidbody2D rigidbody2D)
        {
            _entityBase = entityBase;
            _spriteRenderer = spriteRenderer;
            _rigidbody = rigidbody2D;
            CurrentHealth = MaxHealth;
            _originalColor = _spriteRenderer.color;
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

            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
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
        }

        /// <summary>
        /// 체력을 완전 회복합니다
        /// </summary>
        public void FullHeal()
        {
            Heal(MaxHealth);
        }

        /// <summary>
        /// 현재 체력을 설정합니다.
        /// </summary>
        public void SetCurrentHealth(int value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }

        /// <summary>
        /// 피격 시 색상 변경 효과
        /// </summary>
        private IEnumerator HitFlashEffect()
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
