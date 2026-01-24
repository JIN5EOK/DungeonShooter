using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 수치 테이블 엔트리
    /// CSV 등 테이블을 통해 편집되는 스킬의 수치 정보
    /// </summary>
    [Serializable]
    public class SkillTableEntry
    {
        /// <summary>식별 ID </summary>
        public int Id { get; set; }

        /// <summary>SkillData 에셋 주소</summary>
        public string SkillDataKey { get; set; }

        /// <summary>Float 타입 수치 딕셔너리 </summary>
        public Dictionary<string, float> FloatAmounts { get; set; } = new Dictionary<string, float>();

        /// <summary>Int 타입 수치 딕셔너리 </summary>
        public Dictionary<string, int> IntAmounts { get; set; } = new Dictionary<string, int>();

        /// <summary>데미지 </summary>
        public int Damage { get; set; }

        /// <summary>치유량 </summary>
        public int Heal { get; set; }

        /// <summary> 스킬 지연 시간 </summary>
        public float Delay { get; set; }

        /// <summary>스킬 쿨다운 시간 </summary>
        public float Cooldown { get; set; }

        /// <summary>타겟 개수 </summary>
        public int TargetCount { get; set; }

        /// <summary>넉백 힘 </summary>
        public float KnockbackForce { get; set; }

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
    }
}
