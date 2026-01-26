using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DungeonShooter
{
    public class SceneStartInjector : IStartable
    {
        private IObjectResolver _resolver;

        [Inject]
        public SceneStartInjector(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Start()
        {
            Debug.Log(nameof(SceneStartInjector) + " Start");
            foreach (var obj in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                _resolver.Inject(obj);
            }
        }
    }
}