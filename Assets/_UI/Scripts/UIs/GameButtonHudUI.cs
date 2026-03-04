using System;
using UnityEngine;
using UnityEngine.UI;

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
        public event Action OnInventoryButtonClicked;
        public event Action OnPauseButtonClicked;
        private void Awake()
        {
            if (_inventoryButton != null)
                _inventoryButton.onClick.AddListener(HandleInventoryButtonClicked);
            
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(HandlePauseButtonClicked);
        }

        private void OnDestroy()
        {
            if (_inventoryButton != null)
                _inventoryButton.onClick.RemoveListener(HandleInventoryButtonClicked);
        }

        private void HandleInventoryButtonClicked()
        {
            OnInventoryButtonClicked?.Invoke();
        }
        
        private void HandlePauseButtonClicked()
        {
            OnPauseButtonClicked?.Invoke();
        }
    }
}
