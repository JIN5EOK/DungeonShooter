using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬에 의해 소환되는 오브젝트(투사체, 장판 등)의 추상 베이스 클래스
    /// </summary>
    public abstract class SkillObjectBase : MonoBehaviour
    {
        protected SkillExecutionContext context;
        protected List<EffectBase> effects;
        protected SkillTableEntry skillTableEntry;

        /// <summary>
        /// 스킬 오브젝트를 초기화합니다.
        /// </summary>
        /// <param name="effects">적중 시 실행할 이펙트 목록</param>
        /// <param name="skillTableEntry">스킬 수치 테이블 엔트리</param>
        /// <param name="context">시전 컨텍스트 (초기 위치 산출용)</param>
        /// <param name="spawnPosition">스폰 위치 (Caster = 시전자 위치, Target = 대상 위치)</param>
        protected void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context, SkillOwner spawnPosition)
        {
            this.context = context;
            this.effects = effects;
            this.skillTableEntry = skillTableEntry;

            var position = spawnPosition == SkillOwner.LastHitTarget ? context.LastHitTarget.transform.position : context.Caster.transform.position;
            transform.position = position;
        }

        /// <summary>
        /// 시전 컨텍스트로 이펙트를 실행합니다. (투사체 적중 시 등)
        /// </summary>
        protected async UniTask RunEffectsAsync(SkillExecutionContext context)
        {
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    await effect.Execute(context, skillTableEntry);
                }
            }
        }
    }
}
