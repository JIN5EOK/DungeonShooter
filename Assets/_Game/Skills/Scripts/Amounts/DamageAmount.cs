using System;

namespace DungeonShooter
{
    /// <summary>
    /// 데미지 수치
    /// </summary>
    [Serializable]
    public class DamageAmount : AmountBase<int>
    {
        public DamageAmount() : base(){}

        public DamageAmount(int amount) : base(amount){}
    }
}
