using System;
using UnityEngine;
using VContainer;
using Jin5eok;

namespace DungeonShooter
{
    /// <summary>
    /// IInputHandler를 사용한 입력 매니저입니다.
    /// 플레이어 입력을 관리하고 이벤트를 제공합니다.
    /// </summary>
    public class InputManager : IDisposable
    {
        // 이동 입력
        private readonly Vector2InputHandlerOldInputSystem _moveInputHandler;
        
        // 버튼 입력들
        private readonly ButtonInputHandlerKeyCode _dashInputHandler;
        private readonly ButtonInputHandlerKeyCode _skill1InputHandler;
        private readonly ButtonInputHandlerKeyCode _skill2InputHandler;
        private readonly ButtonInputHandlerKeyCode _skill3InputHandler;
        private readonly ButtonInputHandlerKeyCode _interactInputHandler;

        // 이벤트
        public event Action<Vector2> OnMoveInputChanged;
        public event Action OnDashPressed;
        public event Action OnWeaponAttack;
        public event Action OnSkill1Pressed;
        public event Action OnSkill2Pressed;
        public event Action OnInteractPressed;

        // 현재 입력 값
        public Vector2 MoveInput => _moveInputHandler.Value;
        public bool IsDashPressed => _dashInputHandler.Value;
        public bool IsWeaponAttackPressed => _skill1InputHandler.Value;
        public bool IsSkill1Pressed => _skill2InputHandler.Value;
        public bool IsSkill2Pressed => _skill3InputHandler.Value;
        public bool IsInteractPressed => _interactInputHandler.Value;

        public InputManager()
        {
            // 이동 입력 (Horizontal, Vertical)
            _moveInputHandler = new Vector2InputHandlerOldInputSystem("Horizontal", "Vertical", isUsingAxisRaw: true);
            _moveInputHandler.InputValueChanged += input => OnMoveInputChanged?.Invoke(input);

            // 구르기 (Space)
            _dashInputHandler = new ButtonInputHandlerKeyCode(KeyCode.Space);
            _dashInputHandler.InputValueChanged += isPressed => { if (isPressed) OnDashPressed?.Invoke(); };

            // 스킬1 (J)
            _skill1InputHandler = new ButtonInputHandlerKeyCode(KeyCode.J);
            _skill1InputHandler.InputValueChanged += isPressed => { if (isPressed) OnWeaponAttack?.Invoke(); };

            // 스킬2 (K)
            _skill2InputHandler = new ButtonInputHandlerKeyCode(KeyCode.K);
            _skill2InputHandler.InputValueChanged += isPressed => { if (isPressed) OnSkill1Pressed?.Invoke(); };

            // 스킬3 (L)
            _skill3InputHandler = new ButtonInputHandlerKeyCode(KeyCode.L);
            _skill3InputHandler.InputValueChanged += isPressed => { if (isPressed) OnSkill2Pressed?.Invoke(); };

            // 상호작용 (E)
            _interactInputHandler = new ButtonInputHandlerKeyCode(KeyCode.E);
            _interactInputHandler.InputValueChanged += isPressed => { if (isPressed) OnInteractPressed?.Invoke(); };
        }

        public void Dispose()
        {
            _moveInputHandler?.Dispose();
            _dashInputHandler?.Dispose();
            _skill1InputHandler?.Dispose();
            _skill2InputHandler?.Dispose();
            _skill3InputHandler?.Dispose();
            _interactInputHandler?.Dispose();
        }
    }
}
