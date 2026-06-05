using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 체력 비율과 수치를 표시하는 HUD
    /// </summary>
    public class HealthBarHudUI : HudUI
    {
        [Header("UI 요소")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        
        private int _currentHealth;
        private int _maxHealth;

        public void SetHealth(int current)
        {
            _currentHealth = current;
            UpdateVisuals();
        }
        
        public void SetMaxHealth(int max)
        {
            _maxHealth = max;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var targetFillAmount = (float)_currentHealth / (float)_maxHealth;
            
            if (_healthText != null)
                _healthText.text = $"{_currentHealth} / {_maxHealth}";

            if (_healthFillImage != null)
                _healthFillImage.fillAmount = targetFillAmount;
        }
    }
}
