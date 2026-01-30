using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 체력 비율과 수치를 표시하는 HUD. HealthComponent를 참조하지 않고, 외부에서 SetHealth로 갱신한다.
    /// </summary>
    public class HealthBarUI : HudUI
    {
        [Header("UI 요소")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        
        private int _currentHealth;
        private int _maxHealth;
        private float _targetFillAmount;
        private float _currentFillAmount;
        private float _fillAnimationSpeed = 5f;
        
        private void Update()
        {
            UpdateFill();
        }

        /// <summary>
        /// 체력 수치를 설정한다.
        /// </summary>
        public void SetHealth(int current, int max)
        {
            _currentHealth = Mathf.Max(0, current);
            _maxHealth = Mathf.Max(1, max);
            _targetFillAmount = (float)_currentHealth / _maxHealth;
            UpdateVisuals();
        }

        private void UpdateFill()
        {
            _healthFillImage.fillAmount = _targetFillAmount;
        }

        private void UpdateVisuals()
        {
            if (_healthText != null)
                _healthText.text = $"{_currentHealth} / {_maxHealth}";
        }
    }
}
