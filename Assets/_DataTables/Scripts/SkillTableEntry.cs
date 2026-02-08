using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 수치 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 스킬의 수치 정보
    /// </summary>
    [Serializable]
    public class SkillTableEntry : ITableEntry
    {
        /// <summary>식별 ID </summary>
        public int Id { get; set; }
        
        public string SkillName { get; set; }
        public string SkillDescription { get; set; }

        /// <summary>스킬 아이콘 주소</summary>
        public string SkillIconKey { get; set; }

        /// <summary>SkillData 에셋 주소</summary>
        public string SkillDataKey { get; set; }

        /// <summary>Float 타입 수치 딕셔너리 </summary>
        public Dictionary<string, float> FloatAmounts { get; set; } = new Dictionary<string, float>();

        /// <summary>Int 타입 수치 딕셔너리 </summary>
        public Dictionary<string, int> IntAmounts { get; set; } = new Dictionary<string, int>();

        /// <summary>데미지(%) 또는 회복량 등 스킬 수치 (효과 타입에 따라 의미 상이)</summary>
        public int Amount { get; set; }

        /// <summary> 스킬 사용시 지연 시간 </summary>
        public float Delay { get; set; }

        public float Cooldown { get; set; }

        /// <summary>스킬 그룹의 근간이 되는 SkillTableEntry의 ID</summary>
        public int SkillRootId { get; set; }

        /// <summary>Int 타입 수치를 가져옵니다.</summary>
        public int GetIntAmount(string key)
        {
            return IntAmounts.GetValueOrDefault(key);
        }

        /// <summary>Float 타입 수치를 가져옵니다. </summary>
        public float GetFloatAmount(string key)
        {
            return FloatAmounts.GetValueOrDefault(key);
        }

        /// <summary>스킬 레벨을 계산합니다 (Id - SkillRootId)</summary>
        public int GetLevel()
        {
            return Id - SkillRootId;
        }

        /// <summary>다음 레벨 스킬 ID를 계산합니다 (순수 계산만 수행, 존재 여부는 확인하지 않음)</summary>
        public int CalculateNextLevelSkillId()
        {
            var level = GetLevel();
            return SkillRootId + level + 1;
        }

        /// <summary>이전 레벨 스킬 ID를 계산합니다 (순수 계산만 수행, 존재 여부는 확인하지 않음)</summary>
        public int CalculatePreviousLevelSkillId()
        {
            var level = GetLevel();
            if (level <= 0)
            {
                return 0; // 이전 레벨 없음
            }

            return SkillRootId + (level - 1);
        }

        /// <summary>특정 레벨의 스킬 ID를 계산합니다</summary>
        public int GetSkillIdByLevel(int targetLevel)
        {
            if (targetLevel < 0)
            {
                return 0;
            }

            return SkillRootId + targetLevel;
        }
    }
}
