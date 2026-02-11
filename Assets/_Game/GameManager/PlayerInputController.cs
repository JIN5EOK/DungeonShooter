using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 조작 관련 입력 컨트롤러 입니다.
    /// </summary>
    public class PlayerInputController
    {
        private InputManager _inputManager;
        private PlayerSkillController _playerSkillController;
        private Inventory _inventory;
        private UIManager _uIManager;
        private bool _isSubscribed;

        private EntityBase _playerInstance;
        private MovementComponent _movementComponent;
        private DashComponent _dashComponent;
        private InteractComponent _interactComponent;

        [Inject]
        private void Construct(InputManager inputManager
            , PlayerSkillController playerSkillController
            , Inventory inventory
            , UIManager uIManager)
        {
            _inputManager = inputManager;
            _playerSkillController = playerSkillController;
            _inventory = inventory;
            _uIManager = uIManager;
        }

        /// <summary>
        /// 제어할 플레이어 엔티티를 바인딩하고 입력 구독을 시작합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            _playerInstance = entity;
            if (entity != null)
            {
                _movementComponent = entity.GetComponent<MovementComponent>();
                _dashComponent = entity.GetComponent<DashComponent>();
                _interactComponent = entity.GetComponent<InteractComponent>();
                entity.OnDestroyed += OnPlayerInstanceDestroyed;
                SubscribeToInput();
            }
            else
            {
                _movementComponent = null;
                _dashComponent = null;
                _interactComponent = null;
            }
        }

        private void OnPlayerInstanceDestroyed(EntityBase entity)
        {
            if (_playerInstance != entity)
                return;

            entity.OnDestroyed -= OnPlayerInstanceDestroyed;
            UnsubscribeFromInput();
            UnbindPlayerInstance();
        }

        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제하고 입력 구독을 중단합니다.
        /// </summary>
        public void UnbindPlayerInstance()
        {
            UnsubscribeFromInput();
            _playerInstance = null;
            _movementComponent = null;
            _dashComponent = null;
            _interactComponent = null;
        }

        public void SubscribeToInput()
        {
            if (_inputManager == null || _isSubscribed) return;
            _isSubscribed = true;
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
            if (_inputManager == null || !_isSubscribed) return;
            _isSubscribed = false;
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
            if (_playerInstance == null) return;
            if (_movementComponent != null)
                _movementComponent.Direction = input;
        }

        private void HandleDashPressed()
        {
            if (_playerInstance == null) return;
            _dashComponent?.StartDash();
        }

        private void HandleWeaponAttack()
        {
            if (_playerInstance == null) return;
            _inventory.EquippedWeapon?.ExecuteActiveSkill(_playerInstance).Forget();
        }

        private void HandleSkill1Pressed()
        {
            if (_playerInstance == null) return;
            _playerSkillController.ExecuteActiveSkill1(_playerInstance);
        }

        private void HandleSkill2Pressed()
        {
            if (_playerInstance == null) return;
            _playerSkillController.ExecuteActiveSkill2(_playerInstance);
        }

        private void HandleInteractPressed()
        {
            if (_playerInstance == null) return;
            _interactComponent ??= _playerInstance.GetComponent<InteractComponent>();
            _interactComponent?.TryInteract();
        }

        private async void HandleEscapePressed()
        {
            var inventoryUI = await _uIManager.CreateUIAsync<InventoryUI>(UIAddresses.UI_Inventory, true);
            if (inventoryUI.gameObject.activeSelf)
                inventoryUI.Hide();
            else
                inventoryUI.Show();
        }
    }
}
