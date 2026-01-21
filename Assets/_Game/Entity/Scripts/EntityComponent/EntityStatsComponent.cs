using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// EntityStats를 MonoBehaviour에 연결하여 사용하는 컴포넌트
    /// Inspector에서 스테이터스를 설정하고 다른 컴포넌트에서 참조할 수 있습니다.
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IEntityStats
    {
        [Header("스테이터스 설정")]
        [SerializeField] private EntityStats stats = new EntityStats();

        /// <summary>
        /// 현재 스테이터스 (읽기 전용)
        /// </summary>
        public EntityStats Stats => stats;

        /// <summary>
        /// 스테이터스 초기화 (다른 EntityStats로 설정)
        /// </summary>
        public void Initialize(EntityStats newStats)
        {
            if (newStats != null)
            {
                stats = newStats.Clone();
            }
        }

        // IEntityStats 구현 (stats에 위임)
        public float MoveSpeed
        {
            get => stats.MoveSpeed;
            set => stats.MoveSpeed = value;
        }

        public int MaxHealth
        {
            get => stats.MaxHealth;
            set => stats.MaxHealth = value;
        }

        public int AttackDamage
        {
            get => stats.AttackDamage;
            set => stats.AttackDamage = value;
        }

        public float AttackRange
        {
            get => stats.AttackRange;
            set => stats.AttackRange = value;
        }

        public float AttackCooldown
        {
            get => stats.AttackCooldown;
            set => stats.AttackCooldown = value;
        }

        public int Defense
        {
            get => stats.Defense;
            set => stats.Defense = value;
        }

        public float KnockbackResistance
        {
            get => stats.KnockbackResistance;
            set => stats.KnockbackResistance = value;
        }
    }
}

