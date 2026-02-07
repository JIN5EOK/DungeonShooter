using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 레벨과 경험치 게이지를 표시하는 HUD.
    /// </summary>
    public class ExpGaugeHudUI : HudUI
    {
        private int _expPerLevel = 100;

        [Header("UI 요소")]
        [SerializeField] private Image _expFillImage;
        [SerializeField] private TextMeshProUGUI _levelText;

        public void SetLevel(int level)
        {
            if (_levelText != null)
                _levelText.text = level.ToString();
        }

        public void SetExp(int currentExp)
        {
            if (_expFillImage != null)
                _expFillImage.fillAmount = (float)currentExp / _expPerLevel;
        }

        public void SetMaxExp(int maxExp)
        {
            _expPerLevel = maxExp;
        }
    }
}
