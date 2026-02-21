using System;
using System.Collections.Generic;

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

        /// <summary>처치 시 획득 경험치</summary>
        public int Exp { get; set; }

        /// <summary>활성 스킬 ID 목록</summary>
        public List<int> ActiveSkills { get; set; } = new();

        /// <summary>드랍 아이템 가중치 (아이템 ID -> 가중치). 예: "1001:10/1002:5"</summary>
        public Dictionary<int, int> DropItemWeights { get; set; } = new();
    }
}

