using System;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 오브젝트 풀링되는 게임오브젝트에 부착하는 컴포넌트.
    /// </summary>
    public class PoolableComponent : MonoBehaviour
    {
        /// <summary>풀 식별용 키. 팩토리에서 반환 시 어떤 풀에 넣을지 결정하는 데 사용한다. </summary>
        public string PoolKey { get; set; }

        /// <summary>Release() 호출 시 발생. 팩토리는 이 이벤트에 풀 반환 로직을 등록한다. </summary>
        public event Action<PoolableComponent> OnRelease;

        /// <summary>OnRelease가 등록되어 있으면 풀에 반환, 없으면 게임오브젝트를 파괴한다. </summary>
        public void Release()
        {
            if (OnRelease != null)
            {
                OnRelease.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
