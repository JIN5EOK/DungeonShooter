using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 입력을 관리하고 이벤트를 제공합니다.
    /// </summary>
    public class InputManager : MonoBehaviour 
    {
        // 이벤트
        public event Action<Vector2> OnMoveInputChanged;
        public event Action<bool> OnDashPressed;
        public event Action<bool> OnWeaponAttack;
        public event Action<bool> OnSkill1Pressed;
        public event Action<bool> OnSkill2Pressed;
        public event Action<bool> OnInteractPressed;

        // 현재 입력 값
        public Vector2 MoveInput => _moveAction.ReadValue<Vector2>();
        public bool IsDashPressed => _dashAction.IsPressed();
        public bool IsWeaponAttackPressed => _weaponAttackAction.IsPressed();
        public bool IsSkill1Pressed => _skill1Action.IsPressed();
        public bool IsSkill2Pressed => _skill2Action.IsPressed();
        public bool IsInteractPressed => _interactAction.IsPressed();

        private InputAction _moveAction;
        private InputAction _weaponAttackAction;
        private InputAction _skill1Action;
        private InputAction _skill2Action;
        private InputAction _dashAction;
        private InputAction _interactAction;
        private void Start()
        {
            var inputAction = GetComponent<PlayerInput>();
            
            // 이동 (WASD)
            _moveAction = inputAction.actions[nameof(InputActionTypes.Move)];
            _moveAction.canceled += _ => OnMoveInputChanged?.Invoke(Vector2.zero);
            _moveAction.performed += (value) => OnMoveInputChanged?.Invoke(value.ReadValue<Vector2>());
            
            // 구르기 (Space)
            
            _dashAction = inputAction.actions[nameof(InputActionTypes.Dash)];
            _dashAction.canceled += _ => OnDashPressed?.Invoke(false);
            _dashAction.started += _ => OnDashPressed?.Invoke(true);
            
            // 무기공격 (J)
            _weaponAttackAction = inputAction.actions[nameof(InputActionTypes.WeaponAttack)];
            _weaponAttackAction.canceled += _ => OnWeaponAttack?.Invoke(false);
            _weaponAttackAction.started += _ => OnWeaponAttack?.Invoke(true);
            
            // 스킬1 (K)
            _skill1Action = inputAction.actions[nameof(InputActionTypes.Skill1)];
            _skill1Action.canceled += _ => OnSkill1Pressed?.Invoke(false);
            _skill1Action.started += _ => OnSkill1Pressed?.Invoke(true);
            
            // 스킬2 (L)
            _skill2Action = inputAction.actions[nameof(InputActionTypes.Skill2)];
            _skill2Action.canceled += _ => OnSkill2Pressed?.Invoke(false);
            _skill2Action.started += _ => OnSkill2Pressed?.Invoke(true);

            // 상호작용 (E)
            _interactAction = inputAction.actions[nameof(InputActionTypes.Interact)];
            _interactAction.canceled += _ => OnInteractPressed?.Invoke(false);
            _interactAction.started += _ => OnInteractPressed?.Invoke(true);
        }

    }
}
