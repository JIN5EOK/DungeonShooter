using System;
using UnityEngine;

namespace DungeonShooter
{
    public interface IEntity : IHealth, IEntityStats
    {
    }
    public class EntityBase : MonoBehaviour, IEntity
    {
        public event Action<EntityBase> OnDestroyed;
        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public bool IsDead => EntityContext.HealthModel.IsDead;
        public int CurrentHealth => EntityContext.HealthModel.CurrentHealth;
        public int MaxHealth => EntityContext.HealthModel.MaxHealth;

        public IEntityContext EntityContext => _entityContext;
        private IEntityContext _entityContext;
        
        public void TakeDamage(int damage) => EntityContext.HealthModel.TakeDamage(damage);
        public void Heal(int amount) => EntityContext.HealthModel.Heal(amount);
        public void SetCurrentHealth(int value)  => EntityContext.HealthModel.SetCurrentHealth(value);
        public IEntityStat GetStat(StatType type) => _entityContext.Stat.GetStat(type);

        public void ApplyStatBonus(object key, StatBonus bonus) => _entityContext.Stat.ApplyStatBonus(key, bonus);

        public void RemoveStatBonus(object key) => _entityContext.Stat.RemoveStatBonus(key);
        
        /// <summary>
        /// 엔티티를 해제 혹은 제거합니다. PoolableComponent가 있으면 풀에 반환하고, 없으면 게임오브젝트를 파괴합니다.
        /// </summary>
        public void Release()
        {
            var poolable = GetComponent<PoolableComponent>();
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 팩토리에서 생성한 EntityContext를 주입합니다.
        /// </summary>
        public void SetContext(IEntityContext context)
        {
            if (_entityContext?.Skill != null)
            {
                foreach (var s in _entityContext.Skill.GetSkills())
                {
                    UnapplySkill(s);
                }

                _entityContext.Skill.OnSkillRegisted -= ApplySkill;
                _entityContext.Skill.OnSkillUnregisted -= UnapplySkill;
                _entityContext.HealthModel.OnHealthChanged -= OnHealthChanged;
                _entityContext.HealthModel.OnDeath -= OnDeath;
            }

            _entityContext = context;

            if (context?.Skill != null)
            {
                context.Skill.OnSkillRegisted += ApplySkill;
                context.Skill.OnSkillUnregisted += UnapplySkill;
                _entityContext.HealthModel.OnHealthChanged += OnHealthChanged;
                _entityContext.HealthModel.OnDeath += OnDeath;
                foreach (var s in context.Skill.GetSkills())
                {
                    ApplySkill(s);
                }
            }
        }

        private void ApplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Activate(this);
            }
        }
        
        private void UnapplySkill(Skill skill)
        {
            if (skill?.SkillData != null && skill.SkillData.IsPassiveSkill)
            {
                skill.Deactivate(this);
            }
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }
    }

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
