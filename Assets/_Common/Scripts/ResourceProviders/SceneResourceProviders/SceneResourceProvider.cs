using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
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
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        /// <summary>
        /// 주소에 해당하는 인스턴스를 생성하고 의존성 주입
        /// </summary>
        public async UniTask<GameObject> GetInstanceAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            await UniTask.SwitchToMainThread();
            var handle = _addressablesScope.InstantiateAsync(address, position, rotation, parent, instantiateInWorldSpace);
            await handle.Task;
            return GetInstanceInternal(handle, address);
        }

        /// <summary>
        /// 주소에 해당하는 인스턴스를 동기적으로 생성하고 의존성 주입
        /// </summary>
        public GameObject GetInstanceSync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var handle = _addressablesScope.InstantiateAsync(address, position,  rotation, parent, instantiateInWorldSpace);
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
                Debug.LogError($"[{nameof(SceneResourceProvider)}] 인스턴스 생성 실패: {address}");
                return null;
            }

            if (handle.Result == null)
            {
                Debug.LogError($"[{nameof(SceneResourceProvider)}] 인스턴스가 null입니다: {address}");
                return null;
            }

            _resolver?.InjectGameObject(handle.Result);
            return handle.Result;
        }

        /// <summary>
        /// 주소에 해당하는 에셋을 가져옵니다. T가 Sprite일 경우 주소를 스프라이트 단일 에셋 주소로 직접 로드합니다.
        /// </summary>
        public async UniTask<T> GetAssetAsync<T>(string address) where T : Object
        {
            await UniTask.SwitchToMainThread();
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            await handle.Task;
            return GetAssetInternal(handle, address);
        }

        /// <summary>
        /// 주소에 해당하는 에셋을 가져옵니다. T가 Sprite이고 atlasAddress가 비어있지 않으면 해당 아틀라스에서 스프라이트를 반환하고, 아니면 주소로 직접 로드합니다.
        /// </summary>
        public async UniTask<T> GetAssetAsync<T>(string address, string atlasAddress) where T : Object
        {
            if (typeof(T) == typeof(Sprite) && !string.IsNullOrEmpty(atlasAddress))
            {
                var sprite = await GetSpriteFromAtlasAsync(address, atlasAddress);
                return sprite != null ? (T)(Object)sprite : null;
            }

            return await GetAssetAsync<T>(address);
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
        /// 주소에 해당하는 에셋을 동기적으로 가져옵니다. T가 Sprite이고 atlasAddress가 비어있지 않으면 해당 아틀라스에서 스프라이트를 반환하고, 아니면 주소로 직접 로드합니다.
        /// </summary>
        public T GetAssetSync<T>(string address, string atlasAddress) where T : Object
        {
            if (typeof(T) == typeof(Sprite) && !string.IsNullOrEmpty(atlasAddress))
            {
                var sprite = GetSpriteFromAtlasSync(address, atlasAddress);
                return sprite != null ? (T)(Object)sprite : null;
            }

            return GetAssetSync<T>(address);
        }

        /// <summary>
        /// 에셋 로드 후 처리 (검증, 의존성 주입)
        /// </summary>
        private T GetAssetInternal<T>(AsyncOperationHandle<T> handle, string address) where T : Object
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[{nameof(SceneResourceProvider)}] 에셋 로드 실패: {address}");
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
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
            _addressablesScope?.Dispose();
        }
        
        /// <summary> 지정한 아틀라스 주소로 아틀라스를 로드한 뒤 해당 스프라이트 이름의 스프라이트를 반환합니다. </summary>
        private async UniTask<Sprite> GetSpriteFromAtlasAsync(string spriteName, string atlasAddress)
        {
            await UniTask.SwitchToMainThread();
            var handle = _addressablesScope.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            await handle.Task;
            return GetSpriteFromAtlasInternal(handle, spriteName, atlasAddress);
        }

        /// <summary> 지정한 아틀라스 주소로 아틀라스를 로드한 뒤 해당 스프라이트 이름의 스프라이트를 동기 반환합니다. </summary>
        private Sprite GetSpriteFromAtlasSync(string spriteName, string atlasAddress)
        {
            var handle = _addressablesScope.LoadAssetAsync<SpriteAtlas>(atlasAddress);
            handle.WaitForCompletion();
            return GetSpriteFromAtlasInternal(handle, spriteName, atlasAddress);
        }

        private Sprite GetSpriteFromAtlasInternal(AsyncOperationHandle<SpriteAtlas> handle, string spriteName, string atlasAddress)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[{nameof(SceneResourceProvider)}] 스프라이트 아틀라스 로드 실패: {atlasAddress}");
                return null;
            }

            var sprite = handle.Result.GetSprite(spriteName);
            if (sprite == null)
            {
                Debug.LogError($"[{nameof(SceneResourceProvider)}] 아틀라스 내 스프라이트를 찾을 수 없음: {spriteName}");
                return null;
            }

            _resolver?.Inject(sprite);
            return sprite;
        }

        /// <summary> 스프라이트 아틀라스 요청 이벤트 /// </summary>
        private void OnAtlasRequested(string tag, System.Action<SpriteAtlas> action)
        {
            LogHandler.Log<SceneResourceProvider>($"아틀라스 요청됨: {tag}");

            _addressablesScope.LoadAssetAsync<SpriteAtlas>(tag).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    action(handle.Result);
                }
            };
        }
    }
}
