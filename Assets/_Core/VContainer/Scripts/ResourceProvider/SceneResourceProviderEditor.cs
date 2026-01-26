#if UNITY_EDITOR
using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 에디터에서 사용하는 ResourceProvider
    /// 의존성 주입 없이 Addressables를 직접 사용하여 작동
    /// </summary>
    [Serializable]
    public class SceneResourceProviderEditor : ISceneResourceProvider
    {
        private readonly AddressablesScope _addressablesScope;
        
        public SceneResourceProviderEditor()
        {
            _addressablesScope = new AddressablesScope();
        }
        
        /// <summary>
        /// 주소에 해당하는 에셋을 동기적으로 가져옵니다.
        /// </summary>
        public T GetAssetSync<T>(string address) where T : Object
        {
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProviderEditor)}] 에셋 로드 실패: {address}");
                return null;
            }

            return handle.Result;
        }

        /// <summary>
        /// 주소에 해당하는 인스턴스를 동기적으로 생성합니다.
        /// 에디터에서는 프리팹 연결을 유지하기 위해 PrefabUtility.InstantiatePrefab을 사용합니다.
        /// </summary>
        public GameObject GetInstanceSync(string address)
        {
            // 에디터에서는 프리팹 에셋을 로드한 후 PrefabUtility로 인스턴스화
            var prefabHandle = _addressablesScope.LoadAssetAsync<GameObject>(address);
            prefabHandle.WaitForCompletion();

            if (prefabHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProviderEditor)}] 프리팹 로드 실패: {address}");
                return null;
            }

            var prefab = prefabHandle.Result;
            if (prefab == null)
            {
                Debug.LogWarning($"[{nameof(SceneResourceProviderEditor)}] 프리팹이 null입니다: {address}");
                return null;
            }

            // PrefabUtility.InstantiatePrefab을 사용하여 프리팹 연결 유지
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            return instance;
        }
        
        public void Dispose()
        {
            _addressablesScope?.Dispose();
        }
        
        public UniTask<GameObject> GetInstanceAsync(string address) => throw new NotImplementedException();
        public UniTask<T> GetAssetAsync<T>(string address) where T : Object => throw new NotImplementedException();
        public T AddOrGetComponentWithInejct<T>(GameObject go) where T : Component => throw new NotImplementedException();
        public T AddComponentWithInejct<T>(GameObject go) where T : Component => throw new NotImplementedException();
    }
}
#endif
