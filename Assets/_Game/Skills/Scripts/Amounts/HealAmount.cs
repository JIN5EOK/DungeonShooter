using System;

namespace DungeonShooter
{
    /// <summary>
    /// 치유 수치
    /// </summary>
    [Serializable]
    public class HealAmount : AmountBase<int>
    {
        public HealAmount() : base() {}

        public HealAmount(int amount) : base(amount) {}
    }
}
