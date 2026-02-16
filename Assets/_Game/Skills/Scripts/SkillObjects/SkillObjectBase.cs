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
        protected PoolableComponent poolable;
        protected SkillExecutionContext context;
        protected List<EffectBase> effects;
        protected SkillTableEntry skillTableEntry;

        protected virtual void Start()
        {
            poolable = GetComponent<PoolableComponent>();
        }
        
        /// <summary>
        /// 스킬 오브젝트를 초기화합니다.
        /// </summary>
        protected void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context)
        {
            this.context = context;
            this.effects = effects;
            this.skillTableEntry = skillTableEntry;
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

        /// <summary>
        /// 스킬 오브젝트를 해제합니다. PoolableComponent가 있으면 풀에 반환하고, 없으면 게임오브젝트를 파괴합니다.
        /// </summary>
        public void Release()
        {
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
