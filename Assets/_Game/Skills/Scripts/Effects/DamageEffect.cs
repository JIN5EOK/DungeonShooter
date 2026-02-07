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

        [Header("테이블의 지정된 Damage에 적용할 배율\n0 = 데미지 적용 안됨, 1.0f = 1배율")]
        public float damagePercent = 1.0f;

        private ISceneResourceProvider _resourceProvider;

        public override void Initialize(ISceneResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<DamageEffect>("SkillTableEntry가 null입니다.");
                return false;
            }

            var tableDamagePercent = entry.Damage;
            var skillDamagePercent = Mathf.RoundToInt(tableDamagePercent * damagePercent);
            if (skillDamagePercent <= 0)
            {
                LogHandler.LogWarning<DamageEffect>("데미지 값이 0 이하입니다.");
                return false;
            }

            var targetEntity = executeTarget == SkillOwner.Caster ? context.Caster : context.LastHitTarget;

            if (targetEntity == null)
            {
                LogHandler.LogWarning<DamageEffect>("데미지 적용 대상이 없습니다.");
                return false;
            }
            
            if (targetEntity.TryGetComponent(out HealthComponent health))
            {
                var finalDamage =
                    EntityStatsHelper.CalculatePercentDamage(context.Caster.StatGroup.GetStat(StatType.Attack)
                        , context.LastHitTarget.StatGroup.GetStat(StatType.Defense)
                        , skillDamagePercent);

                health.TakeDamage(finalDamage);
                
                if (_resourceProvider != null)
                {
                    // 추후 로직 최적화 필요
                    var damageTextGo = await _resourceProvider.GetInstanceAsync(DamageTextAddress);
                    if (damageTextGo != null)
                    {
                        var hitPosition = context.LastHitTarget.transform.position;
                        damageTextGo.transform.position = hitPosition + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.5f) + Vector3.up;
                        var tmpText = damageTextGo.GetComponentInChildren<TMP_Text>(true);
                        if (tmpText != null)
                        {
                            tmpText.text = finalDamage.ToString();
                        }
                    }
                }

                return true;
            }
            
            LogHandler.LogError<DamageEffect>("데미지 주기 실패");
            return false;
        }
    }
}
