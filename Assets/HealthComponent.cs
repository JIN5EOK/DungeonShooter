using UnityEngine;
using System;

/// <summary>
/// HP를 가진 엔티티에 부착하는 독립적인 컴포넌트
/// </summary>
public class HealthComponent : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool startWithFullHealth = true;
    
    private int currentHealth;

    // 이벤트 시스템
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action<int, int> OnDamaged; // (damage, currentHealth)
    public event Action<int, int> OnHealed; // (amount, currentHealth)
    public event Action OnDeath;

    // 프로퍼티
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

    private void Awake()
    {
        if (startWithFullHealth)
        {
            currentHealth = maxHealth;
        }
    }

    /// <summary>
    /// 데미지를 받습니다
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (damage < 0) damage = 0;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamaged?.Invoke(damage, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력을 회복합니다
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount < 0) amount = 0;

        int oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        int actualHealed = currentHealth - oldHealth;
        if (actualHealed > 0)
        {
            OnHealed?.Invoke(actualHealed, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 최대 체력을 변경합니다
    /// </summary>
    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = Mathf.Max(1, newMax);
        
        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 즉시 사망 처리
    /// </summary>
    public void Kill()
    {
        if (IsDead) return;
        
        currentHealth = 0;
        Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 체력을 완전 회복합니다
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }
}
