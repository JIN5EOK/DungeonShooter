using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    public class PauseMenuUI : PopupUI
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
    }
}
