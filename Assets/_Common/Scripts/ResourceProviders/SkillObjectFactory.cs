using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public interface ISkillObjectFactory
    {
        UniTask<T> CreateSkillObjectAsync<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true) where T : SkillObjectBase;
    }

    public class SkillObjectFactory : ISkillObjectFactory
    {
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly GameObjectPool _pool = new();

        [Inject]
        public SkillObjectFactory(ISceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }

        public async UniTask<T> CreateSkillObjectAsync<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true) where T : SkillObjectBase
        {
            var poolKey = GetPoolKey<T>(key);
            var go = _pool.Get(poolKey);
            if (go != null)
            {
                ApplyTransform(go.transform, position, rotation, parent, instantiateInWorldSpace);
                go.SetActive(true);
            }
            else
            {
                go = await _sceneResourceProvider.GetInstanceAsync(key, position, rotation, parent,
                    instantiateInWorldSpace);
                
                EnsurePoolable(go, poolKey);
            }

            return ProcessSkillObject<T>(go);
        }

        private string GetPoolKey<T>(string key) where T : SkillObjectBase
        {
            return $"{typeof(T).FullName}:{key}";
        }
        
        
        private static void ApplyTransform(Transform transform, Vector3 position, Quaternion rotation, Transform parent, bool instantiateInWorldSpace)
        {
            if (parent != null)
                transform.SetParent(parent, instantiateInWorldSpace);

            if (instantiateInWorldSpace)
            {
                transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }

        private void EnsurePoolable(GameObject go, string poolKey)
        {
            var poolable = go.AddOrGetComponent<PoolableComponent>();
            poolable.PoolKey = poolKey;
            poolable.OnRelease -= ReturnToPool;
            poolable.OnRelease += ReturnToPool;
        }

        private void ReturnToPool(PoolableComponent poolable)
        {
            if (poolable != null && !string.IsNullOrEmpty(poolable.PoolKey))
            {
                _pool.Return(poolable.PoolKey, poolable.gameObject);
            }
        }

        private T ProcessSkillObject<T>(GameObject go) where T : SkillObjectBase
        {
            var skillComponent = go.AddOrGetComponent<T>();
            return skillComponent;
        }
    }
}