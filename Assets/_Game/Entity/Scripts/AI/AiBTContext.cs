using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// AI 행동트리 실행에 필요한 컨텍스트입니다.
    /// </summary>
    public class AiBTContext
    {
        /// <summary>
        /// 행동트리를 실행하는 엔티티(자기 자신)
        /// </summary>
        public EntityBase Self { get; set; }

        /// <summary>
        /// 현재 타겟 엔티티 (예: 플레이어 추적)
        /// </summary>
        public EntityBase Target { get; set; }
        /// <summary>
        /// 사용 가능한 액티브 스킬 리스트
        /// </summary>
        public List<Skill> ActiveSkills { get; set; }
    }
}
