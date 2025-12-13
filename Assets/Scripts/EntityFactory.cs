using Jin5eok;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace DungeonShooter
{
    public class EntityFactory
    {
        private AddressablesScope _addressablesScope;
        // LifeTimeScope의 Resolver, 생성한 오브젝트의 의존성 주입시 사용
        private IObjectResolver _resolver;

        [Inject]
        public EntityFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
            _addressablesScope = new AddressablesScope();
        }

        public async Awaitable<GameObject> Create(string key)
        {
            var handle = _addressablesScope.LoadAssetAsync<GameObject>(key);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _resolver.Inject(handle.Result);
                return handle.Result;
            }
            return null;
        }
    }
}