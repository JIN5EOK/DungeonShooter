using UnityEngine;

/// <summary>
/// 체력을 회복하는 이펙트
/// </summary>
[System.Serializable]
public class HealEffect : EffectBase
{
    [Header("회복 설정")]
    public int healAmount;
    
    public override bool Execute(EntityBase owner, EntityBase target)
    {
        // TODO: 구현 예정
        return false;
    }
}
