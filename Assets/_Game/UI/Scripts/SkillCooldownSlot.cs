using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 개별 스킬의 쿨다운을 표시하는 UI. SkillCooldownHudUI의 하위 부품
    /// </summary>
    public class SkillCooldownSlot : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;

        [Header("시각적 설정")]
        [SerializeField] private Color _readyColor = Color.white;
        [SerializeField] private Color _cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        [Header("애니메이션")]
        [SerializeField] private bool _enableReadyPulse = true;
        [SerializeField] private float _pulseSpeed = 3f;
        [SerializeField] private float _pulseIntensity = 0.2f;

        private float _remainingTime;
        private float _totalCooldown;

        private void Update()
        {
            UpdateCooldownVisuals();
        }

        public void SetCooldown(float remainingTime)
        {
            _remainingTime = Mathf.Max(0f, remainingTime);
        }

        public void SetMaxCooldown(float totalCooldown)
        {
            _totalCooldown = Mathf.Max(0f, totalCooldown);
        }
        
        public void SetSkillIcon(Sprite iconSprite)
        {
            if (_skillIcon != null && iconSprite != null)
                _skillIcon.sprite = iconSprite;
        }
        
        private void UpdateCooldownVisuals()
        {
            var isReady = _totalCooldown <= 0f || _remainingTime <= 0f;

            if (isReady)
            {
                if (_enableReadyPulse)
                {
                    var pulse = 1f + _pulseIntensity * Mathf.Sin(Time.time * _pulseSpeed);
                    _skillIcon.color = _readyColor * pulse;
                }
                else
                {
                    _skillIcon.color = _readyColor;
                }
                _cooldownOverlay.fillAmount = 0f;                    
                _cooldownText.text = "";
            }
            else
            {
                if (_totalCooldown > 0f)
                    _cooldownOverlay.fillAmount = _remainingTime / _totalCooldown;

                if (_remainingTime > 1f)
                    _cooldownText.text = Mathf.Ceil(_remainingTime).ToString("F0");
                else
                    _cooldownText.text = _remainingTime.ToString("F1");
            }
        }
    }
}
