using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShooter
{
    /// <summary>
    /// 레벨과 경험치 게이지를 표시하는 HUD.
    /// </summary>
    public class ExpGaugeHudUI : HudUI
    {
        private int _expPerLevel = 0;

        private int _currentExp = 0;
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
            _currentExp = currentExp;
            if (_expFillImage != null)
                _expFillImage.fillAmount = (float)currentExp / _expPerLevel;
        }

        public void SetMaxExp(int maxExp)
        {
            _expPerLevel = maxExp;
            SetExp(_currentExp);
        }
    }
}
