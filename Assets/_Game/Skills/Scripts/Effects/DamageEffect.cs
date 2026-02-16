using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 데미지를 주는 이펙트
    /// </summary>
    [Serializable]
    public class DamageEffect : EffectBase
    {
        private const string DamageTextAddress = "DamageText";

        [Header("테이블의 Amount에 적용할 배율\n0 = 데미지 적용 안됨, 1.0f = 1배율")]
        public float damagePercent = 1.0f;

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if (!await base.Execute(context, entry))
                return false;
        
            var targetEntity = executeTarget == SkillOwner.Caster
            ? context.Caster 
            : context.LastHitTarget;

            if (targetEntity == null)
            {
                LogHandler.LogError<DamageEffect>("데미지 적용 대상이 없습니다.");
                return false;
            }
            
            var tableDamagePercent = entry.Amount;
            var skillDamagePercent = Mathf.RoundToInt(tableDamagePercent * damagePercent);
            

            if (targetEntity.TryGetComponent(out HealthComponent health))
            {
                var casterAtk = context.Caster.StatContainer.GetStat(StatType.Attack).GetValue();
                var targetDef = context.LastHitTarget.StatContainer.GetStat(StatType.Defense).GetValue();
                var finalDamage = EntityStatsHelper.CalculatePercentDamage(casterAtk, targetDef, skillDamagePercent);

                health.TakeDamage(finalDamage);
                
                var damageTextGo = await context.SkillObjectFactory.CreateSkillObjectAsync<ParticleSkillObject>(DamageTextAddress);
                if (damageTextGo != null && context.LastHitTarget != null)
                {
                    var hitPosition = context.LastHitTarget.transform.position;
                    damageTextGo.transform.position = hitPosition + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.5f) + Vector3.up;
                    var tmpText = damageTextGo.GetComponentInChildren<TMP_Text>(true);
                    tmpText.SetText(finalDamage.ToString());
                }

                return true;
            }
            
            LogHandler.LogError<DamageEffect>("데미지 주기 실패");
            return false;
        }
    }
}
