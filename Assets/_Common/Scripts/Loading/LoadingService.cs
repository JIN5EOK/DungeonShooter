using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 태스크 큐를 관리하고 로딩 처리 시 ViewModel에 윈도우/스피너 표시 상태를 반영한다.
    /// </summary>
    public class LoadingService : ILoadingService
    {
        private readonly LoadingViewModel _viewModel;
        private readonly Queue<LoadingTask> _windowTasks = new();
        private readonly Queue<LoadingTask> _spinnerTasks = new();
        private bool _isProcessing;

        public LoadingService(LoadingViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void EnqueueTask(LoadingTask loadingTask)
        {
            if (loadingTask.LoadingType == LoadingType.LoadingWindow)
                _windowTasks.Enqueue(loadingTask);
            else
                _spinnerTasks.Enqueue(loadingTask);

            if (!_isProcessing)
                ProcessQueueAsync().Forget();
        }

        private async UniTaskVoid ProcessQueueAsync()
        {
            _isProcessing = true;
            try
            {
                while (_windowTasks.Count > 0)
                {
                    var task = _windowTasks.Dequeue();
                    _viewModel.SetWindowVisible(true);
                    try
                    {
                        await task.Run();
                    }
                    finally
                    {
                        _viewModel.SetWindowVisible(false);
                    }
                }

                while (_spinnerTasks.Count > 0)
                {
                    var task = _spinnerTasks.Dequeue();
                    _viewModel.SetSpinnerVisible(true);
                    try
                    {
                        await task.Run();
                    }
                    finally
                    {
                        _viewModel.SetSpinnerVisible(false);
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}
