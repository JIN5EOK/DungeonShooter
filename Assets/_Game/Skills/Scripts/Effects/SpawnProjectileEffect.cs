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

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            try
            { 
                var position = _spawnPosition == SkillOwner.Caster ? context.Caster.transform.position : context.LastHitTarget.transform.position;
                Quaternion rotation = Quaternion.identity;
                if (_rotateToCastDirection)
                {
                    if (context.Caster.TryGetComponent(out MovementComponent moveComp))
                    {
                        var direction = moveComp.LookDirection;
                        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        rotation = Quaternion.Euler(0f, 0f, angle);    
                    }
                }
                var obj = await context.ResourceProvider.GetInstanceAsync(SkillObjectAddress, position, rotation);
                var skillObj = obj.AddOrGetComponent<ProjectileSkillObject>();
                
                skillObj.Initialize(_effects, entry, context, targetCount, speed, lifeTime, _rotateToCastDirection);

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
