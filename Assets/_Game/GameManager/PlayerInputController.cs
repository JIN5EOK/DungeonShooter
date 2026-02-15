using System;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 조작 관련 입력 세션입니다.
    /// </summary>
    public class PlayerInputController : IDisposable
    {
        private IEventBus _eventBus;
        private InputManager _inputManager;
        private EntityInputContext _entityInputContext;
        private Inventory _inventory;
        private PlayerSkillManager _skillManager;
        private UIManager _uiManager;
        [Inject]
        public PlayerInputController(InputManager inputManager, IEventBus eventBus, Inventory inventory, PlayerSkillManager playerSkillManager,
            UIManager uiManager)
        {
            _inputManager = inputManager;
            _eventBus = eventBus;
            _inventory = inventory;
            _skillManager = playerSkillManager;
            _uiManager = uiManager;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(PlayerObjectDestroyed);
            SubscribeToInput();
        }

        private void PlayerObjectSpawned(PlayerObjectSpawnEvent playerObjectSpawnEvent)
        {
            _entityInputContext = playerObjectSpawnEvent.player.EntityInputContext;
        }
        
        private void PlayerObjectDestroyed(PlayerObjectDestroyEvent playerObjectDestroyEvent)
        {
            _entityInputContext = null;
        }

        private void SubscribeToInput()
        {
            if (_inputManager == null) 
                return;
            
            _inputManager.OnMoveInputChanged += HandleMoveInput;
            _inputManager.OnDashPressed += HandleDashPressed;
            _inputManager.OnWeaponAttack += HandleWeaponAttack;
            _inputManager.OnSkill1Pressed += HandleSkill1Pressed;
            _inputManager.OnSkill2Pressed += HandleSkill2Pressed;
            _inputManager.OnInteractPressed += HandleInteractPressed;
            _inputManager.OnEscapePressed += HandleEscapePressed;
        }

        public void UnsubscribeFromInput()
        {
            if (_inputManager == null ) 
                return;
            
            _inputManager.OnMoveInputChanged -= HandleMoveInput;
            _inputManager.OnDashPressed -= HandleDashPressed;
            _inputManager.OnWeaponAttack -= HandleWeaponAttack;
            _inputManager.OnSkill1Pressed -= HandleSkill1Pressed;
            _inputManager.OnSkill2Pressed -= HandleSkill2Pressed;
            _inputManager.OnInteractPressed -= HandleInteractPressed;
            _inputManager.OnEscapePressed -= HandleEscapePressed;
        }

        private void HandleMoveInput(Vector2 input)
        {
            if (_entityInputContext == null)
                return;
            
            _entityInputContext.moveInput = input;
        }

        private void HandleDashPressed(bool isPressed)
        {
            _entityInputContext.dashInput = isPressed;
        }

        private void HandleWeaponAttack(bool isPressed)
        {
            if (_entityInputContext == null)
                return;
            
            HandleSkillInputInternal(_inventory?.EquippedWeapon?.ActiveSkill, isPressed);
        }

        private void HandleSkill1Pressed(bool isPressed)
        {
            if (_entityInputContext == null)
                return;
            
            HandleSkillInputInternal(_skillManager?.GetActiveSkill(0), isPressed);
        }

        private void HandleSkill2Pressed(bool isPressed)
        {
            if (_entityInputContext == null)
                return;
            
            HandleSkillInputInternal(_skillManager?.GetActiveSkill(1), isPressed);
        }

        private void HandleSkillInputInternal(Skill skill, bool isPressed)
        {
            if (isPressed == true)
            {
                _entityInputContext.skillInput = skill;    
            }
            else if(_entityInputContext.skillInput == skill)
            {
                _entityInputContext.skillInput = null;
            }
        }
        
        private void HandleInteractPressed(bool isPressed)
        {
            if (_entityInputContext == null)
                return;
            
            _entityInputContext.interactInput = isPressed;
        }

        private async void HandleEscapePressed(bool isPressed)
        {
            var inventoryUI = await _uiManager.GetSingletonUIAsync<InventoryUI>(UIAddresses.UI_Inventory);
            if (inventoryUI.gameObject.activeSelf)
                inventoryUI.Hide();
            else
                inventoryUI.Show();
        }

        public void Dispose()
        {
            if (_inputManager == null) 
                return;

            UnsubscribeFromInput();
        }
    }
}
