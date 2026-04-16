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
    }
}
