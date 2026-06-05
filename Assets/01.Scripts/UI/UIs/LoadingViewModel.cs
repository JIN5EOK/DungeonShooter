using System;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 뷰가 구독하는 상태. ILoadingService의 상태 변화를 구독하여 윈도우/스피너 표시 여부를 노출한다.
    /// </summary>
    public class LoadingViewModel
    {
        private readonly ILoadingService _loadingService;

        public bool IsWindowVisible { get; private set; }
        public bool IsSpinnerVisible { get; private set; }
        public event Action OnStateChanged;
        [Inject]
        public LoadingViewModel(ILoadingService loadingService)
        {
            _loadingService = loadingService;
            _loadingService.OnStateChanged += OnServiceChanged;
            OnServiceChanged();
        }

        private void OnServiceChanged()
        {
            var windowVisible = _loadingService.IsWindowLoadingRunning;
            var spinnerVisible = _loadingService.IsSpinnerLoadingRunning;
            if (IsWindowVisible == windowVisible && IsSpinnerVisible == spinnerVisible)
                return;
            IsWindowVisible = windowVisible;
            IsSpinnerVisible = spinnerVisible;
            OnStateChanged?.Invoke();
        }
    }
}
