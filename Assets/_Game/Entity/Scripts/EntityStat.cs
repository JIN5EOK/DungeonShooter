using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 개별 스탯. 장비·아이템 등에 의한 modifier를 반영해 최종 수치를 계산한다.
    /// Constant·Add는 int 합산, Multiply는 100 단위(100=1.0, 200=2배, 50=0.5배). 최종만 int로 반환한다.
    /// </summary>
    public class EntityStat
    {
        private const int MultiplyUnit = 100;

        private readonly Dictionary<string, List<StatModifier>> _modifiersByKey = new Dictionary<string, List<StatModifier>>();
        private int _cachedValue;
        private int _cachedOriginValue;
        private bool _dirty = true;

        /// <summary>
        /// 최종 수치 (캐시 반영, 변동 시 재계산)
        /// </summary>
        public int GetValue()
        {
            if (_dirty)
            {
                Recompute();
            }

            return _cachedValue;
        }

        /// <summary>
        /// 원본 수치 (Constant 합만 반환, 캐시 반영)
        /// </summary>
        public int GetOriginValue()
        {
            if (_dirty)
            {
                Recompute();
            }

            return _cachedOriginValue;
        }

        /// <summary>
        /// modifier 추가. Multiply는 100 단위(100=1.0, 200=2.0배, 50=0.5배).
        /// </summary>
        public void AddModifier(string key, StatModifierType modiType, int value)
        {
            if (!_modifiersByKey.TryGetValue(key, out var list))
            {
                list = new List<StatModifier>();
                _modifiersByKey[key] = list;
            }

            list.Add(new StatModifier(modiType, value));
            _dirty = true;
        }

        /// <summary>
        /// 해당 key로 등록된 modifier를 모두 제거한다.
        /// </summary>
        public void RemoveModifier(string key)
        {
            _modifiersByKey.Remove(key);
            _dirty = true;
        }

        private void Recompute()
        {
            int baseSum = 0;
            int addSum = 0;
            int multiplyProduct = MultiplyUnit;

            foreach (var list in _modifiersByKey.Values)
            {
                foreach (var m in list)
                {
                    switch (m.Type)
                    {
                        case StatModifierType.Constant:
                            baseSum += m.Value;
                            break;
                        case StatModifierType.Add:
                            addSum += m.Value;
                            break;
                        case StatModifierType.Multiply:
                            multiplyProduct = multiplyProduct * m.Value / MultiplyUnit;
                            break;
                    }
                }
            }

            _cachedOriginValue = baseSum;
            _cachedValue = (baseSum + addSum) * multiplyProduct / MultiplyUnit;
            _dirty = false;
        }

        private readonly struct StatModifier
        {
            public readonly StatModifierType Type;
            public readonly int Value;

            public StatModifier(StatModifierType type, int value)
            {
                Type = type;
                Value = value;
            }
        }
    }
}
