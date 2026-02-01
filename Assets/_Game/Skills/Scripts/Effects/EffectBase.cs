using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 실행할 스킬 이펙트의 기본 추상 클래스
    /// </summary>
    [Serializable]
    public abstract class EffectBase
    {
        [SerializeField]
        [Header("이펙트 적용 대상")]
        protected SkillOwner applyTarget;
        protected ISceneResourceProvider _resourceProvider;
        public virtual void Initialize(ISceneResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }
        
        /// <summary>
        /// 이펙트를 실행합니다. (액티브 스킬 사용 시 호출)
        /// </summary>
        /// <param name="context">시전 컨텍스트 (시전자, 대상, 시전/목표 위치)</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        /// <returns>실행 성공 여부</returns>
        public abstract UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry);
        
        /// <summary>
        /// 효과를 활성화합니다. (패시브 스킬 등록 시 호출)
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        public virtual void Activate(EntityBase owner, SkillTableEntry entry) { }
        
        /// <summary>
        /// 효과를 비활성화합니다. (패시브 스킬 해제 시 호출)
        /// </summary>
        /// <param name="owner">스킬 소유자</param>
        /// <param name="entry">스킬 수치 테이블 엔트리</param>
        public virtual void Deactivate(EntityBase owner, SkillTableEntry entry) { }
    }
}
