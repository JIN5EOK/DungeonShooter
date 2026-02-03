using System;

namespace DungeonShooter
{
    /// <summary>
    /// 기타 오브젝트 테이블 엔트리.
    /// 플레이어 스폰, 적 랜덤 스폰 등 방 전용 특수 오브젝트를 정의합니다.
    /// </summary>
    [Serializable]
    public class MiscObjectTableEntry : ITableEntry
    {
        /// <summary>식별 ID</summary>
        public int Id { get; set; }

        /// <summary>표시 이름 (에디터/디버그용)</summary>
        public string Name { get; set; }

        /// <summary>기타 오브젝트 타입 (스폰 포인트 등)</summary>
        public MiscObjectType ObjectType { get; set; }

        /// <summary>에디터 배치 시 사용할 마커 프리팹 어드레스 (선택, 비어 있으면 빈 오브젝트 등으로 표시)</summary>
        public string GameObjectKey { get; set; }
    }
}
