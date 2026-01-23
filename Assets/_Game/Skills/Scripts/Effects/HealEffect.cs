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
        public override async UniTask<bool> Execute(EntityBase target, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<HealEffect>("SkillTableEntry가 null입니다.");
                return false;
            }

            var healAmount = entry.GetAmount<HealAmount>();
            if (healAmount == null)
            {
                LogHandler.LogError<HealEffect>("HealAmount를 찾을 수 없습니다.");
                return false;
            }

            if (target.TryGetComponent(out HealthComponent health))
            {
                health.Heal(healAmount.Amount);
                return true;
            }
            
            LogHandler.LogError<HealEffect>("체력 회복 실패");
            return false;
        }
    }
}
