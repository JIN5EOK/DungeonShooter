using System;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 뷰가 구독하는 상태. 윈도우/스피너 표시 여부와 상태 변화만 노출한다.
    /// </summary>
    public class LoadingViewModel
    {
        public bool IsWindowVisible { get; private set; }
        public bool IsSpinnerVisible { get; private set; }
        public event Action OnStateChanged;

        public void SetWindowVisible(bool visible)
        {
            if (IsWindowVisible == visible) 
                return;
            IsWindowVisible = visible;
            OnStateChanged?.Invoke();
        }

        public void SetSpinnerVisible(bool visible)
        {
            if (IsSpinnerVisible == visible) 
                return;
            IsSpinnerVisible = visible;
            OnStateChanged?.Invoke();
        }
    }
}
