using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 투사체를 소환하는 이펙트
    /// </summary>
    [System.Serializable]
    public class SpawnProjectileEffect : EffectBase
    {
    [Header("투사체 프리팹")]
    [SerializeField]
    public AssetReferenceGameObject projectile;
    
    [Header("투사체 적중시 효과")]
    [SerializeReference]
    public List<EffectBase> effects = new List<EffectBase>();

    private string ProjectileAddress => projectile.AssetGUID.ToString();
    private ISceneResourceProvider _resourceProvider;
    
    [Inject]
    public void Construct(ISceneResourceProvider resourceProvider)
    {
        // 미리 메모리에 올려두기
        _resourceProvider = resourceProvider;
        resourceProvider.GetAssetAsync<GameObject>(ProjectileAddress);
    }
    
    public override async UniTask<bool> Execute(EntityBase target, SkillTableEntry entry)
    {
        try
        {
            var obj = await _resourceProvider.GetInstanceAsync(ProjectileAddress);
            if (obj.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(target, effects, entry);
            }

            return true;
        }
        catch (Exception e)
        {
            LogHandler.LogException<SpawnProjectileEffect>(e, "투사체 생성 실패");
        }

        return false;
    }
    }
}
