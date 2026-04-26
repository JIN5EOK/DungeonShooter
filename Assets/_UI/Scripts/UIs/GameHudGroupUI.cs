using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬의 인게임 HUD 요소들을 일괄적으로 관리(표시/숨기기)하는 UI
    /// </summary>
    public class GameHudGroupUI : HudUI
    {
        public HealthBarHudUI HealthBarHudUI => _healthBarHudUI;
        public ExpGaugeHudUI ExpGaugeHudUI => _expGaugeHudUI;
        public TouchInputUI TouchInputUI => _touchInputUI;
        
        [SerializeField]
        private HealthBarHudUI _healthBarHudUI;
        [SerializeField]
        private ExpGaugeHudUI _expGaugeHudUI;
        [SerializeField]
        private TouchInputUI _touchInputUI;

        [Header("Buttons")]
        [SerializeField] private Button _pauseButton;

        public event Action OnPauseRequested;

        private void Awake()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        }

        private void OnPauseButtonClicked() => OnPauseRequested?.Invoke();

        public void ShowHud()
        {
            _touchInputUI?.Show();
            _healthBarHudUI?.Show();
            _expGaugeHudUI?.Show();
        }

        public void HideHud()
        {
            _touchInputUI?.Hide();
            _healthBarHudUI?.Hide();
            _expGaugeHudUI?.Hide();
        }
    }
}
