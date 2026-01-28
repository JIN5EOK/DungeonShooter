using UnityEngine;
using Jin5eok;

namespace DungeonShooter
{
    public abstract class EntityBase : MonoBehaviour
    {
        /// <summary>
        /// 이 Entity가 사용하는 런타임 스탯 인스턴스
        /// </summary>
        protected EntityStatsComponent statsComponent;

        /// <summary>
        /// 스탯 테이블 엔트리를 기반으로 스탯 컴포넌트를 초기화합니다.
        /// </summary>
        /// <param name="statsTableEntry">기본 스탯 테이블 엔트리</param>
        protected virtual void Initialize(EntityStatsTableEntry statsTableEntry)
        {
            var statsComponent = gameObject.AddOrGetComponent<EntityStatsComponent>();
            if (statsTableEntry == null)
            {
                return;
            }

            var stats = EntityStats.FromTableEntry(statsTableEntry);
            if (stats != null)
            {
                statsComponent.Initialize(stats);
            }
        }
    }
}
