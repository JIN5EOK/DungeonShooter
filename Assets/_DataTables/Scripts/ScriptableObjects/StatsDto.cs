using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 직렬화를 위한 스탯 집합 클래스
    /// </summary>
    [Serializable]
    public class StatsDto : ITableEntry
    {
        /// <summary>식별 ID </summary>
        public int Id => _id;
        [SerializeField] private int _id;
        /// <summary>최대 체력</summary>
        public int MaxHp => _maxHp;
        [SerializeField] private int _maxHp;

        /// <summary>기본 공격력</summary>
        public int Attack => _attack;
        [SerializeField] private int _attack;

        /// <summary>방어력</summary>
        public int Defense => _defense;
        [SerializeField] private int _defense;

        /// <summary>이동 속도</summary>
        public int MoveSpeed => _moveSpeed;
        [SerializeField] private int _moveSpeed;
    }
}

