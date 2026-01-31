using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 투사체 인스턴스 프리팹
    /// </summary>
    public class ProjectileSkillObject : SkillObjectBase
    {
        private EntityBase _owner;
        private List<EffectBase> _effects = new List<EffectBase>();
        
        [Header("투사체 설정")]
        [SerializeField] private float _speed;
        [SerializeField] private float _lifeTime;
        [SerializeField] private float _triggerStartTime;
        [SerializeField] private float _triggerEndTime;
        [SerializeField] private bool _destroyOnTrigger;
        [SerializeField] private bool _applyToOpponent;
        [SerializeField] private bool _applyToFriend;

        private bool _stopTrigger = false;
        private float _elapsedTime = 0f;
        private SkillTableEntry _skillTableEntry;
        private Vector2 _direction;
        public override void Initialize(EntityBase owner, List<EffectBase> effects, SkillTableEntry skillTableEntry)
        {
            _owner = owner;
            _effects = effects;
            _skillTableEntry = skillTableEntry;
            // TODO: 이동 전략에 대한 커스텀 기능 필요
            if (_owner.TryGetComponent(out MovementComponent movement))
            {
                _direction = movement.LookDirection;
            }
            transform.position = _owner.transform.position;
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
            
            // 충돌 가능 시간 체크
            if (_elapsedTime < _triggerStartTime || _elapsedTime >= _triggerEndTime)
            {
                _stopTrigger = true;
            }
            else
            {
                _stopTrigger = false;
            }
            
            // 전진 이동
            transform.position += (Vector3)_direction * _speed * Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_stopTrigger == true)
                return;
            
            var otherEntity = other.GetComponent<EntityBase>();
            
            if(otherEntity == null)
                return;
            
            var ownerLayer = _owner.gameObject.layer;
            var otherLayer = other.gameObject.layer;

            // 아군에게 적용 체크
            if (ownerLayer == otherLayer && !_applyToFriend)
                return;

            // 상대편에게 적용 체크
            if (ownerLayer != otherLayer && !_applyToOpponent)
                return;

            foreach (var effect in _effects)
            {
                effect.Execute(otherEntity, _skillTableEntry);
            }
            
            if (_destroyOnTrigger == true)
            {
                Destroy(gameObject);
            }
        }
    }
}
