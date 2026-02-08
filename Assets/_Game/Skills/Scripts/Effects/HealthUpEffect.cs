using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 최대 체력을 증가시키는 패시브 이펙트.
    /// </summary>
    [System.Serializable]
    public class HealthUpEffect : EffectBase
    {
        [Header("테이블의 Amount에 적용할 배율 (0 = 미적용, 1.0f = 1배율)")]
        [SerializeField]
        private float _amountPercent = 1.0f;

        public override UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            return UniTask.FromResult(false);
        }

        public override void Activate(EntityBase owner, SkillTableEntry entry)
        {
            if (entry == null || owner?.StatGroup == null)
            {
                return;
            }

            var percent = Mathf.RoundToInt(entry.Amount * _amountPercent);
            if (percent <= 0)
            {
                return;
            }

            var multiply = 100 + percent;
            var bonus = new StatBonus(0, multiply, 0, 100, 0, 100, 0, 100);
            owner.StatGroup.ApplyStatBonus(this, bonus);
        }

        public override void Deactivate(EntityBase owner, SkillTableEntry entry)
        {
            if (owner?.StatGroup == null)
            {
                return;
            }

            owner.StatGroup.RemoveStatBonus(this);
        }
    }
}
