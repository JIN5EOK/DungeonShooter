using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 데이터 테이블 관리자 인터페이스
    /// 데이터를 어떻게 가져올 것인가의 내부 구현은 구체 클래스에서 수행
    /// </summary>
    public interface ITableRepository
    {
        /// <summary>
        /// 테이블 엔트리를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">테이블 엔트리 타입</typeparam>
        /// <param name="id">엔트리 ID</param>
        /// <returns>테이블 엔트리, 없으면 null</returns>
        T GetTableEntry<T>(int id) where T : class, ITableEntry;

        /// <summary>
        /// 지정한 타입의 테이블 엔트리 전체 목록을 가져옵니다.
        /// </summary>
        /// <typeparam name="T">테이블 엔트리 타입</typeparam>
        /// <returns>해당 타입의 엔트리 목록 (없으면 빈 목록)</returns>
        IReadOnlyList<T> GetAllTableEntries<T>() where T : class, ITableEntry;
    }
}
