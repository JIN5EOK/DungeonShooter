using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DungeonShooter
{
    /// <summary>
    /// 범위 내 대상을 찾아, 대상마다 타겟 추적 스킬 오브젝트를 소환하는 이펙트. (메테오 등)
    /// </summary>
    [Serializable]
    public class SpawnMeteorEffect : EffectBase
    {
        [Header("타겟 검색 중심")]
        [SerializeField]
        private SkillOwner _searchCenter = SkillOwner.Caster;

        [Header("검색 반경")]
        [SerializeField]
        [Min(0.01f)]
        private float _searchRadius = 5f;

        [Header("최대 타겟 수")]
        [SerializeField]
        [Min(1)]
        private int _maxTargetCount = 5;

        [Header("스킬 오브젝트 프리팹")]
        [SerializeField]
        private AssetReferenceGameObject _skillObject;

        [Header("스킬 오브젝트 이동 속도")]
        [SerializeField]
        [Min(0.01f)]
        private float _speed = 10f;

        [Header("스킬 오브젝트 수명 (초)")]
        [SerializeField]
        [Min(0.01f)]
        private float _lifeTime = 10f;

        [Header("타겟 도달 판정 거리")]
        [SerializeField]
        [Min(0.01f)]
        private float _hitRadius = 0.5f;

        [Header("스킬 오브젝트 스폰 오프셋 (타겟 기준 상대 위치, 예: 위쪽 스폰)")]
        [SerializeField]
        private Vector2 _spawnOffset = new Vector2(0f, 5f);

        [Header("타겟 도달 시 실행할 이펙트")]
        [SerializeReference]
        private List<EffectBase> _effects;

        private string SkillObjectAddress => _skillObject.AssetGUID.ToString();

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if(!await base.Execute(context, entry))
                return false;

            var center = _searchCenter == SkillOwner.LastHitTarget && context.LastHitTarget != null ? (Vector2)context.LastHitTarget.transform.position : 
                (Vector2)context.Caster.transform.position;

            var targets = FindTargetsInRange(center, context);

            var isSucceed = true;
            
            foreach (var target in targets)
            {
                var spawnPosition = (Vector2)target.transform.position + _spawnOffset;
                var skillObj = await context.SkillObjectFactory.CreateSkillObjectAsync<HomingSkillObject>(SkillObjectAddress, spawnPosition, Quaternion.identity);

                if (skillObj == null)
                    return false;
                
                context = context.WithLastHitTarget(target);
                skillObj.Initialize(_effects, entry, context, target, _speed, _lifeTime, _hitRadius);
            }

            return true;
        }

        /// <summary>
        /// 범위 내에서 시전자와 다른 진영의 엔티티를 거리순으로 찾아 최대 개수만큼 반환합니다.
        /// </summary>
        private List<EntityBase> FindTargetsInRange(Vector2 center, SkillExecutionContext context)
        {
            var casterLayer = context.Caster.gameObject.layer;
            var colliders = Physics2D.OverlapCircleAll(center, _searchRadius);

            var candidates = new List<EntityBase>();
            foreach (var col in colliders)
            {
                var entity = col.GetComponent<EntityBase>();
                
                if (entity == null || entity.gameObject.layer == casterLayer)
                    continue;
                
                candidates.Add(entity);
            }

            candidates.Sort((a, b) =>
            {
                var distA = Vector2.SqrMagnitude((Vector2)a.transform.position - center);
                var distB = Vector2.SqrMagnitude((Vector2)b.transform.position - center);
                return distA.CompareTo(distB);
            });

            var maxTake = Mathf.Min(_maxTargetCount, candidates.Count);
            var list = new List<EntityBase>(maxTake);
            for (var i = 0; i < maxTake; i++)
            {
                list.Add(candidates[i]);
            }

            return list;
        }
    }
}
