using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 체력 비율과 수치를 표시하는 HUD. HealthComponent를 참조하지 않고, 외부에서 SetHealth로 갱신한다.
    /// </summary>
    public class HealthBarHudUI : HudUI
    {
        [Header("UI 요소")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        
        private int _currentHealth;
        private int _maxHealth;
        private float _targetFillAmount;
        
        public void SetHealthAndMaxHealth(int current, int max)
        {
            _currentHealth = current;
            _maxHealth = max;
            UpdateVisuals();
        }
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
            _targetFillAmount = (float)_currentHealth / (float)_maxHealth;
            
            if (_healthText != null)
                _healthText.text = $"{_currentHealth} / {_maxHealth}";

            if (_healthFillImage != null)
                _healthFillImage.fillAmount = _targetFillAmount;
        }
    }
}
