using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 플레이 중 사용하는 버튼 묶음 HUD (인벤토리 열기 등).
    /// </summary>
    public class GameButtonHudUI : HudUI
    {
        [SerializeField]
        private Button _inventoryButton;
        [SerializeField]
        private Button _pauseButton;
        private IInventory _inventory;
        private GamePausePresenter _gamePausePresenter;

        [Inject]
        public void Construct(IInventory inventory, GamePausePresenter gamePausePresenter)
        {
            _inventory = inventory;
            _gamePausePresenter = gamePausePresenter;
        }

        private void Awake()
        {
            if (_inventoryButton != null)
                _inventoryButton.onClick.AddListener(HandleInventoryButtonClicked);
            
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(HandlePauseButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_inventoryButton != null)
                _inventoryButton.onClick.RemoveListener(HandleInventoryButtonClicked);
            
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(HandlePauseButtonClicked);
        }

        private void HandleInventoryButtonClicked()
        {
            _inventory?.Open();
        }
        
        private void HandlePauseButtonClicked()
        {
            _gamePausePresenter?.PauseGame();
        }
    }
}
