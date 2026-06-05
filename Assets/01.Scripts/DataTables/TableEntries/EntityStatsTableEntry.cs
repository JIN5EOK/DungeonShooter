using System;

namespace DungeonShooter
{
    /// <summary>
    /// Entity 기본 스탯 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 개체별 기본 스탯 정보
    /// </summary>
    [Serializable]
    public class EntityStatsTableEntry : ITableEntry
    {
        /// <summary>식별 ID </summary>
        public int Id { get; set; }

        /// <summary>최대 체력</summary>
        public int MaxHp { get; set; }

        /// <summary>기본 공격력</summary>
        public int Attack { get; set; }

        /// <summary>방어력</summary>
        public int Defense { get; set; }

        /// <summary>이동 속도</summary>
        public int MoveSpeed { get; set; }
    }
}

