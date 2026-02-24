using System;

namespace DungeonShooter
{
    /// <summary>
    /// 문자열 텍스트 테이블 엔트리
    /// ID로 조회할 수 있는 로컬라이즈/문자열 데이터
    /// </summary>
    [Serializable]
    public class StringTextTableEntry : ITableEntry
    {
        /// <summary>식별 ID</summary>
        public int Id { get; set; }

        /// <summary>표시할 텍스트 (쉼표 포함 시 CSV에서 열 구분에 주의)</summary>
        public string Text { get; set; }
    }
}
