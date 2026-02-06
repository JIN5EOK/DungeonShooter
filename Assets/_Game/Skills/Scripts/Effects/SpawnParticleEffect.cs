using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 파티클 오브젝트를 생성하고 재생하는 이펙트
    /// </summary>
    [Serializable]
    public class SpawnParticleEffect : EffectBase
    {
        [Header("파티클 프리팹")]
        [SerializeField]
        private AssetReferenceGameObject _particlePrefab;

        [Header("이펙트 소환 위치")]
        [SerializeField]
        private SkillOwner _spawnPosition;

        private string ParticlePrefabAddress => _particlePrefab.AssetGUID.ToString();

        private ISceneResourceProvider _resourceProvider;

        public override void Initialize(ISceneResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            try
            {
                var position = _spawnPosition == SkillOwner.LastHitTarget && context.LastHitTarget != null
                    ? context.LastHitTarget.transform.position
                    : context.Caster.transform.position;
                var obj = await _resourceProvider.GetInstanceAsync(ParticlePrefabAddress, position);

                var particleSystem = obj.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }

                return true;
            }
            catch (Exception e)
            {
                LogHandler.LogException<SpawnParticleEffect>(e, "파티클 오브젝트 생성 실패");
                return false;
            }
        }
    }
}
