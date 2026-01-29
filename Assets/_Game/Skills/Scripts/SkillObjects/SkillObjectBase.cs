using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬에 의해 소환되는 오브젝트(투사체, 장판 등)의 추상 베이스 클래스
    /// </summary>
    public abstract class SkillObjectBase : MonoBehaviour
    {
        /// <summary>
        /// 스킬 오브젝트를 초기화합니다.
        /// </summary>
        /// <param name="owner">스킬 시전자</param>
        /// <param name="effects">적중 시 실행할 이펙트 목록</param>
        /// <param name="skillTableEntry">스킬 수치 테이블 엔트리</param>
        public abstract void Initialize(EntityBase owner, List<EffectBase> effects, SkillTableEntry skillTableEntry);
    }
}
