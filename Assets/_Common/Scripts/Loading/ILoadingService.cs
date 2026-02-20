using System.Threading;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 명령 실행 주체. 태스크를 큐에 넣거나, 로딩을 표시한 채로 작업을 실행한다.
    /// </summary>
    public interface ILoadingService
    {
        /// <summary>
        /// 로딩 태스크를 큐에 넣는다. 태스크는 순서대로 실행된다.
        /// </summary>
        void EnqueueTask(LoadingTask loadingTask);
    }
}
