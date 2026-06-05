using UnityEngine;

namespace DungeonShooter
{
    public static class Bootstrap
    {
        private static bool IsDevelopmentBuild()
        {
#if DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }

        private static bool IsEditor() => Application.isEditor;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (IsDevelopmentBuild() || IsEditor())
            {
                FPSOverlayInit(); 
            }
            
            SetFrameRateLimit();
        }

        private static void FPSOverlayInit()
        {
            var fpsOverlay = new GameObject(nameof(FpsOverlay));
            fpsOverlay.AddComponent<FpsOverlay>();
            Object.DontDestroyOnLoad(fpsOverlay.gameObject);
        }

        private static void SetFrameRateLimit()
        {
            Application.targetFrameRate = Constants.TargetFrameRate;       
        }
    }
}