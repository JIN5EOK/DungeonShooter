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
        public override UniTask<bool> Execute(EntityBase owner, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<DamageEffect>("SkillTableEntry가 null입니다.");
                return UniTask.FromResult(false);
            }

            // 직접 필드를 우선 사용, 0이면 딕셔너리에서 fallback
            var damage = entry.Damage;
            if (damage <= 0)
            {
                LogHandler.LogWarning<DamageEffect>("데미지 값이 0 이하입니다.");
                return UniTask.FromResult(false);
            }

            if (owner.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(damage);
                return UniTask.FromResult(true);
            }

            LogHandler.LogError<DamageEffect>("데미지 주기 실패");
            return UniTask.FromResult(false);
        }
    }
}
