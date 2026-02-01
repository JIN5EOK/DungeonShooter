using System;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        [Header("생성 위치")]
        [SerializeField]
        private SkillOwner _spawnPosition;

        [Header("재생 후 자동 파괴 시간 (초, 0이면 자동 파괴 안 함)")]
        [SerializeField]
        [Min(0f)]
        private float _destroyAfterSeconds;

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
                var obj = await _resourceProvider.GetInstanceAsync(ParticlePrefabAddress);

                var position = _spawnPosition == SkillOwner.LastHitTarget && context.LastHitTarget != null
                    ? context.LastHitTarget.transform.position
                    : context.Caster.transform.position;
                obj.transform.position = position;

                var particleSystem = obj.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }

                if (_destroyAfterSeconds > 0f)
                {
                    UnityEngine.Object.Destroy(obj, _destroyAfterSeconds);
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
