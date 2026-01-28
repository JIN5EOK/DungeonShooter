using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 씬에서 사용할 리소스를 제공하는 기본 구현 클래스
    /// </summary>
    public class SceneResourceProvider : ISceneResourceProvider
    {
        protected readonly AddressablesScope _addressablesScope;
        protected readonly IObjectResolver _resolver;
        
        [Inject]
        public SceneResourceProvider(IObjectResolver resolver)
        {
            _addressablesScope = new AddressablesScope();
            _resolver = resolver;
        }
        
        /// <summary>
        /// 주소에 해당하는 인스턴스를 생성하고 의존성 주입
        /// </summary>
        public async UniTask<GameObject> GetInstanceAsync(string address)
        {
            await UniTask.SwitchToMainThread();
            var handle = _addressablesScope.InstantiateAsync(address);
            await handle.Task;
            return GetInstanceInternal(handle, address);
        }

        /// <summary>
        /// 주소에 해당하는 인스턴스를 동기적으로 생성하고 의존성 주입
        /// </summary>
        public GameObject GetInstanceSync(string address)
        {
            var handle = _addressablesScope.InstantiateAsync(address);
            handle.WaitForCompletion();
            return GetInstanceInternal(handle, address);
        }

        /// <summary>
        /// 인스턴스 생성 후 처리 (검증, 의존성 주입)
        /// </summary>
        private GameObject GetInstanceInternal(AsyncOperationHandle<GameObject> handle, string address)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProvider)}] 인스턴스 생성 실패: {address}");
                return null;
            }

            if (handle.Result == null)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProvider)}] 인스턴스가 null입니다: {address}");
                return null;
            }

            _resolver?.InjectGameObject(handle.Result);
            return handle.Result;
        }

        /// <summary>
        /// 주소에 해당하는 에셋을 가져옵니다.
        /// </summary>
        public async UniTask<T> GetAssetAsync<T>(string address) where T : Object
        {
            await UniTask.SwitchToMainThread();
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            await handle.Task;
            return GetAssetInternal(handle, address);
        }

        /// <summary>
        /// 주소에 해당하는 에셋을 동기적으로 가져옵니다.
        /// </summary>
        public T GetAssetSync<T>(string address) where T : Object
        {
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            handle.WaitForCompletion();
            return GetAssetInternal(handle, address);
        }

        /// <summary>
        /// 에셋 로드 후 처리 (검증, 의존성 주입)
        /// </summary>
        private T GetAssetInternal<T>(AsyncOperationHandle<T> handle, string address) where T : Object
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProvider)}] 에셋 로드 실패: {address}");
                return null;
            }

            if (handle.Result != null)
            {
                _resolver?.Inject(handle.Result);
            }

            return handle.Result;
        }

        /// <summary>
        /// 컴포넌트를 가져오거나 추가하고 의존성 주입
        /// </summary>
        public T AddOrGetComponentWithInejct<T>(GameObject go) where T : Component
        {
            if (go.TryGetComponent(out T comp))
            {
                return comp;
            }
            return AddComponentWithInejct<T>(go);
        }

        /// <summary>
        /// 컴포넌트를 추가하고 의존성 주입
        /// </summary>
        public T AddComponentWithInejct<T>(GameObject go) where T : Component
        {
            var comp = go.AddComponent<T>();
            _resolver?.Inject(comp);
            return comp;
        }

        public void Dispose()
        {
            _addressablesScope?.Dispose();
        }
    }
}
