namespace DungeonShooter
{
    /// <summary>
    /// 스킬 시전 시 이펙트가 참고할 컨텍스트
    /// </summary>
    public readonly struct SkillExecutionContext
    {
        /// <summary>스킬 시전자</summary>
        public EntityBase Caster { get; }
        /// <summary>스킬에 피격당한 대상</summary>
        public EntityBase LastHitTarget { get; }
        /// <summary> 스킬 리소스 제공자 </summary>
        public ISceneResourceProvider ResourceProvider { get; }
        private SkillExecutionContext(EntityBase caster, EntityBase other, ISceneResourceProvider resourceProvider)
        {
            ResourceProvider = resourceProvider;   
            Caster = caster;
            LastHitTarget = other;
        }

        /// <summary> 체이닝으로 값을 설정하기 위한 빈 컨텍스트를 반환합니다. </summary>
        public static SkillExecutionContext Create()
        {
            return default;
        }

        /// <summary> 시전자를 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithCaster(EntityBase caster)
        {
            return new SkillExecutionContext(caster, LastHitTarget, ResourceProvider);
        }

        /// <summary> 마지막 피격 대상을 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithLastHitTarget(EntityBase lastHitTarget)
        {
            return new SkillExecutionContext(Caster, lastHitTarget, ResourceProvider);
        }
        
        /// <summary> 씬 리소스 제공자를 포함한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithResourceProvider(ISceneResourceProvider resourceProvider)
        {
            return new SkillExecutionContext(Caster, LastHitTarget, resourceProvider);
        }
    }
}
