using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 쿨다운을 표시하는 UI 컴포넌트
    /// </summary>
    public class SkillCooldownUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image skillIcon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;

        [Header("시각적 설정")]
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.6f);

        [Header("애니메이션")]
        [SerializeField] private bool enableReadyPulse = true;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseIntensity = 0.2f;

        private string _skillCooldownKey;
        private string _skillDisplayName;
        private bool _isReady = true;

        /// <summary>
        /// 스킬 UI 초기화
        /// </summary>
        public void Initialize(string cooldownKey, string displayName)
        {
            _skillCooldownKey = cooldownKey;
            _skillDisplayName = displayName;

            // 초기 상태는 준비 완료
            SetReadyState();
        }

        /// <summary>
        /// 쿨다운 상태 업데이트
        /// </summary>
        // public void UpdateCooldown(CooldownComponent cooldownComponent)
        // {
        //     if (cooldownComponent == null || string.IsNullOrEmpty(_skillCooldownKey))
        //     {
        //         LogHandler.LogWarning<SkillCooldownUI>($"UpdateCooldown 실패: cooldownComponent={cooldownComponent != null}, key='{_skillCooldownKey}'");
        //         return;
        //     }

        //     var skillReady = cooldownComponent.IsReady(_skillCooldownKey);
        //     var remainingTime = cooldownComponent.GetRemainingCooldown(_skillCooldownKey);
        //     var totalCooldown = cooldownComponent.GetTotalCooldown(_skillCooldownKey);

        //     // 디버그 로그 (쿨다운 중일 때만)
        //     if (!skillReady)
        //     {
        //         LogHandler.Log<SkillCooldownUI>($"{_skillCooldownKey}: {remainingTime:F1}s / {totalCooldown:F1}s");
        //     }

        //     if (skillReady)
        //     {
        //         if (!_isReady) SetReadyState();
        //     }
        //     else
        //     {
        //         if (_isReady) SetCooldownState();
        //         UpdateCooldownProgress(remainingTime, totalCooldown);
        //     }
        // }

        private void Update()
        {
            // 준비 상태에서 펄스 효과
            if (_isReady && enableReadyPulse && skillIcon != null)
            {
                var pulse = 1f + pulseIntensity * Mathf.Sin(Time.time * pulseSpeed);
                skillIcon.color = readyColor * pulse;
            }
        }

        /// <summary>
        /// 준비 완료 상태로 설정
        /// </summary>
        private void SetReadyState()
        {
            _isReady = true;

            if (skillIcon != null)
            {
                skillIcon.color = readyColor;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.color = overlayColor;
            }

            if (cooldownText != null)
            {
                cooldownText.text = "준비완료";
                cooldownText.color = Color.green;
            }
        }

        /// <summary>
        /// 쿨다운 상태로 설정
        /// </summary>
        private void SetCooldownState()
        {
            _isReady = false;

            if (skillIcon != null)
            {
                skillIcon.color = cooldownColor;
            }

            if (cooldownText != null)
            {
                cooldownText.color = Color.white;
            }
        }

        /// <summary>
        /// 쿨다운 진행도 업데이트
        /// </summary>
        private void UpdateCooldownProgress(float remainingTime, float totalCooldown)
        {
            if (totalCooldown <= 0) return;

            // 오버레이 fillAmount 업데이트 (시계 방향으로 줄어듦)
            if (cooldownOverlay != null)
            {
                var progress = remainingTime / totalCooldown;
                cooldownOverlay.fillAmount = progress;
            }

            // 남은 시간 텍스트 업데이트
            if (cooldownText != null)
            {
                if (remainingTime > 1f)
                {
                    cooldownText.text = Mathf.Ceil(remainingTime).ToString("F0");
                }
                else
                {
                    cooldownText.text = remainingTime.ToString("F1");
                }
            }
        }

        /// <summary>
        /// 스킬 아이콘 설정
        /// </summary>
        public void SetSkillIcon(Sprite iconSprite)
        {
            if (skillIcon != null && iconSprite != null)
            {
                skillIcon.sprite = iconSprite;
            }
        }

        /// <summary>
        /// 스킬 사용 트리거 (시각적 피드백용)
        /// </summary>
        public void OnSkillUsed()
        {
            // 스킬 사용 시 간단한 시각 효과
            StartCoroutine(SkillUsedEffect());
        }

        private System.Collections.IEnumerator SkillUsedEffect()
        {
            if (skillIcon == null) yield break;

            // 잠깐 크게 만들었다가 원래 크기로
            var originalScale = skillIcon.transform.localScale;
            var bigScale = originalScale * 1.2f;

            var duration = 0.1f;
            var elapsed = 0f;

            // 커지기
            while (elapsed < duration)
            {
                skillIcon.transform.localScale = Vector3.Lerp(originalScale, bigScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            // 원래 크기로
            while (elapsed < duration)
            {
                skillIcon.transform.localScale = Vector3.Lerp(bigScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            skillIcon.transform.localScale = originalScale;
        }
    }
}
