using TMPro;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 스탯(공격력, 방어력, 이동속도)과 남은 적 수를 수치로 표시하는 HUD.
    /// </summary>
    public class PlayerStatusHudUI : HudUI
    {
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _moveSpeedText;
        [SerializeField] private TextMeshProUGUI _remainingEnemyCountText;

        public void SetAttack(int value)
        {
            if (_attackText != null) _attackText.text = value.ToString();
        }

        public void SetDefense(int value)
        {
            if (_defenseText != null) _defenseText.text = value.ToString();
        }

        public void SetMoveSpeed(int value)
        {
            if (_moveSpeedText != null) _moveSpeedText.text = value.ToString();
        }

        public void SetRemainingEnemyCount(int count)
        {
            if (_remainingEnemyCountText != null)
                _remainingEnemyCountText.text = count.ToString();
        }
    }
}
