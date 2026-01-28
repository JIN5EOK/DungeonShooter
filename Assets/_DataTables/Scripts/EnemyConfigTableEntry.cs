using System;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 적 캐릭터 정보
    /// </summary>
    [Serializable]
    public class EnemyConfigTableEntry : ITableEntry
    {
        /// <summary>식별 ID </summary>
        public int Id { get; set; }

        /// <summary>적 이름</summary>
        public string Name { get; set; }

        /// <summary>적 프리팹 어드레서블 주소</summary>
        public string GameObjectKey { get; set; }

        /// <summary>AI 동작 타입 (행동트리 추가 예정)</summary>
        public string AIType { get; set; }

        /// <summary>기본 스탯 EntityStatsTableEntry.Id </summary>
        public int StatsId { get; set; }
    }
}

