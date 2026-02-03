namespace DungeonShooter
{
    /// <summary>
    /// 방 이벤트 트리거(특정 위치에 배치되는 이벤트 트리거) 타입.
    /// RoomEventTriggerTableEntry.ObjectType에 사용됩니다.
    /// </summary>
    public enum RoomEventTriggerType
    {
        /// <summary>플레이어 스폰 포인트 (종류는 런타임에 지정)</summary>
        PlayerSpawnPoint = 0,
        /// <summary>적 랜덤 스폰 포인트</summary>
        RandomEnemySpawn = 1
    }
}
