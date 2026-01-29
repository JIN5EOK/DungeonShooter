namespace DungeonShooter
{
    /// <summary>
    /// 스탯 보너스 값 (플러스 증가·곱셈 증가).
    /// Multiply: 100=1.0, 110=10% 증가.
    /// </summary>
    public readonly struct StatBonus
    {
        public int HpAdd { get; }
        public int HpMultiply { get; }
        public int AttackAdd { get; }
        public int AttackMultiply { get; }
        public int DefenseAdd { get; }
        public int DefenseMultiply { get; }
        public int MoveSpeedAdd { get; }
        public int MoveSpeedMultiply { get; }

        public StatBonus(int hpAdd, int hpMultiply, int attackAdd, int attackMultiply,
            int defenseAdd, int defenseMultiply, int moveSpeedAdd, int moveSpeedMultiply)
        {
            HpAdd = hpAdd;
            HpMultiply = hpMultiply;
            AttackAdd = attackAdd;
            AttackMultiply = attackMultiply;
            DefenseAdd = defenseAdd;
            DefenseMultiply = defenseMultiply;
            MoveSpeedAdd = moveSpeedAdd;
            MoveSpeedMultiply = moveSpeedMultiply;
        }

        /// <summary>
        /// ItemTableEntry에서 StatBonus를 생성합니다.
        /// </summary>
        public static StatBonus From(ItemTableEntry entry)
        {
            if (entry == null)
            {
                return default;
            }

            return new StatBonus(
                entry.HpAdd, entry.HpMultiply,
                entry.AttackAdd, entry.AttackMultiply,
                entry.DefenseAdd, entry.DefenseMultiply,
                entry.MoveSpeedAdd, entry.MoveSpeedMultiply);
        }
    }
}
