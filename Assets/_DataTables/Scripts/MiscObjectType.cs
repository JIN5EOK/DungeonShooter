namespace DungeonShooter
{
    /// <summary>
    /// 기타 오브젝트(방 특수 오브젝트) 타입.
    /// MiscObjectTableEntry.ObjectType에 사용됩니다.
    /// </summary>
    public enum MiscObjectType
    {
        /// <summary>플레이어 스폰 포인트 (종류는 런타임에 지정)</summary>
        PlayerSpawnPoint = 0,
        /// <summary>적 랜덤 스폰 포인트</summary>
        RandomEnemySpawn = 1
    }
}
