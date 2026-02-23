using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// HP를 가진 엔티티에 부착하는 독립적인 컴포넌트.
    /// 현재 체력은 EntityContext.Status(EntityStatus)를 참조하며, 자체 보관하지 않는다.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        private int MaxHealth =>
            _entityBase?.EntityContext?.Stat != null ? _entityBase.EntityContext.Stat.GetStat(StatType.Hp).GetValue() : 0;

        public int CurrentHealth => _hpStatus?.GetValue() ?? 0;
        public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsDead => CurrentHealth <= 0;

        private Color hitColor = Color.red;
        private float hitFlashDuration = 0.2f;
        private Color deathColor = Color.gray;
        private Color _originalColor;
        private EntityBase _entityBase;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private IEntityStatusValue _hpStatus;

        [Inject]
        private void Construct(EntityBase entityBase, SpriteRenderer spriteRenderer, Rigidbody2D rigidbody2D)
        {
            _entityBase = entityBase;
            _spriteRenderer = spriteRenderer;
            _rigidbody = rigidbody2D;
            _originalColor = _spriteRenderer.color;

            _hpStatus = _entityBase.EntityContext?.Status?.GetStatus(StatusType.Hp);
            if (_hpStatus != null)
                _hpStatus.OnValueChanged += OnHpStatusChanged;
        }

        private void OnHpStatusChanged(int value)
        {
            OnHealthChanged?.Invoke(value);
            if (value <= 0)
                Die();
        }

        private void OnDestroy()
        {
            if (_hpStatus != null)
                _hpStatus.OnValueChanged -= OnHpStatusChanged;
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            
            if (damage < 0)
                damage = 0;
            
            if (damage > 0)
                StartCoroutine(HitFlashEffect());
            
            var newValue = Mathf.Max(0, CurrentHealth - damage);
            _hpStatus?.SetValue(newValue);
        }

        /// <summary>
        /// 체력을 회복합니다
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead) return;
            if (amount < 0) amount = 0;
            _hpStatus?.SetValue(Mathf.Min(CurrentHealth + amount, MaxHealth));
        }

        /// <summary>
        /// 즉시 사망 처리
        /// </summary>
        public void Kill()
        {
            if (IsDead) return;
            _hpStatus?.SetValue(0);
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }

        /// <summary>
        /// 체력을 완전 회복합니다
        /// </summary>
        public void FullHeal()
        {
            _hpStatus?.SetValue(MaxHealth);
        }

        /// <summary>
        /// 현재 체력을 설정합니다.
        /// </summary>
        public void SetCurrentHealth(int value)
        {
            _hpStatus?.SetValue(Mathf.Clamp(value, 0, MaxHealth));
        }

        public void ResetState()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }

            FullHeal();
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
