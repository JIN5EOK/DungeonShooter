using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 체력을 회복하는 이펙트
/// </summary>
[System.Serializable]
public class HealEffect : EffectBase
{
    [Header("회복 설정")]
    public int healAmount;
    
    public override UniTask<bool> Execute(EntityBase target)
    {
        if (target.TryGetComponent(out HealthComponent health))
        {
            health.Heal(healAmount);
            return UniTask.FromResult(true);
        }
        
        Debug.LogError($"{nameof(DamageEffect)} : 체력 회복 실패");
        return UniTask.FromResult(false);
    }
}
