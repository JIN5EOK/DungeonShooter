/// <summary>
/// 공통 스테이터스 접근용 인터페이스 (읽기/쓰기)
/// </summary>
public interface IEntityStats
{
    float MoveSpeed { get; set; }
    int MaxHealth { get; set; }
    int AttackDamage { get; set; }
    float AttackRange { get; set; }
    float AttackCooldown { get; set; }
    int Defense { get; set; }
    float KnockbackResistance { get; set; }
}
