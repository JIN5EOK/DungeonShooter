using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// EntityStats를 MonoBehaviour에 연결하여 사용하는 컴포넌트
    /// Inspector에서 스테이터스를 설정하고 다른 컴포넌트에서 참조할 수 있습니다.
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IEntityStats
    {
        [Header("스테이터스")]
        [SerializeField] private EntityStats stats = new EntityStats();

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

        public int Attack
        {
            get => stats.Attack;
            set => stats.Attack = value;
        }

        public int Defense
        {
            get => stats.Defense;
            set => stats.Defense = value;
        }
    }
}

