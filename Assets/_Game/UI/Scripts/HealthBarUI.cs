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

    private HealthComponent _healthComponent;
    private float _targetFillAmount;
    private float _currentFillAmount;

    /// <summary>
    /// 체력 컴포넌트로 초기화
    /// </summary>
    public void Initialize(HealthComponent health)
    {
        _healthComponent = health;

        if (_healthComponent != null)
        {
            // 초기 체력 설정
            var maxHP = _healthComponent.MaxHealth;
            var currentHP = _healthComponent.CurrentHealth;

            _targetFillAmount = (float)currentHP / maxHP;
            _currentFillAmount = _targetFillAmount;

            UpdateVisuals();

            // 체력 변경 이벤트 구독
            _healthComponent.OnDamaged += OnHealthChanged;
            // OnHealed 이벤트가 있다면 구독
        }
    }

    private void Update()
    {
        if (_healthComponent == null) return;

        UpdateFillAnimation();
        UpdatePulseEffect();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (_healthComponent == null) return;

        // 현재 체력 비율 계산
        var healthRatio = (float)_healthComponent.CurrentHealth / _healthComponent.MaxHealth;
        _targetFillAmount = healthRatio;

        // 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{_healthComponent.CurrentHealth} / {_healthComponent.MaxHealth}";
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
        _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * fillAnimationSpeed);
        healthFillImage.fillAmount = _currentFillAmount;
    }

    /// <summary>
    /// 시각적 요소 업데이트
    /// </summary>
    private void UpdateVisuals()
    {
        if (healthFillImage == null) return;

        // 체력 비율에 따른 색상 변경
        Color targetColor;
        if (_targetFillAmount <= criticalHealthThreshold)
        {
            targetColor = criticalHealthColor;
        }
        else if (_targetFillAmount <= lowHealthThreshold)
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
        if (!enablePulseEffect || _targetFillAmount > criticalHealthThreshold) return;

        if (healthFillImage != null)
        {
            var alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * pulseSpeed);
            var color = healthFillImage.color;
            color.a = alpha;
            healthFillImage.color = color;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_healthComponent != null)
        {
            _healthComponent.OnDamaged -= OnHealthChanged;
        }
    }
}
