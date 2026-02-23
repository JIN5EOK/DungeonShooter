using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 체력을 회복하는 이펙트
    /// </summary>
    [System.Serializable]
    public class HealEffect : EffectBase
    {
        [Header("테이블의 Amount에 적용할 배율 (0 = 미적용, 1.0f = 1배율)")]
        [SerializeField]
        private float _healPercent = 1.0f;

        public override UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            base.Execute(context, entry);
            
            var rawHeal = entry.Amount;
            var heal = Mathf.RoundToInt(rawHeal * _healPercent);
            
            var targetEntity = executeTarget == SkillOwner.Caster
                ? context.Caster 
                : context.LastHitTarget;

            var health = targetEntity.GetComponent<IHealthComponent>();
            if (health != null)
            {
                health.Heal(heal);
                return UniTask.FromResult(true);
            }

            LogHandler.LogError<HealEffect>("체력 회복 실패");
            return UniTask.FromResult(false);
        }
    }
}
