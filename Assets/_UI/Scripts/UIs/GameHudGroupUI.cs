using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬의 인게임 HUD 요소들을 일괄적으로 관리(표시/숨기기)하는 UI
    /// </summary>
    public class GameHudGroupUI : HudUI
    {
        [SerializeField]
        private HealthBarHudUI _healthBarHudUI;
        [SerializeField]
        private ExpGaugeHudUI _expGaugeHudUI;
        [SerializeField]
        private PlayerStatusHudUI _playerStatusHudUI;
        [SerializeField]
        private GameButtonHudUI _gameButtonHudUI;
        [SerializeField]
        private TouchInputUI _touchInputUI;

        public void ShowHud()
        {
            _touchInputUI?.Show();
            _healthBarHudUI?.Show();
            _expGaugeHudUI?.Show();
            _playerStatusHudUI?.Show();
            _gameButtonHudUI?.Show();
        }

        public void HideHud()
        {
            _touchInputUI?.Hide();
            _healthBarHudUI?.Hide();
            _expGaugeHudUI?.Hide();
            _playerStatusHudUI?.Hide();
            _gameButtonHudUI?.Hide();
        }
    }
}
