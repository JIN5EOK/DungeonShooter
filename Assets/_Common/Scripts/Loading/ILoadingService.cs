using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 명령 실행 주체. 태스크를 큐에 넣거나, 로딩을 표시한 채로 작업을 실행한다.
    /// 윈도우/스피너 표시 상태를 보유하며, 상태 변경 시 OnStateChanged를 발생시킨다.
    /// </summary>
    public interface ILoadingService
    {
        bool IsWindowLoadingRunning { get; }
        bool IsSpinnerLoadingRunning { get; }

        /// <summary>
        /// 윈도우 또는 스피너 표시 상태가 바뀔 때 발생한다.
        /// </summary>
        event Action OnStateChanged;

        /// <summary>
        /// 로딩 태스크를 큐에 넣는다. 태스크는 순서대로 실행된다.
        /// </summary>
        void EnqueueTask(LoadingTask loadingTask);
    }
}
