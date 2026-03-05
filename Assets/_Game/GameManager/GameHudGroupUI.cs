using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬의 인게임 HUD 요소들을 일괄적으로 관리(표시/숨기기)하는 클래스
    /// </summary>
    public class GameHudGroupUI
    {
        private readonly HealthBarHudUI _healthBarHudUI;
        private readonly ExpGaugeHudUI _expGaugeHudUI;
        private readonly PlayerStatusHudUI _playerStatusHudUI;
        private readonly SkillCooldownHudUI _skillCooldownHudUI;
        private readonly GameButtonHudUI _gameButtonHudUI;

        [Inject]
        public GameHudGroupUI(
            HealthBarHudUI healthBarHudUI,
            ExpGaugeHudUI expGaugeHudUI,
            PlayerStatusHudUI playerStatusHudUI,
            SkillCooldownHudUI skillCooldownHudUI,
            GameButtonHudUI gameButtonHudUI)
        {
            _healthBarHudUI = healthBarHudUI;
            _expGaugeHudUI = expGaugeHudUI;
            _playerStatusHudUI = playerStatusHudUI;
            _skillCooldownHudUI = skillCooldownHudUI;
            _gameButtonHudUI = gameButtonHudUI;
        }

        public void ShowHud()
        {
            _healthBarHudUI?.Show();
            _expGaugeHudUI?.Show();
            _playerStatusHudUI?.Show();
            _skillCooldownHudUI?.Show();
            _gameButtonHudUI?.Show();
        }

        public void HideHud()
        {
            _healthBarHudUI?.Hide();
            _expGaugeHudUI?.Hide();
            _playerStatusHudUI?.Hide();
            _skillCooldownHudUI?.Hide();
            _gameButtonHudUI?.Hide();
        }
    }
}
