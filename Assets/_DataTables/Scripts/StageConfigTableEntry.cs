using System;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 스테이지 설정 정보
    /// </summary>
    [Serializable]
    public class StageConfigTableEntry : ITableEntry
    {
        public int Id { get; set; }

        /// <summary> 스테이지 이름 </summary>
        public string Name { get; set; }

        /// <summary> 기본 지형 타일 주소 (Addressable 주소) </summary>
        public string GroundTileKey { get; set; }
        
        /// <summary> 스테이지에 등장할 수 있는 적들의 라벨 </summary>
        public string StageEnemiesLabel { get; set; }
        
        /// <summary> 시작 방 데이터 라벨</summary>
        public string StartRoomsLabel { get; set; }
        
        /// <summary> 일반 방 데이터 라벨 </summary>
        public string NormalRoomsLabel { get; set; }
        
        /// <summary> 보스 방 데이터 라벨 </summary>
        public string BossRoomsLabel { get; set; }
    }
}
