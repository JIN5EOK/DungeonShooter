using System;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 구성에 필요한 문맥정보
    /// </summary>
    public class StageContext
    {
        public readonly string PlayerPrefabKey;
        public readonly string StageConfigTableId;

        public StageContext(string playerPrefabKey, string stageConfigTableId)
        {
            PlayerPrefabKey = playerPrefabKey;
            StageConfigTableId = stageConfigTableId;
        }
    }
}
