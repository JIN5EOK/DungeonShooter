using DG.Tweening;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩 ViewModel을 구독하여 윈도우/스피너 패널 표시 여부를 갱신하는 뷰. DontDestroyOnLoad로 유지된다.
    /// </summary>
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private GameObject _windowPanel;
        [SerializeField] private GameObject _spinnerPanel;

        private LoadingViewModel _viewModel;

        [Inject]
        public void Construct(LoadingViewModel viewModel)
        {
            if (_viewModel != null)
                _viewModel.OnStateChanged -= RefreshVisibility;

            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.OnStateChanged += RefreshVisibility;
                RefreshVisibility();
            }
        }

        private void OnEnable()
        {
            if (_viewModel != null)
                RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_viewModel != null)
                _viewModel.OnStateChanged -= RefreshVisibility;
        }

        private void RefreshVisibility()
        {
            if (_viewModel == null)
                return;

            if (_windowPanel != null)
                _windowPanel.SetActive(_viewModel.IsWindowVisible);
            if (_spinnerPanel != null)
            {
                if (_viewModel.IsSpinnerVisible == true)
                {
                    _spinnerPanel.SetActive(true);

                    _spinnerPanel.transform.DORotate(new Vector3(0,0,360),1f,RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1);    
                }
                else
                {
                    _spinnerPanel.SetActive(false);
                    _spinnerPanel.transform.DOKill();
                }
            }
                
        }
    }
}
