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

        public event Action OnExitClickedEvent;

        public void Start()
        {
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }

        public void ShowResult(string resultMessage, string enemyKillCountMessage, string playTimeMessage,
            string exitButtonMessage)
        {
            if (_resultText != null)
            {
                _resultText.text = resultMessage;
            }

            if (_enemyKillCountText != null)
            {
                _enemyKillCountText.text = enemyKillCountMessage;
            }

            if (_playTimeText != null)
            {
                _playTimeText.text = playTimeMessage;
            }

            if (_exitButtonText != null)
            {
                _exitButtonText.text = exitButtonMessage;
            }

            Show();
        }

        private void OnExitClicked()
        {
            OnExitClickedEvent?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }
        }
    }
}
