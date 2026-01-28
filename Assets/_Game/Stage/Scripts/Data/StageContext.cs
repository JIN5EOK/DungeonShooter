using System;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 구성에 필요한 문맥정보
    /// </summary>
    public class StageContext
    {
        public readonly int PlayerConfigTableId;
        public readonly int StageConfigTableId;

        public StageContext(int playerConfigTableId, int stageConfigTableId)
        {
            PlayerConfigTableId = playerConfigTableId;
            StageConfigTableId = stageConfigTableId;
        }
    }
}
