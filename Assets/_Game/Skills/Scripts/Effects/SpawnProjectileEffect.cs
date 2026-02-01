using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 오브젝트(투사체, 장판 등)를 소환하는 이펙트
    /// </summary>
    [Serializable]
    public class SpawnProjectileEffect : EffectBase
    {
        [Header("스킬 시전 위치")]
        [SerializeField]
        private SkillOwner _spawnPosition;
        
        [Header("오브젝트 프리팹")]
        [SerializeField]
        private AssetReferenceGameObject _skillObject;

        [Header("투사체 비행속도")] 
        [SerializeField] 
        private float speed;
        
        [Header("투사체 소멸시간")] 
        [SerializeField] 
        private float lifeTime;
        
        [Header("적용 타겟 수")] 
        [Range(1, 99)]
        [SerializeField] 
        private int targetCount = 1;

        [Header("투사체가 시전 방향을 바라보도록 회전 여부")]
        [SerializeField]
        private bool _rotateToCastDirection;

        [Header("스킬 오브젝트 적중시 효과")]
        [SerializeReference]
        private List<EffectBase> _effects;
        
        private string SkillObjectAddress => _skillObject.AssetGUID.ToString();

        protected ISceneResourceProvider _resourceProvider;
        public override void Initialize(ISceneResourceProvider resourceProvider)
        {
            if (_effects == null)
                return;

            _resourceProvider = resourceProvider;
            foreach (var effect in _effects)
            {
                effect.Initialize(resourceProvider);
            }
        }
        
        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            try
            { 
                var obj = await _resourceProvider.GetInstanceAsync(SkillObjectAddress);

                var skillObj = obj.AddOrGetComponent<ProjectileSkillObject>();
                skillObj.Initialize(_effects, entry, context, _spawnPosition, targetCount, speed, lifeTime, _rotateToCastDirection);

                return false;
            }
            catch (Exception e)
            {
                LogHandler.LogException<SpawnProjectileEffect>(e, "스킬 오브젝트 생성 실패");
            }

            return false;
        }
    }
}
