using UnityEngine;

namespace DungeonShooter
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            DevelopmentInit();
        }

        public static void DevelopmentInit()
        {
            // 개발빌드 혹은 에디터 환경에서만 띄우기
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
                var fpsOverlay = new GameObject(nameof(FpsOverlay));
                fpsOverlay.AddComponent<FpsOverlay>();
                Object.DontDestroyOnLoad(fpsOverlay.gameObject);
            #endif
        }
    }
}