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
    /// 스킬 오브젝트(투사체, 장판 등)를 소환하는 이펙트
    /// </summary>
    [Serializable]
    public class SpawnSkillObjectEffect : EffectBase
    {
        [Header("스킬 오브젝트 프리팹")]
        [SerializeField]
        private AssetReferenceGameObject _skillObject;

        [Header("스킬 오브젝트 생성 위치")]
        [SerializeField]
        private SkillOwner _spawnPosition;
        
        [Header("스킬 오브젝트 적중시 효과")]
        [SerializeReference]
        private List<EffectBase> _effects;
        
        private string SkillObjectAddress => _skillObject.AssetGUID.ToString();

        public override void Initialize(ISceneResourceProvider resourceProvider)
        {
            base.Initialize(resourceProvider);

            if(_effects == null)
                return;

            foreach (var effect in _effects)
            {
                effect.Initialize(resourceProvider);
            }
        }
        
        public override async UniTask<bool> Execute(EntityBase owner, SkillTableEntry entry)
        {
            try
            {
                var obj = await _resourceProvider.GetInstanceAsync(SkillObjectAddress);
                if (obj.TryGetComponent(out SkillObjectBase skillObj))
                {
                    skillObj.Initialize(owner, _effects, entry);
                    return true;
                }

                LogHandler.LogError<SpawnSkillObjectEffect>("스킬 오브젝트에 SkillObjectBase가 없습니다.");
                return false;
            }
            catch (Exception e)
            {
                LogHandler.LogException<SpawnSkillObjectEffect>(e, "스킬 오브젝트 생성 실패");
            }

            return false;
        }
    }
}
