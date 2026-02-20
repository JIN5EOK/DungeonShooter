using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 씬과 생명주기를 함께하는 에셋이나 인스턴스를 제공
    /// </summary>
    public interface ISceneResourceProvider : IDisposable
    {
        UniTask<GameObject> GetInstanceAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<T> GetAssetAsync<T>(string address) where T : Object;
        UniTask<T> GetAssetAsync<T>(string address, string atlasAddress) where T : Object;

        GameObject GetInstanceSync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        T GetAssetSync<T>(string address) where T : Object;
        T GetAssetSync<T>(string address, string atlasAddress) where T : Object;
    }
}
