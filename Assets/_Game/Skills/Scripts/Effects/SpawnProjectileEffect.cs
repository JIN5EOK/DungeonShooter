using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 투사체를 소환하는 이펙트
/// </summary>
[System.Serializable]
public class SpawnProjectileEffect : EffectBase
{
    [Header("투사체 프리팹")]
    [SerializeField]
    public AssetReferenceT<Projectile> projectile;
    
    [Header("투사체 적중시 효과")]
    [SerializeReference]
    public List<EffectBase> effects = new List<EffectBase>();
    
    public override bool Execute(EntityBase owner, EntityBase target)
    {
        // TODO: 구현 예정
        return false;
    }
}
