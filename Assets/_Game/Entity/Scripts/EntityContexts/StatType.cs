namespace DungeonShooter
{
    /// <summary>
    /// Entity 스탯 타입
    /// </summary>
    public enum StatType
    {
        Hp, // 최대 체력
        Attack,
        Defense,
        MoveSpeed
    }

    /// <summary>
    /// 스탯 변경 데이터 타입 (Constant: 기본수치, Add: 더하기, Multiply: 곱하기)
    /// </summary>
    public enum StatModifierType
    {
        Constant,
        Add,
        Multiply
    }
}
