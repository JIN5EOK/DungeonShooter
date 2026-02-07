using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯을 관리하는 Pure C# 객체.
    /// EntityStatsTableEntry를 기준으로 기본 스탯(Constant)을 설정하고, GetStat으로 최종 수치를 반환한다.
    /// </summary>
    public class EntityStatGroup
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

        /// <summary>
        /// 최종 스탯 수치를 반환합니다. 해당 StatType이 없으면 0으로 생성 후 반환합니다.
        /// </summary>
        public int GetStat(StatType type)
        {
            return GetOrAddStat(type).GetValue();
        }

        /// <summary>
        /// 해당 스탯의 원본(Constant) 수치를 반환합니다. 해당 StatType이 없으면 0으로 생성 후 반환합니다.
        /// </summary>
        public int GetOriginStat(StatType type)
        {
            return GetOrAddStat(type).GetOriginValue();
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
