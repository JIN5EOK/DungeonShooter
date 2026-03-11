using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 리소스를 비동기 또는 동기적으로 로드하거나 인스턴스화하는 기능 제공
    /// </summary>
    public interface IResourceProvider
    {
        UniTask<GameObject> GetInstanceAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<T> GetAssetAsync<T>(string address) where T : Object;
        UniTask<T> GetAssetAsync<T>(string address, string atlasAddress) where T : Object;
        GameObject GetInstanceSync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        T GetAssetSync<T>(string address) where T : Object;
        T GetAssetSync<T>(string address, string atlasAddress) where T : Object;
    }
    
    public interface IGlobalResourceProvider : IResourceProvider { }
    public interface ISceneResourceProvider : IResourceProvider { }
    public interface IFeatureResourceProvider : IResourceProvider { }
}
