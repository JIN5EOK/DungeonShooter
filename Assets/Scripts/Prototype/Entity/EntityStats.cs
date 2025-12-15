using UnityEngine;
using System;

/// <summary>
/// Entity들이 공통적으로 사용하는 스테이터스를 관리하는 클래스
/// </summary>
[Serializable]
public class EntityStats : IEntityStats
{
    [Header("기본 스테이터스")]
    [Tooltip("이동 속도")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Tooltip("최대 체력")]
    [SerializeField] private int maxHealth = 100;

    [Tooltip("기본 공격력")]
    [SerializeField] private int attackDamage = 10;
    
    [Tooltip("공격 범위")]
    [SerializeField] private float attackRange = 1.5f;
    
    [Tooltip("공격 쿨다운 (초)")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Tooltip("방어력 (받는 데미지 감소량)")]
    [SerializeField] private int defense = 0;
    
    [Tooltip("넉백 저항 (0~1, 1이면 넉백 무효)")]
    [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0f;

    // 프로퍼티
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = Mathf.Max(1, value);
    }

    public int AttackDamage
    {
        get => attackDamage;
        set => attackDamage = Mathf.Max(0, value);
    }

    public float AttackRange
    {
        get => attackRange;
        set => attackRange = Mathf.Max(0f, value);
    }

    public float AttackCooldown
    {
        get => attackCooldown;
        set => attackCooldown = Mathf.Max(0f, value);
    }

    public int Defense
    {
        get => defense;
        set => defense = Mathf.Max(0, value);
    }

    public float KnockbackResistance
    {
        get => knockbackResistance;
        set => knockbackResistance = Mathf.Clamp01(value);
    }

    public EntityStats() {}

    private EntityStats(EntityStats other)
    {
        if (other == null) return;

        moveSpeed = other.moveSpeed;
        maxHealth = other.maxHealth;
        attackDamage = other.attackDamage;
        attackRange = other.attackRange;
        attackCooldown = other.attackCooldown;
        defense = other.defense;
        knockbackResistance = other.knockbackResistance;
    }

    /// <summary>
    /// 스테이터스 복사
    /// </summary>
    public EntityStats Clone()
    {
        return new EntityStats(this);
    }
}

