using System;
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
        [SerializeField] private Image _skillIcon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        
        private Color _readyColor = Color.white;
        
        private float _pulseSpeed = 3f;
        private float _pulseIntensity = 0.2f;

        private Skill _skill;
        private float _maxCooldown;
        public void SetSkill(Skill skill)
        {
            if (_skill != null)
            {
                _skill.OnCooldownChanged -= UpdateCooldownVisuals;
            }
            
            _skill = skill;
            _skillIcon.sprite = _skill.Icon;
            _maxCooldown = skill.MaxCooldown;
            _skill.OnCooldownChanged += UpdateCooldownVisuals;
            UpdateCooldownVisuals(skill.Cooldown);
        }
        
        private void UpdateCooldownVisuals(float cooldown)
        {
            var isReady = _maxCooldown <= 0f || cooldown <= 0f;

            if (isReady)
            {
                var pulse = 1f + _pulseIntensity * Mathf.Sin(Time.time * _pulseSpeed);
                    _skillIcon.color = _readyColor * pulse;
                _cooldownOverlay.fillAmount = 0f;                    
                _cooldownText.text = "";
            }
            else
            {
                if (_maxCooldown > 0f)
                    _cooldownOverlay.fillAmount = cooldown / _maxCooldown;

                if (cooldown > 1f)
                    _cooldownText.text = Mathf.Ceil(cooldown).ToString("F0");
                else
                    _cooldownText.text = cooldown.ToString("F1");
            }
        }

        public void OnDestroy()
        {
            if(_skill != null)
                _skill.OnCooldownChanged -= UpdateCooldownVisuals;
        }
    }
}
