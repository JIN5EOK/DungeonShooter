using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 체력을 표시하는 UI 컴포넌트
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("시각적 설정")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color criticalHealthColor = new Color(1f, 0.3f, 0f); // 주황색
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float criticalHealthThreshold = 0.15f;

    [Header("애니메이션")]
    [SerializeField] private float fillAnimationSpeed = 5f;
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;

    private HealthComponent healthComponent;
    private float targetFillAmount;
    private float currentFillAmount;

    /// <summary>
    /// 체력 컴포넌트로 초기화
    /// </summary>
    public void Initialize(HealthComponent health)
    {
        healthComponent = health;

        if (healthComponent != null)
        {
            // 초기 체력 설정
            int maxHP = healthComponent.MaxHealth;
            int currentHP = healthComponent.CurrentHealth;

            targetFillAmount = (float)currentHP / maxHP;
            currentFillAmount = targetFillAmount;

            UpdateVisuals();

            // 체력 변경 이벤트 구독
            healthComponent.OnDamaged += OnHealthChanged;
            // OnHealed 이벤트가 있다면 구독
        }
    }

    private void Update()
    {
        if (healthComponent == null) return;

        UpdateFillAnimation();
        UpdatePulseEffect();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (healthComponent == null) return;

        // 현재 체력 비율 계산
        float healthRatio = (float)healthComponent.CurrentHealth / healthComponent.MaxHealth;
        targetFillAmount = healthRatio;

        // 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{healthComponent.CurrentHealth} / {healthComponent.MaxHealth}";
        }

        UpdateVisuals();
    }

    /// <summary>
    /// 체력 변경 이벤트 핸들러
    /// </summary>
    private void OnHealthChanged(int damage, int remainingHealth)
    {
        UpdateUI();
    }

    /// <summary>
    /// 게이지 애니메이션 업데이트
    /// </summary>
    private void UpdateFillAnimation()
    {
        if (healthFillImage == null) return;

        // 부드럽게 게이지 변경
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * fillAnimationSpeed);
        healthFillImage.fillAmount = currentFillAmount;
    }

    /// <summary>
    /// 시각적 요소 업데이트
    /// </summary>
    private void UpdateVisuals()
    {
        if (healthFillImage == null) return;

        // 체력 비율에 따른 색상 변경
        Color targetColor;
        if (targetFillAmount <= criticalHealthThreshold)
        {
            targetColor = criticalHealthColor;
        }
        else if (targetFillAmount <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else
        {
            targetColor = fullHealthColor;
        }

        healthFillImage.color = targetColor;
    }

    /// <summary>
    /// 위험 상태에서 펄스 효과
    /// </summary>
    private void UpdatePulseEffect()
    {
        if (!enablePulseEffect || targetFillAmount > criticalHealthThreshold) return;

        if (healthFillImage != null)
        {
            float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * pulseSpeed);
            Color color = healthFillImage.color;
            color.a = alpha;
            healthFillImage.color = color;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (healthComponent != null)
        {
            healthComponent.OnDamaged -= OnHealthChanged;
        }
    }
}
