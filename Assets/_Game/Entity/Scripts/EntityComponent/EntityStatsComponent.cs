using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// Entity의 스탯 컴포넌트.
    /// EntityStatsTableEntry를 기준으로 기본 스탯(Constant)을 설정하고, GetStat으로 최종 수치를 반환한다.
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour
    {
        private const string BaseModifierKey = "EntityStatsTableEntry";

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

            AddStat(StatType.Hp, Mathf.Max(1, entry.MaxHp));
            AddStat(StatType.Attack, Mathf.Max(0, entry.Attack));
            AddStat(StatType.Defense, Mathf.Max(0, entry.Defense));
            AddStat(StatType.MoveSpeed, Mathf.Max(0, entry.MoveSpeed));
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
        /// 스탯에 modifier를 추가합니다. key는 발원지 구분용(장비, 버프 등). 해당 StatType이 없으면 생성 후 추가합니다.
        /// </summary>
        public void AddModifier(StatType type, string key, StatModifierType modiType, int value)
        {
            GetOrAddStat(type).AddModifier(key, modiType, value);
        }

        /// <summary>
        /// 해당 key로 등록된 modifier를 해당 스탯에서 모두 제거합니다. 해당 StatType이 없으면 아무 동작도 하지 않습니다.
        /// </summary>
        public void RemoveModifier(StatType type, string source)
        {
            if (_stats.TryGetValue(type, out var stat))
            {
                stat.RemoveModifier(source);
            }
        }

        /// <summary>
        /// StatType에 해당하는 EntityStat을 반환합니다. 없으면 Constant 0으로 생성해 Dictionary에 추가한 뒤 반환합니다.
        /// </summary>
        private EntityStat GetOrAddStat(StatType type)
        {
            if (_stats.TryGetValue(type, out var stat))
            {
                return stat;
            }

            AddStat(type, 0);
            return _stats[type];
        }

        private void AddStat(StatType type, int baseValue)
        {
            var entityStat = new EntityStat();
            entityStat.AddModifier(BaseModifierKey, StatModifierType.Constant, baseValue);
            _stats[type] = entityStat;
        }
    }
}
