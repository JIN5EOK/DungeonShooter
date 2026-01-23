using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 데미지를 주는 이펙트
    /// </summary>
    [Serializable]
    public class DamageEffect : EffectBase
    {
        public override async UniTask<bool> Execute(EntityBase target, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<DamageEffect>("SkillTableEntry가 null입니다.");
                return false;
            }

            var damageAmount = entry.GetAmount<DamageAmount>();
            if (damageAmount == null)
            {
                LogHandler.LogError<DamageEffect>("DamageAmount를 찾을 수 없습니다.");
                return false;
            }

            if (target.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(damageAmount.Amount);
                return true;
            }
            
            LogHandler.LogError<DamageEffect>("데미지 주기 실패");
            return false;
        }
    }
}
