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
        /// <summary>
        /// 식별 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// SkillData 에셋 주소
        /// </summary>
        public string SkillDataKey { get; set; }

        /// <summary>
        /// 스킬 수치 딕셔너리 (Type을 키로, AmountBase를 값으로)
        /// </summary>
        public Dictionary<Type, AmountBase> Amounts { get; set; } = new Dictionary<Type, AmountBase>();

        /// <summary>
        /// 스킬 지연 시간
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        /// 스킬 쿨다운 시간
        /// </summary>
        public float Cooldown { get; set; }

        /// <summary>
        /// 타겟 개수
        /// </summary>
        public int TargetCount { get; set; }

        /// <summary>
        /// 넉백 힘
        /// </summary>
        public float KnockbackForce { get; set; }

        /// <summary>
        /// 지정된 타입의 Amount를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">AmountBase를 상속한 타입</typeparam>
        /// <returns>Amount 인스턴스, 없으면 null</returns>
        public T GetAmount<T>() where T : AmountBase
        {
            var type = typeof(T);
            if (Amounts.TryGetValue(type, out var amount) && amount is T result)
            {
                return result;
            }

            return null;
        }
    }
}
