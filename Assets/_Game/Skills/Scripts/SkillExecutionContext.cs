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
        
        public ISkillObjectFactory SkillObjectFactory { get; }
        public ISceneResourceProvider SceneResourceProvider { get; }
        private SkillExecutionContext(EntityBase caster, EntityBase other, ISceneResourceProvider sceneResourceProvider, ISkillObjectFactory skillObjectFactory)
        {
            SceneResourceProvider = sceneResourceProvider;   
            Caster = caster;
            LastHitTarget = other;
            SkillObjectFactory = skillObjectFactory;
        }

        /// <summary> 체이닝으로 값을 설정하기 위한 빈 컨텍스트를 반환합니다. </summary>
        public static SkillExecutionContext Create()
        {
            return default;
        }

        /// <summary> 시전자를 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithCaster(EntityBase newCaster)
        {
            return new SkillExecutionContext(newCaster, LastHitTarget, SceneResourceProvider, SkillObjectFactory);
        }

        /// <summary> 마지막 피격 대상을 설정한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithLastHitTarget(EntityBase newLastHitTarget)
        {
            return new SkillExecutionContext(Caster, newLastHitTarget, SceneResourceProvider, SkillObjectFactory);
        }
        
        /// <summary> 씬 리소스 제공자를 포함한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithResourceProvider(ISceneResourceProvider newResourceProvider)
        {
            return new SkillExecutionContext(Caster, LastHitTarget, newResourceProvider, SkillObjectFactory);
        }
        
        /// <summary> 스킬팩토리를 포함한 새 컨텍스트를 반환합니다. </summary>
        public SkillExecutionContext WithSkillObjectFactory(ISkillObjectFactory newSkillObjectFactory)
        {
            return new SkillExecutionContext(Caster, LastHitTarget, SceneResourceProvider, newSkillObjectFactory);
        }
    }
}
