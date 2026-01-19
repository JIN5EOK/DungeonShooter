using System.Collections.Generic;
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

    public void Initialize(EntityBase owner, List<EffectBase> effects)
    {
        _owner = owner;
        _effects = effects;
    }
    // TODO: 구현 예정
}
