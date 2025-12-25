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
            var globalScope = globalScopeObject.AddComponent<GlobalLifeTimeScope>();
            
            // 전역 스코프를 부모 스코프로 설정
            LifetimeScope.EnqueueParent(globalScope);
        }       
    }
}