using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// Entity 스테이터스 관련 정적 유틸리티
    /// </summary>
    public static class EntityStatsHelper
    {
    /// <summary>
    /// 방어력 적용 최종 데미지 계산
    /// </summary>
    public static int CalculateFinalDamage(IEntityStats stats, int baseDamage)
    {
        if (stats == null) return Mathf.Max(1, baseDamage);
        var finalDamage = baseDamage - stats.Defense;
        return Mathf.Max(1, finalDamage);
    }
    

    /// <summary>
    /// source의 값을 target에 복사합니다.
    /// </summary>
    public static void Copy(IEntityStats source, IEntityStats target)
    {
        if (source == null || target == null) return;

        target.MoveSpeed = source.MoveSpeed;
        target.MaxHealth = source.MaxHealth;
        target.AttackDamage = source.AttackDamage;
        target.Defense = source.Defense;
    }
    }
}

