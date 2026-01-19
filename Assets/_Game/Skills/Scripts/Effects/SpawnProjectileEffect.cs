using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

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

    private string ProjectileAddress => projectile.RuntimeKey.ToString();
    private IStageResourceProvider _resourceProvider;
    
    [Inject]
    public SpawnProjectileEffect(IStageResourceProvider resourceProvider)
    {
        // 미리 메모리에 올려두기
        resourceProvider.GetAsset<GameObject>(ProjectileAddress);
        _resourceProvider = resourceProvider;
    }
    
    public override async UniTask<bool> Execute(EntityBase owner, EntityBase target)
    {
        try
        {
            var obj = await _resourceProvider.GetInstance(ProjectileAddress);
            if (obj.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(owner, effects);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"{nameof(SpawnProjectileEffect)} : {e}");
            throw;
        }
    }
}
