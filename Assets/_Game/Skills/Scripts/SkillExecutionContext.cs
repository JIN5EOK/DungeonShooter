using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 시전 시 이펙트가 참고할 컨텍스트
    /// </summary>
    public readonly struct SkillExecutionContext
    {
        /// <summary>스킬 시전자</summary>
        public EntityBase Caster { get; }

        /// <summary>스킬 적용 대상</summary>
        public EntityBase Other { get; }

        private SkillExecutionContext(EntityBase caster, EntityBase other)
        {
            Caster = caster;
            Other = other;
        }

        /// <summary> 체이닝으로 값을 설정하기 위한 빈 컨텍스트를 반환합니다. </summary>
        public static SkillExecutionContext Create()
        {
            return default;
        }

        /// <summary> 시전자를 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithCaster(EntityBase caster)
        {
            return new SkillExecutionContext(caster, Other);
        }

        /// <summary> 대상을 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithOther(EntityBase other)
        {
            return new SkillExecutionContext(Caster, other);
        }
    }
}
