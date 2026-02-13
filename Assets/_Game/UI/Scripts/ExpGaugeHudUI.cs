using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 레벨과 경험치 게이지를 표시하는 HUD.
    /// </summary>
    public class ExpGaugeHudUI : HudUI
    {
        private int _expPerLevel = 100;

        private int _currentExp = 0;
        [Header("UI 요소")]
        [SerializeField] private Image _expFillImage;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        private PlayerLevelManager _levelManager;
        
        [Inject]
        public void Construct(PlayerLevelManager levelManager)
        {
            _levelManager = levelManager;
            _levelManager.OnLevelChanged += SetLevel;
            _levelManager.OnExpChanged += SetExp;
            _levelManager.OnMaxExpChanged += SetMaxExp;
            SetLevel(_levelManager.Level);
            SetExp(_levelManager.Exp);
            SetMaxExp(_levelManager.MaxExp);
        }
        
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _levelManager.OnExpChanged -= SetExp;
            _levelManager.OnMaxExpChanged -= SetMaxExp;
            _levelManager.OnLevelChanged -= SetLevel;
        }
    }
}
