using UnityEngine;

/// <summary>
/// 데미지를 주는 이펙트
/// </summary>
[System.Serializable]
public class DamageEffect : EffectBase
{
    [Header("데미지 설정")]
    public int damage;
    
    public override bool Execute(EntityBase owner)
    {
        // TODO: 구현 예정
        return false;
    }
}
