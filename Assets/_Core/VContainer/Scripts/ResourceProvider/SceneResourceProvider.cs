using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        public SceneResourceProvider(IObjectResolver resolver)
        {
            _addressablesScope = new AddressablesScope();
            _resolver = resolver;
        }

        /// <summary>
        /// 주소에 해당하는 인스턴스를 생성하고 의존성 주입
        /// </summary>
        public async UniTask<GameObject> GetInstance(string address)
        {
            var handle = _addressablesScope.InstantiateAsync(address);
            await handle.Task;

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
        public async UniTask<T> GetAsset<T>(string address) where T : Object
        {
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            await handle.Task;

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
