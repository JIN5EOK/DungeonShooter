using UnityEngine;
using VContainer.Unity;

namespace DungeonShooter
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // GlobalLifeTimeScope 생성 및 초기화
            var globalScopeObject = new GameObject("GlobalLifeTimeScope");
            globalScopeObject.AddComponent<GlobalLifeTimeScope>();
        }       
    }
}