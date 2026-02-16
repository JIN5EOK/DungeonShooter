using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 투사체 인스턴스 프리팹
    /// </summary>
    public class ProjectileSkillObject : SkillObjectBase
    {
        private float _speed;
        private float _lifeTime;
        private int _targetCount;
        private bool _rotateToCastDirection;
        private float _elapsedTime = 0f;
        private readonly HashSet<EntityBase> _appliedTargets = new ();

        public void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context, int targetCount, float speed, float lifeTime)
        {
            base.Initialize(effects, skillTableEntry, context);

            _elapsedTime = 0f;
            _appliedTargets.Clear();
            _speed = speed;
            _lifeTime = lifeTime;
            _targetCount = targetCount;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            
            // 생명주기 체크
            if (_elapsedTime >= _lifeTime)
            {
                Release();
                return;
            }
            
            // 전진 이동
            transform.position += transform.right * _speed * Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_appliedTargets.Count >= _targetCount)
            {
                return;
            }

            var otherEntity = other.GetComponent<EntityBase>();

            if (otherEntity == null)
                return;

            if (context.Caster != null && context.Caster.gameObject.layer == other.gameObject.layer)
                return;

            if (_appliedTargets.Contains(otherEntity))
                return;

            _appliedTargets.Add(otherEntity);
            
            if (effects != null)
            {
                var newContext = context.WithLastHitTarget(otherEntity);
                RunEffectsAsync(newContext).Forget();
            }

            if (_appliedTargets.Count >= _targetCount)
            {
                Release();
            }
        }
    }
}
