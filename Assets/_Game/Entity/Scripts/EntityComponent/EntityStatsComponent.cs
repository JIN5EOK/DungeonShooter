using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// Entity 스탯을 보유하고 다른 컴포넌트에서 참조할 수 있게 하는 컴포넌트.
    /// 테이블 엔트리(원본 스탯)를 기준으로 초기화하며, 이후 장비·버프 등 연산 확장 가능.
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour
    {
        [Header("스테이터스")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _attack = 10;
        [SerializeField] private int _defense = 0;

        /// <summary>
        /// 스탯 테이블 엔트리를 기반으로 스탯을 초기화합니다.
        /// </summary>
        public void Initialize(EntityStatsTableEntry entry)
        {
            if (entry == null) return;

            _moveSpeed = Mathf.Max(0f, entry.MoveSpeed);
            _maxHealth = Mathf.Max(1, entry.MaxHp);
            _attack = Mathf.Max(0, entry.Attack);
            _defense = Mathf.Max(0, entry.Defense);
        }

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = Mathf.Max(1, value);
        }

        public int Attack
        {
            get => _attack;
            set => _attack = Mathf.Max(0, value);
        }

        public int Defense
        {
            get => _defense;
            set => _defense = Mathf.Max(0, value);
        }
    }
}
