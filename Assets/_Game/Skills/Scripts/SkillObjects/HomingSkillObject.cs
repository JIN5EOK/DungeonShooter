using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 지정한 타겟을 추적하다가, 타겟에 도달하면 이펙트를 실행하는 스킬 오브젝트. (메테오 등)
    /// </summary>
    public class HomingSkillObject : SkillObjectBase
    {
        private EntityBase _target;
        private float _speed;
        private float _lifeTime;
        private float _hitRadius;
        private float _elapsedTime;
        private bool _effectExecuted;

        /// <summary>
        /// 타겟 추적 스킬 오브젝트를 초기화합니다.
        /// </summary>
        /// <param name="target">추적할 대상</param>
        /// <param name="speed">이동 속도</param>
        /// <param name="lifeTime">수명 (초)</param>
        /// <param name="hitRadius">이 거리 이내 접근 시 도달로 간주</param>
        public void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context, EntityBase target, float speed, float lifeTime, float hitRadius)
        {
            base.Initialize(effects, skillTableEntry, context);
            _target = target;
            _speed = speed;
            _lifeTime = lifeTime;
            _hitRadius = hitRadius;
            _elapsedTime = 0f;
            _effectExecuted = false;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _lifeTime)
            {
                Destroy(gameObject);
                return;
            }

            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            var current = (Vector2)transform.position;
            var toTarget = (Vector2)_target.transform.position - current;
            var distance = toTarget.magnitude;

            if (distance <= _hitRadius && !_effectExecuted)
            {
                ExecuteEffectAndDestroy();
                return;
            }

            if (distance > 0.001f)
            {
                var move = toTarget.normalized * (_speed * Time.deltaTime);
                if (move.magnitude >= distance)
                {
                    transform.position = _target.transform.position;
                    ExecuteEffectAndDestroy();
                    return;
                }
                transform.position += (Vector3)move;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_effectExecuted) return;
            if (_target == null) return;

            var otherEntity = other.GetComponent<EntityBase>();
            if (otherEntity != _target) return;

            if (context.Caster != null && context.Caster.gameObject.layer == other.gameObject.layer)
                return;

            ExecuteEffectAndDestroy();
        }

        private void ExecuteEffectAndDestroy()
        {
            if (_effectExecuted) return;
            _effectExecuted = true;

            if (effects != null && _target != null)
            {
                var newContext = context.WithLastHitTarget(_target);
                RunEffectsAsync(newContext).Forget();
            }

            Destroy(gameObject);
        }
    }
}
