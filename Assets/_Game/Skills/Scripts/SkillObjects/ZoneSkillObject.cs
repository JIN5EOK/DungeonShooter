using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 장판 스킬 오브젝트. 트리거 안에 있는 대상에게 주기적으로 이펙트를 적용합니다.
    /// </summary>
    public class ZoneSkillObject : SkillObjectBase
    {
        private float _duration = 5f;
        private float _applyInterval = 1f;
        private float _elapsedTime;
        private float _nextApplyTime;
        private readonly List<EntityBase> _entitiesInside = new List<EntityBase>();

        /// <summary>
        /// 장판 스킬 오브젝트를 초기화합니다.
        /// </summary>
        public void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context, float duration, float applyInterval)
        {
            base.Initialize(effects, skillTableEntry, context);

            _duration = duration;
            _applyInterval = Mathf.Max(0.01f, applyInterval);
            _elapsedTime = 0f;
            _nextApplyTime = _applyInterval;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _duration)
            {
                Destroy(gameObject);
                return;
            }

            if (_elapsedTime >= _nextApplyTime)
            {
                if (_entitiesInside.Count > 0)
                {
                    ApplyEffectsToTargetsInside();
                }
                _nextApplyTime += _applyInterval;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var otherEntity = other.GetComponent<EntityBase>();
            if (otherEntity == null)
                return;

            if (context.Caster != null && context.Caster.gameObject.layer == other.gameObject.layer)
                return;

            if (!_entitiesInside.Contains(otherEntity))
            {
                _entitiesInside.Add(otherEntity);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var otherEntity = other.GetComponent<EntityBase>();
            if (otherEntity != null)
            {
                _entitiesInside.Remove(otherEntity);
            }
        }

        private void ApplyEffectsToTargetsInside()
        {
            for (var i = _entitiesInside.Count - 1; i >= 0; i--)
            {
                var entity = _entitiesInside[i];
                if (entity == null)
                {
                    _entitiesInside.RemoveAt(i);
                    continue;
                }

                var newContext = context.WithLastHitTarget(entity);
                RunEffectsAsync(newContext).Forget();
            }
        }
    }
}
