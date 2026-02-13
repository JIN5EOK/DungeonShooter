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
        private float _targetFillAmount;

        private IEventBus _eventBus;
        private HealthComponent _healthComponent;
        private EntityStatGroup _entityStatGroup;
        
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(PlayerSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(PlayerDestroyed);
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
        
        private void PlayerSpawned(PlayerObjectSpawnEvent spawnEvent)
        {
            _healthComponent = spawnEvent.player.GetComponent<HealthComponent>();
            _entityStatGroup = spawnEvent.player.StatGroup;
            if (_healthComponent != null)
            {
                _healthComponent.OnHealthChanged += SetHealth;
                SetHealth(_healthComponent.CurrentHealth);
            }

            if (_entityStatGroup != null)
            {
                _entityStatGroup.OnStatChanged += MaxHpChanged;
                SetMaxHealth(_entityStatGroup.GetStat(StatType.Hp));
            }
        }

        
        private void MaxHpChanged(StatType statType, int maxHp)
        {
            if (statType != StatType.Hp)
                return;
            
            SetMaxHealth(maxHp);
        }
        
        private void PlayerDestroyed(PlayerObjectDestroyEvent destroyEvent)
        {
            if (_healthComponent != null)
                _healthComponent.OnHealthChanged -= SetHealth;
            if (_entityStatGroup != null)
                _entityStatGroup.OnStatChanged -= MaxHpChanged;
            
            _healthComponent = null;
        }
        
        private void UpdateVisuals()
        {
            _targetFillAmount = (float)_currentHealth / (float)_maxHealth;
            
            if (_healthText != null)
                _healthText.text = $"{_currentHealth} / {_maxHealth}";

            if (_healthFillImage != null)
                _healthFillImage.fillAmount = _targetFillAmount;
        }
        
        protected override void OnDestroy()
        {
            if (_healthComponent != null)
                _healthComponent.OnHealthChanged -= SetHealth;

            if (_entityStatGroup != null)
                _entityStatGroup.OnStatChanged -= MaxHpChanged;
            
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(PlayerSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(PlayerDestroyed);
        }
    }
}
