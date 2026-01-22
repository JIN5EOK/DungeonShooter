using System;
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

        /// <summary>
        /// Log을 출력합니다.
        /// </summary>
        /// <typeparam name="T">클래스 타입</typeparam>
        public static void Log<T>(string message = "")
        {
            if (!IsUseLog)
                return;

            Log(typeof(T).Name, message);
        }
        
        /// <summary>
        /// Log을 출력합니다.
        /// </summary>
        /// <param name="header">로그 헤더 (클래스 이름 사용 권장)</param>
        public static void Log(string header, string message = "")
        {
            if (!IsUseLog) 
                return;

            message = string.IsNullOrEmpty(message) ? "디버깅 로그 발생." : message;
            Debug.Log(GetMessage(header, message));
        }
        
        /// <summary>
        /// LogWarning을 출력합니다.
        /// </summary>
        /// <typeparam name="T">클래스 타입</typeparam>
        public static void LogWarning<T>(string message = "")
        {
            if (!IsUseLog) 
                return;

            LogWarning(typeof(T).Name, message);
        }
        
        /// <summary>
        /// LogWarning을 출력합니다.
        /// </summary>
        /// <param name="header">로그 헤더 (클래스 이름 사용 권장)</param>
        public static void LogWarning(string header, string message = "")
        {
            if (!IsUseLog) 
                return;

            message = string.IsNullOrEmpty(message) ? "경고 발생." : message;
            Debug.LogWarning(GetMessage(header, message));
        }
        
        /// <summary>
        /// LogError를 출력합니다.
        /// </summary>
        /// <typeparam name="T">클래스 타입</typeparam>
        public static void LogError<T>(string message = "")
        {
            if (!IsUseLog) 
                return;

            Debug.LogError(GetMessage<T>(message));
        }

        /// <summary>
        /// LogError를 출력합니다.
        /// </summary>
        /// <param name="header">로그 헤더 (클래스 이름 사용 권장)</param>
        public static void LogError(string header, string message = "")
        {
            if (!IsUseLog) 
                return;

            message = string.IsNullOrEmpty(message) ? "오류 발생." : message;
            Debug.LogError(GetMessage(header, message));
        }

        /// <summary>
        /// Exception을 출력합니다.
        /// </summary>
        /// <typeparam name="T">클래스 타입</typeparam>
        /// <param name="exception">발생한 예외</param>
        /// <param name="context">예외 발생 컨텍스트</param>
        public static void LogError<T>(Exception exception, string context = "")
        {
            if (!IsUseLog) 
                return;
            
            LogError(typeof(T).Name, exception, context);
        }

        /// <summary>
        /// Exception을 출력합니다. (static 클래스용)
        /// </summary>
        /// <param name="header">로그 헤더 (클래스 이름 사용 권장)</param>
        /// <param name="exception">발생한 예외</param>
        /// <param name="context">예외 발생 컨텍스트</param>
        public static void LogError(string header, Exception exception, string context = "")
        {
            if (!IsUseLog) 
                return;
            
            var message = string.IsNullOrEmpty(context) 
                ? $"예외 발생: {exception.Message}" 
                : $"{context}: {exception.Message}";
            
            Debug.LogError(GetMessage(header, message));
        }

        private static string GetMessage<T>(string message) => GetMessage(typeof(T).Name, message);
        private static string GetMessage(string className, string message) => $"[{className}] {message}";
    }
}