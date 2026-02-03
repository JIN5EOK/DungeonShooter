using System;

namespace DungeonShooter
{
    /// <summary>
    /// 방 이벤트 트리거 테이블 엔트리.
    /// 특정 위치에 배치되는 이벤트 트리거(플레이어 스폰, 적 랜덤 스폰 등)를 정의합니다.
    /// Id가 RoomEventTriggerType enum 값과 동일하게 사용됩니다.
    /// </summary>
    [Serializable]
    public class RoomEventTriggerTableEntry : ITableEntry
    {
        /// <summary>식별 ID (RoomEventTriggerType enum 값과 동일)</summary>
        public int Id { get; set; }

        /// <summary>표시 이름 (에디터/디버그용)</summary>
        public string Name { get; set; }
    }
}
