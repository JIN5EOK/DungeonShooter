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

        [Header("스킬 오브젝트 적중시 효과")]
        [SerializeReference]
        private List<EffectBase> _effects = new List<EffectBase>();

        private string SkillObjectAddress => _skillObject.AssetGUID.ToString();
        private ISceneResourceProvider _resourceProvider;

        [Inject]
        public void Construct(ISceneResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
            _resourceProvider.GetAssetAsync<GameObject>(SkillObjectAddress);
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
