using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    public interface ISceneResourceProvider : IDisposable
    {
        UniTask<GameObject> GetInstanceAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<T> GetAssetAsync<T>(string address) where T : Object;
        UniTask<T> GetAssetAsync<T>(string address, string atlasAddress) where T : Object;

        GameObject GetInstanceSync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        T GetAssetSync<T>(string address) where T : Object;
        T GetAssetSync<T>(string address, string atlasAddress) where T : Object;
        
        T AddOrGetComponentWithInejct<T>(GameObject go) where T : Component;
        T AddComponentWithInejct<T>(GameObject go) where T : Component;
    }
}
