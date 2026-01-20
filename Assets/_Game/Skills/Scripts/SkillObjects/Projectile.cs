using System;
using System.Collections.Generic;
using DungeonShooter;
using UnityEngine;

/// <summary>
/// 투사체 인스턴스 프리팹
/// </summary>
public class Projectile : MonoBehaviour
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
    
    public void Initialize(EntityBase owner, List<EffectBase> effects)
    {
        _owner = owner;
        _effects = effects;
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
        transform.position += transform.right * _speed * Time.deltaTime;
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
            effect.Execute(otherEntity);
        }
        
        if (_destroyOnTrigger == true)
        {
            Destroy(gameObject);
        }
    }
}
