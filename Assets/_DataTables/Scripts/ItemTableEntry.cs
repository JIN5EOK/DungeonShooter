using System;

namespace DungeonShooter
{
    /// <summary>
    /// 아이템 수치 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 아이템의 수치 정보
    /// </summary>
    [Serializable]
    public class ItemTableEntry : ITableEntry
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public ItemType ItemType { get; set; }
        public int MaxStackCount { get; set; }
        public int UseSkillId { get; set; }

        /// <summary>
        /// 패시브 효과 (SkillTableEntry ID, Passive 전용)
        /// 아이템이 인벤토리에 들어왔을 때 적용
        /// </summary>
        public int PassiveSkillId { get; set; }

        /// <summary>
        /// 장착 효과 (SkillTableEntry ID, Weapon 전용)
        /// 무기 아이템을 장착 중일 때 효과
        /// </summary>
        public int EquipSkillId { get; set; }

        /// <summary>
        /// 액티브 효과 (SkillTableEntry ID, Weapon 전용)
        /// 무기 아이템을 사용할 때 효과
        /// </summary>
        public int ActiveSkillId { get; set; }

        /// <summary>
        /// 아이템 아이콘 주소 (Sprite Addressable 주소)
        /// </summary>
        public string ItemIcon { get; set; }

        /// <summary>체력 고정 증가량</summary>
        public int HpAdd { get; set; }

        /// <summary>체력 배율 (100=1.0, 120=20% 증가)</summary>
        public int HpMultiply { get; set; }

        /// <summary>공격력 고정 증가량</summary>
        public int AttackAdd { get; set; }

        /// <summary>공격력 배율 (100=1.0)</summary>
        public int AttackMultiply { get; set; }

        /// <summary>방어력 고정 증가량</summary>
        public int DefenseAdd { get; set; }

        /// <summary>방어력 배율 (100=1.0)</summary>
        public int DefenseMultiply { get; set; }

        /// <summary>이동속도 고정 증가량</summary>
        public int MoveSpeedAdd { get; set; }

        /// <summary>이동속도 배율 (100=1.0)</summary>
        public int MoveSpeedMultiply { get; set; }
    }
}
