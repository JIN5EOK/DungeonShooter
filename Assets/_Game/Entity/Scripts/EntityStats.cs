using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace DungeonShooter
{
    /// <summary>
    /// Entity들이 공통적으로 사용하는 스테이터스를 관리하는 클래스
    /// </summary>
    [Serializable]
    public class EntityStats : IEntityStats
    {
        [Header("기본 스테이터스")]
        [Tooltip("이동 속도")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("최대 체력")]
        [SerializeField] private int maxHealth = 100;

        [FormerlySerializedAs("attackDamage")]
        [Tooltip("기본 공격력")]
        [SerializeField] private int attack = 10;

        [Tooltip("방어력 (받는 데미지 감소량)")]
        [SerializeField] private int defense = 0;

        // 프로퍼티
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = Mathf.Max(1, value);
        }

        public int Attack
        {
            get => attack;
            set => attack = Mathf.Max(0, value);
        }

        public int Defense
        {
            get => defense;
            set => defense = Mathf.Max(0, value);
        }

        /// <summary>
        /// 테이블 엔트리에서 EntityStats 인스턴스를 생성합니다.
        /// </summary>
        public static EntityStats FromTableEntry(EntityStatsTableEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            var stats = new EntityStats
            {
                MoveSpeed = entry.MoveSpeed,
                MaxHealth = entry.MaxHp,
                Attack = entry.Attack,
                Defense = entry.Defense
            };

            return stats;
        }

        public EntityStats() {}

        private EntityStats(EntityStats other)
        {
            if (other == null) return;

            moveSpeed = other.moveSpeed;
            maxHealth = other.maxHealth;
            attack = other.attack;
            defense = other.defense;
        }

        /// <summary>
        /// 스테이터스 복사
        /// </summary>
        public EntityStats Clone()
        {
            return new EntityStats(this);
        }
    }
}

