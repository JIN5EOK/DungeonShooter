using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 장판 스킬 오브젝트를 소환하는 이펙트. 장판 안에 있는 대상에게 주기적으로 이펙트를 적용합니다.
    /// </summary>
    [Serializable]
    public class SpawnZoneEffect : EffectBase
    {
        [Header("스킬 시전 위치")]
        [SerializeField]
        private SkillOwner _spawnPosition;
        
        [Header("오브젝트 프리펩")]
        [SerializeField]
        private AssetReferenceGameObject _skillObject;

        [Header("장판 지속시간 (초)")]
        [SerializeField]
        [Min(0.01f)]
        private float _duration = 5f;

        [Header("이펙트 적용 주기 (초)")]
        [SerializeField]
        [Min(0.01f)]
        private float _applyInterval = 1f;

        [Header("장판 안에 있을 때 적용할 이펙트")]
        [SerializeReference]
        private List<EffectBase> _effects;

        private string SkillObjectAddress => _skillObject.AssetGUID.ToString();

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            var position = _spawnPosition == SkillOwner.Caster ? context.Caster.transform.position : context.LastHitTarget.transform.position;
            var obj = await context.SceneResourceProvider.GetInstanceAsync(SkillObjectAddress, position);
            
            if(obj == null)
                return false;
            
            var skillObj = obj.AddOrGetComponent<ZoneSkillObject>();
            skillObj.Initialize(_effects, entry, context, _duration, _applyInterval);

            return true;
        }
    }
}
