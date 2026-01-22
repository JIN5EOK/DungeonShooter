using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 로그 사용시 로그 출력 주체 지정과 에디터,디버깅 환경에서만 로그를 호출하기 위한 핸들러
    /// </summary>
    public static class LogHandler
    {
        public static bool IsUseLog
        {
            get
            {
#if DEBUG || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static void Log<T>(string message)
        {
            if (!IsUseLog) 
                return;
            Debug.Log(GetMessage<T>(message));
        }
        
        public static void LogWarning<T>(string message)
        {
            if (!IsUseLog) 
                return;
            Debug.LogWarning(GetMessage<T>(message));
        }
        
        public static void LogError<T>()
        {
            if (!IsUseLog) 
                return;
            Debug.LogError(GetMessage<T>("에러가 발생했습니다."));
        }
        
        public static void LogError<T>(string message)
        {
            if (!IsUseLog) 
                return;
            Debug.LogError(GetMessage<T>(message));
        }

        private static string GetMessage<T>(string message) => $"[{typeof(T).Name}] {message}";
    }
}