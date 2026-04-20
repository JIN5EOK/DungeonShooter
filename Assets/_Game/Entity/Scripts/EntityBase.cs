using UnityEngine;

namespace DungeonShooter
{
    public class EntityBase : MonoBehaviour
    {
        public IEntityContext GetContext() => _entityContext;
        private IEntityContext _entityContext;
        /// <summary>
        /// 엔티티를 해제 혹은 제거합니다. PoolableComponent가 있으면 풀에 반환하고, 없으면 게임오브젝트를 파괴합니다.
        /// </summary>
        public void Release()
        {
            var poolable = GetComponent<PoolableComponent>();
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 팩토리에서 생성한 EntityContext를 주입합니다.
        /// </summary>
        public void SetContext(IEntityContext context)
        {
            _entityContext = context;
        }
    }
}
