using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 전용 UI 매니저. 
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private StageManager _stageManager;
        private UIManager _uiManager;
        private IPlayerFactory _playerFactory;
        
        private HealthBarUI _healthBarUI;
        [Inject]
        public void Construct(StageManager stageManager, UIManager uiManager, IPlayerFactory playerFactory)
        {
            _stageManager = stageManager;
            _uiManager =  uiManager;
            _playerFactory = playerFactory;
        }
        
        private async UniTaskVoid Start()
        {
            _healthBarUI = await _uiManager.CreateUIAsync<HealthBarUI>("UI_HpHud");
            _playerFactory.OnPlayerCreated += PlayerCreated;
            await _stageManager.CreateStageAsync();
        }

        private void PlayerCreated(Player player)
        {
            var healthComponent = player.GetComponent<HealthComponent>();
            healthComponent.OnHealthChanged += _healthBarUI.SetHealth;
            _healthBarUI.SetHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);
        }

        private void OnDestroy()
        {
            _playerFactory.OnPlayerCreated -= PlayerCreated;
            _uiManager.RemoveUI(_healthBarUI);
        }
    }
}
