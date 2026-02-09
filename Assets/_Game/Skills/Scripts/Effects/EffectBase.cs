using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonShooter
{
    /// <summary>
    /// 실행할 스킬 이펙트의 기본 추상 클래스
    /// </summary>
    [Serializable]
    public abstract class EffectBase
    {
        [SerializeField] 
        [Header("이펙트 시전자")]
        protected SkillOwner executeTarget;

        /// <summary>이펙트를 실행합니다. (액티브 스킬 사용 시 호출) </summary>
        public virtual UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if (context.SceneResourceProvider == null || context.Caster == null || entry == null)
            {
                LogHandler.LogError<EffectBase>("스킬 사용 실패, 파라미터가 올바르지 않습니다.");
                return UniTask.FromResult(false);
            }
            return UniTask.FromResult(true);
        }

        /// <summary>효과를 활성화합니다. (패시브 스킬 등록 시 호출) </summary>
        public virtual void Activate(EntityBase owner, SkillTableEntry entry)
        {
            if (owner == null || entry == null)
                throw new ArgumentException("스킬 활성화 실패, 파라미터가 올바르지 않습니다", nameof(EffectBase));
        }

        /// <summary>효과를 비활성화합니다. (패시브 스킬 해제 시 호출) </summary>
        public virtual void Deactivate(EntityBase owner, SkillTableEntry entry)
        {
            if (owner == null || entry == null)
                throw new ArgumentException("스킬 비활성화 실패, 파라미터가 올바르지 않습니다", nameof(EffectBase));
        }
    }
}
