using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    public class GameResultView : PopupUI
    {
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _enemyKillCountText;
        [SerializeField] private TMP_Text _playTimeText;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TMP_Text _exitButtonText;

        private GameResultViewModel _viewModel;

        [Inject]
        public void Construct(GameResultViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.OnResultUpdated += UpdateUI;

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);

            if (_exitButtonText != null)
                _exitButtonText.text = _viewModel.ExitButtonMessage;
            if (_exitButtonText != null)
                _exitButtonText.text = _viewModel.ExitButtonMessage;
        }

        private void UpdateUI()
        {
            if (_resultText != null)
            {
                _resultText.text = _viewModel.ResultMessage;
            }

            if (_enemyKillCountText != null)
            {
                _enemyKillCountText.text = _viewModel.EnemyKillCountMessage;
            }

            if (_playTimeText != null)
            {
                _playTimeText.text = _viewModel.PlayTimeMessage;
            }

            if (_exitButtonText != null)
            {
                _exitButtonText.text = _viewModel.ExitButtonMessage;
            }

            Show();
        }

        private void OnExitClicked()
        {
            _viewModel?.ExitGame();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_viewModel != null)
            {
                _viewModel.OnResultUpdated -= UpdateUI;
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }
        }
    }
}
