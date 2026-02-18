using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터 설정 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 플레이어 캐릭터 정보
    /// </summary>
    [Serializable]
    public class PlayerConfigTableEntry : ITableEntry
    {
        public int Id { get; set; }
        
        /// <summary> 플레이어 캐릭터 이름 </summary>
        public string Name { get; set; }
        
        /// <summary> 플레이어 캐릭터 설명 </summary>
        public string Description { get; set; }
        
        /// <summary> 플레이어 게임오브젝트 어드레서블 주소 </summary>
        public string GameObjectKey { get; set; }
        
        /// <summary> 시작 무기 ItemTableEntry.Id </summary>
        public int StartWeaponId { get; set; }
        
        /// <summary> 1번 액티브 스킬 SkillTableEntry.Id </summary>
        public int Skill1Id { get; set; }
        
        /// <summary> 2번 액티브 스킬 SkillTableEntry.Id </summary>
        public int Skill2Id { get; set; }
        
        /// <summary> 기본 스탯 EntityStatsTableEntry.Id </summary>
        public int StatsId { get; set; }

        /// <summary> 기본적으로 지닐 SkillTableEntry.Id 리스트</summary>
        public List<int> AcquirableSkills { get; set; } = new();
    }
}
