using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 데미지를 주는 이펙트
    /// </summary>
    [System.Serializable]
    public class DamageEffect : EffectBase
    {
        [Header("데미지 설정")]
        public int damage;
        
        public override UniTask<bool> Execute(EntityBase target)
        {
            if (target.TryGetComponent(out HealthComponent health))
            {
                health.TakeDamage(damage);
                return UniTask.FromResult(true);
            }
            
            Debug.LogError($"{nameof(DamageEffect)} : 데미지 주기 실패");
            return UniTask.FromResult(false);
        }
    }
}
