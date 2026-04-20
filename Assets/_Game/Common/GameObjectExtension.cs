using UnityEngine;

namespace DungeonShooter
{
    public static class GameObjectExtension
    {
        /// <summary>
        /// PoolableComponent가 있으면 풀에 반환하고, 없으면 게임오브젝트를 파괴합니다.
        /// </summary>
        public static void ReleaseOrDestroy(this Component component)
        {
            var poolable = component.GetComponent<PoolableComponent>();
            if (poolable != null)
                poolable.Release();
            else
                Object.Destroy(component.gameObject);
        }
        
        public static void ReleaseOrDestroy(this GameObject component)
        {
            var poolable = component.GetComponent<PoolableComponent>();
            if (poolable != null)
                poolable.Release();
            else
                Object.Destroy(component.gameObject);
        }
    }
}
