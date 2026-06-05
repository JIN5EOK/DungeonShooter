using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 태스크 큐를 관리하고, 로딩 처리 시 윈도우/스피너 표시 상태를 보유하며 OnStateChanged로 알린다.
    /// </summary>
    public class LoadingService : ILoadingService
    {
        public event Action OnStateChanged;

        private readonly Queue<LoadingTask> _windowTasks = new();
        private readonly Queue<LoadingTask> _spinnerTasks = new();

        private bool _isProcessing;

        public bool IsWindowLoadingRunning { get; private set; }
        public bool IsSpinnerLoadingRunning { get; private set; }

        private IPauseManager _pauseManager;
        [Inject]
        public LoadingService(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
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

        private void SetWindowVisible(bool visible)
        {
            if (IsWindowLoadingRunning == visible)
                return;
            IsWindowLoadingRunning = visible;
            OnStateChanged?.Invoke();
        }

        private void SetSpinnerVisible(bool visible)
        {
            if (IsSpinnerLoadingRunning == visible)
                return;
            IsSpinnerLoadingRunning = visible;
            OnStateChanged?.Invoke();
        }

        private async UniTaskVoid ProcessQueueAsync()
        {
            _isProcessing = true;
            try
            {
                while (_windowTasks.Count > 0)
                {
                    var task = _windowTasks.Dequeue();
                    SetWindowVisible(true);
                    try
                    {
                        // 로딩화면시엔 게임 일시정지 처리
                        _pauseManager.PauseRequest(this);
                        await task.Run();
                    }
                    finally
                    {
                        _pauseManager.ResumeRequest(this);
                        SetWindowVisible(false);
                    }
                }

                while (_spinnerTasks.Count > 0)
                {
                    var task = _spinnerTasks.Dequeue();
                    SetSpinnerVisible(true);
                    try
                    {
                        await task.Run();
                    }
                    finally
                    {
                        SetSpinnerVisible(false);
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
