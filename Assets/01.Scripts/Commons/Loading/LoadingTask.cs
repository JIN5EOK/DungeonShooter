using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 명령 태스크. 실행 시 로딩 UI가 표시된 상태로 작업이 수행된다.
    /// </summary>
    public class LoadingTask
    {
        private readonly Func<CancellationToken, UniTask> _work;
        private readonly UniTaskCompletionSource _completion = new();
        private readonly CancellationToken _cancellationToken;

        public LoadingType LoadingType { get; }

        /// <summary>
        /// 이 태스크가 완료될 때까지 대기하는 UniTask
        /// </summary>
        public UniTask CompletionTask => _completion.Task;

        /// <summary>
        /// 취소 토큰을 넣을 수도, 생략하면 취소 없이 동작한다.
        /// </summary>
        public LoadingTask(LoadingType loadingType, Func<CancellationToken, UniTask> work, CancellationToken cancellationToken = default)
        {
            LoadingType = loadingType;
            _work = work ?? throw new ArgumentNullException(nameof(work));
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 로딩 작업을 실행한다. 완료 시 CompletionTask가 완료된다.
        /// </summary>
        public async UniTask Run()
        {
            try
            {
                await _work(_cancellationToken);
                _completion.TrySetResult();
            }
            catch (Exception ex)
            {
                _completion.TrySetException(ex);
                throw;
            }
        }
    }
}
