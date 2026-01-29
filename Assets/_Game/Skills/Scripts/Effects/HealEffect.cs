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
        public override UniTask<bool> Execute(EntityBase owner, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<HealEffect>("SkillTableEntry가 null입니다.");
                return UniTask.FromResult(false);
            }

            // 직접 필드를 우선 사용, 0이면 딕셔너리에서 fallback
            var heal = entry.Heal;
            if (heal <= 0)
            {
                LogHandler.LogWarning<HealEffect>("회복량이 0 이하입니다.");
                return UniTask.FromResult(false);
            }

            if (owner.TryGetComponent(out HealthComponent health))
            {
                health.Heal(heal);
                return UniTask.FromResult(true);
            }

            LogHandler.LogError<HealEffect>("체력 회복 실패");
            return UniTask.FromResult(false);
        }
    }
}
