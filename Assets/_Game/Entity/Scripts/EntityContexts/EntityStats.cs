using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯(최대 체력 등) 수치 관련 인터페이스.
    /// </summary>
    public interface IEntityStats
    {
        public IEntityStat GetStat(StatType type);
        public void ApplyStatBonus(object key, StatBonus bonus);
        public void RemoveStatBonus(object key);
    }
    
    /// <summary>
    /// Entity의 스탯을 관리하는 Pure C# 객체.
    /// </summary>
    public class EntityStats : IEntityStats
    {
        private readonly Dictionary<StatType, EntityStat> _stats = new Dictionary<StatType, EntityStat>();
        private object _baseStatsKey;

        public void Initialize(StatsDto statsDto)
        {
            if (statsDto == null) return;

            _baseStatsKey = statsDto;
            _stats.Clear();

            GetOrAddStat(StatType.Hp).AddModifier(_baseStatsKey, StatModifierType.Constant, statsDto.MaxHp);
            GetOrAddStat(StatType.Attack).AddModifier(_baseStatsKey, StatModifierType.Constant, statsDto.Attack);
            GetOrAddStat(StatType.Defense).AddModifier(_baseStatsKey, StatModifierType.Constant, statsDto.Defense);
            GetOrAddStat(StatType.MoveSpeed).AddModifier(_baseStatsKey, StatModifierType.Constant, statsDto.MoveSpeed);
        }

        public IEntityStat GetStat(StatType type)
        {
            return GetOrAddStat(type);
        }

        /// <summary>
        /// StatBonus를 source 키로 적용합니다.
        /// </summary>
        public void ApplyStatBonus(object key, StatBonus bonus)
        {
            if (key == null) return;

            ApplyStatBonusInternal(key, StatType.Hp, bonus.HpAdd, bonus.HpMultiply);
            ApplyStatBonusInternal(key, StatType.Attack, bonus.AttackAdd, bonus.AttackMultiply);
            ApplyStatBonusInternal(key, StatType.Defense, bonus.DefenseAdd, bonus.DefenseMultiply);
            ApplyStatBonusInternal(key, StatType.MoveSpeed, bonus.MoveSpeedAdd, bonus.MoveSpeedMultiply);
        }

        /// <summary>
        /// 해당 source로 등록된 modifier를 모든 스탯에서 제거합니다.
        /// </summary>
        public void RemoveStatBonus(object key)
        {
            foreach (var stat in _stats.Values)
            {
                stat.RemoveModifier(key);
            }
        }

        private EntityStat GetOrAddStat(StatType type)
        {
            if (_stats.TryGetValue(type, out var stat))
            {
                return stat;
            }

            var entityStat = new EntityStat();
            _stats[type] = entityStat;

            return entityStat;
        }

        private void ApplyStatBonusInternal(object source, StatType type, int add, int multiply)
        {
            var stat = GetOrAddStat(type);
            if (add != 0)
            {
                stat.AddModifier(source, StatModifierType.Add, add);
            }

            if (multiply != 100 && multiply != 0)
            {
                stat.AddModifier(source, StatModifierType.Multiply, multiply);
            }
        }
    }
}
