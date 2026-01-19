using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    public interface ISceneResourceProvider : IDisposable
    {
        UniTask<GameObject> GetInstance(string address);
        UniTask<T> GetAsset<T>(string address) where T : Object;
        T AddOrGetComponentWithInejct<T>(GameObject go) where T : Component;
        T AddComponentWithInejct<T>(GameObject go) where T : Component;
    }
}
