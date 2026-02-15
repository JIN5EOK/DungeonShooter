using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public interface ISkillObjectFactory
    {
        public UniTask<T> CreateSkillObjectAsync<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true) where T : SkillObjectBase;
    }
    public class SkillObjectFactory : ISkillObjectFactory
    {
        private ISceneResourceProvider _sceneResourceProvider;
        
        [Inject]
        public SkillObjectFactory(ISceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }
        
        public async UniTask<T> CreateSkillObjectAsync<T>(string key, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true) where T : SkillObjectBase
        {
            var go = await _sceneResourceProvider.GetInstanceAsync(key, position, rotation, parent, instantiateInWorldSpace);
            return ProcessSkillObject<T>(go);
        }

        public T ProcessSkillObject<T>(GameObject go) where T : SkillObjectBase
        {
            var skillComponent = go.AddOrGetComponent<T>();
            return skillComponent;
        }
    }
}