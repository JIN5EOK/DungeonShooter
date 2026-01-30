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
        [SerializeField] private Color _overlayColor = new Color(0f, 0f, 0f, 0.6f);

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

        /// <summary>
        /// 쿨다운 수치를 설정한다.
        /// </summary>
        public void SetCooldown(float remainingTime)
        {
            _remainingTime = Mathf.Max(0f, remainingTime);
        }

        public void SetMaxCooldown(float totalCooldown)
        {
            _totalCooldown = Mathf.Max(0f, totalCooldown);
        }
        
        private void UpdateCooldownVisuals()
        {
            var isReady = _totalCooldown <= 0f || _remainingTime <= 0f;

            if (isReady)
            {
                if (_skillIcon != null)
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
                }

                if (_cooldownOverlay != null)
                    _cooldownOverlay.fillAmount = 0f;

                if (_cooldownText != null)
                {
                    _cooldownText.text = "준비완료";
                    _cooldownText.color = Color.green;
                }
            }
            else
            {
                if (_skillIcon != null)
                    _skillIcon.color = _cooldownColor;

                if (_cooldownOverlay != null && _totalCooldown > 0f)
                    _cooldownOverlay.fillAmount = _remainingTime / _totalCooldown;

                if (_cooldownText != null)
                {
                    _cooldownText.color = Color.white;
                    if (_remainingTime > 1f)
                        _cooldownText.text = Mathf.Ceil(_remainingTime).ToString("F0");
                    else
                        _cooldownText.text = _remainingTime.ToString("F1");
                }
            }
        }

        /// <summary>
        /// 스킬 아이콘을 설정한다.
        /// </summary>
        public void SetSkillIcon(Sprite iconSprite)
        {
            if (_skillIcon != null && iconSprite != null)
                _skillIcon.sprite = iconSprite;
        }

        /// <summary>
        /// 스킬 사용 시각 피드백을 재생한다.
        /// </summary>
        public void OnSkillUsed()
        {
            StartCoroutine(SkillUsedEffect());
        }

        private IEnumerator SkillUsedEffect()
        {
            if (_skillIcon == null) yield break;

            var originalScale = _skillIcon.transform.localScale;
            var bigScale = originalScale * 1.2f;
            var duration = 0.1f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                _skillIcon.transform.localScale = Vector3.Lerp(originalScale, bigScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                _skillIcon.transform.localScale = Vector3.Lerp(bigScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _skillIcon.transform.localScale = originalScale;
        }
    }
}
