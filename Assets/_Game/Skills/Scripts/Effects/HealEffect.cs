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
        [Header("테이블의 Heal에 적용할 배율 (0 = 미적용, 1.0f = 1배율)")]
        [SerializeField]
        private float _healPercent = 1.0f;

        public override UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if (entry == null)
            {
                LogHandler.LogError<HealEffect>("SkillTableEntry가 null입니다.");
                return UniTask.FromResult(false);
            }

            var rawHeal = entry.Heal;
            var heal = Mathf.RoundToInt(rawHeal * _healPercent);
            if (heal <= 0)
            {
                LogHandler.LogWarning<HealEffect>("회복량이 0 이하입니다.");
                return UniTask.FromResult(false);
            }

            var target = context.Other;
            if (target != null && target.TryGetComponent(out HealthComponent health))
            {
                health.Heal(heal);
                return UniTask.FromResult(true);
            }

            LogHandler.LogError<HealEffect>("체력 회복 실패");
            return UniTask.FromResult(false);
        }
    }
}
