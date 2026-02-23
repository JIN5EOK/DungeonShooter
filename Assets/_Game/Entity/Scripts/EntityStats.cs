using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯(최대 체력 등) 수치 관련 인터페이스.
    /// </summary>
    public interface IEntityStats
    {
        public void Initialize(EntityStatsTableEntry entry);
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
        private EntityStatsTableEntry _statsTableEntry;

        /// <summary>
        /// 스탯 테이블 엔트리를 기반으로 기본 스탯(Constant)을 설정합니다.
        /// </summary>
        public void Initialize(EntityStatsTableEntry entry)
        {
            if (entry == null) return;

            _statsTableEntry = entry;
            _stats.Clear();

            GetOrAddStat(StatType.Hp).AddModifier(_statsTableEntry, StatModifierType.Constant, entry.MaxHp);
            GetOrAddStat(StatType.Attack).AddModifier(_statsTableEntry, StatModifierType.Constant, entry.Attack);
            GetOrAddStat(StatType.Defense).AddModifier(_statsTableEntry, StatModifierType.Constant, entry.Defense);
            GetOrAddStat(StatType.MoveSpeed).AddModifier(_statsTableEntry, StatModifierType.Constant, entry.MoveSpeed);
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
