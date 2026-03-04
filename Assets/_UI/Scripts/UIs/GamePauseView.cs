using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    public class GamePauseView : PopupUI
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitButton;

        public event Action OnResumeClickedEvent;
        public event Action OnExitClickedEvent;

        private void Start()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeClicked);
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnResumeClicked()
        {
            OnResumeClickedEvent?.Invoke();
        }

        private void OnExitClicked()
        {
            OnExitClickedEvent?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(OnExitClicked);
        }
    }
}
