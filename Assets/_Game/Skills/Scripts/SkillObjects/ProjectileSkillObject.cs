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

        private float _elapsedTime = 0f;
    
        private Vector2 _direction = Vector2.right;

        private int _appliedTargetCount = 0;

        public void Initialize(List<EffectBase> effects, SkillTableEntry skillTableEntry,
            SkillExecutionContext context, SkillOwner spawnPosition, int targetCount, float speed, float lifeTime)
        {
            base.Initialize(effects, skillTableEntry, context, spawnPosition);

            _speed = speed;
            _lifeTime = lifeTime;
            _targetCount = targetCount;

            // TODO: 이동 전략에 대한 커스텀 기능 필요
            if (context.Caster.TryGetComponent(out MovementComponent movement))
            {
                _direction = movement.LookDirection;
            }
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            
            // 생명주기 체크
            if (_elapsedTime >= _lifeTime)
            {
                Destroy(gameObject);
                return;
            }
            
            // 전진 이동
            transform.position += (Vector3)_direction * _speed * Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var otherEntity = other.GetComponent<EntityBase>();
            
            if(otherEntity == null)
                return;

            if (context.Caster.gameObject.layer == other.gameObject.layer)
                return;
            
            _appliedTargetCount++;
            if (effects != null)
            {
                var newContext = context
                    .WithLastHitTarget(otherEntity);
                RunEffectsAsync(newContext).Forget();
            }

            if (_appliedTargetCount >= _targetCount)
            {
                Destroy(gameObject);
            }
        }
    }
}
