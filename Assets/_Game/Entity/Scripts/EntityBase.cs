using System;
using UnityEngine;

namespace DungeonShooter
{
    public class EntityBase : MonoBehaviour
    {
        private IEntityContext _entityContext;
        public IEntityContext GetContext() => _entityContext;
        public void SetContext(IEntityContext context) => _entityContext = context;

        protected virtual void Awake() { }
    }
}
