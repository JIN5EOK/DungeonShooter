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

        private IPlayerContextManager _playerContextManager;
        private IEntityStatus _hpStatus;
        private IEntityStat _hpStat;

        [Inject]
        public void Construct(IPlayerContextManager playerContextManager)
        {
            _playerContextManager = playerContextManager;
            var context = _playerContextManager.EntityContext;
            if (context == null) return;

            _hpStatus = context.Statuses?.GetStatus(StatusType.Hp);
            _hpStat = context.Stat?.GetStat(StatType.Hp);
            if (_hpStatus != null)
                _hpStatus.OnValueChanged += SetHealth;
            if (_hpStat != null)
                _hpStat.OnValueChanged += SetMaxHealth;
            SetHealth(_hpStatus?.GetValue() ?? 0);
            SetMaxHealth(_hpStat?.GetValue() ?? 0);
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
            var targetFillAmount = (float)_currentHealth / (float)_maxHealth;
            
            if (_healthText != null)
                _healthText.text = $"{_currentHealth} / {_maxHealth}";

            if (_healthFillImage != null)
                _healthFillImage.fillAmount = targetFillAmount;
        }
        
        protected override void OnDestroy()
        {
            if (_hpStatus != null)
                _hpStatus.OnValueChanged -= SetHealth;
            if (_hpStat != null)
                _hpStat.OnValueChanged -= SetMaxHealth;
        }
    }
}
