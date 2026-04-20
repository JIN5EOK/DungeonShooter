using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 최대 체력을 증가시키는 패시브 이펙트.
    /// </summary>
    [Serializable]
    public class HealthUpEffect : EffectBase
    {
        [Header("테이블의 Amount에 적용할 배율 (0 = 미적용, 1.0f = 1배율)")]
        [SerializeField]
        private float _amountPercent = 1.0f;

        public override void Activate(IEntityContext context, SkillLevelData levelData)
        {
            base.Activate(context, levelData);

            var percent = Mathf.RoundToInt(levelData.Amount * _amountPercent);
            var multiply = 100 + percent;
            var bonus = new StatBonus(0, multiply, 0, 100, 0, 100, 0, 100);
            context.Stats.ApplyStatBonus(this, bonus);
        }

        public override void Deactivate(IEntityContext context, SkillLevelData levelData)
        {
            base.Deactivate(context, levelData);

            context.Stats.RemoveStatBonus(this);
        }
    }
}
