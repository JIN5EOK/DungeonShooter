using System;
using UnityEngine;

namespace DungeonShooter
{
    public class EntityBase : MonoBehaviour
    {
        public IEntityContext GetContext() => _entityContext;
        private IEntityContext _entityContext;

        /// <summary>
        /// 팩토리에서 생성한 EntityContext를 주입합니다.
        /// </summary>
        public void SetContext(IEntityContext context)
        {
            _entityContext = context;
        }
    }
}
