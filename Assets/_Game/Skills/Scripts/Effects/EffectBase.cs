using System;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 실행할 스킬 이펙트의 기본 추상 클래스
    /// </summary>
    [Serializable]
    public abstract class EffectBase
    {
        /// <summary>
        /// 이펙트를 실행합니다. (액티브 스킬 사용 시 호출)
        /// </summary>
        /// <param name="target">스킬을 적용할 Entity</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        /// <returns>실행 성공 여부</returns>
        public abstract UniTask<bool> Execute(EntityBase target, SkillTableEntry entry);
        
        /// <summary>
        /// 효과를 활성화합니다. (패시브 스킬 등록 시 호출)
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        public virtual void Activate(EntityBase owner, SkillTableEntry entry)
        {
            // 기본 구현은 비어있음
        }
        
        /// <summary>
        /// 효과를 비활성화합니다. (패시브 스킬 해제 시 호출)
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        public virtual void Deactivate(EntityBase owner, SkillTableEntry entry)
        {
            // 기본 구현은 비어있음
        }
    }
}
