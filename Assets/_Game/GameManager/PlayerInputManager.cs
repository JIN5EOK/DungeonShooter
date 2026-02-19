using System;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 조작 관련 입력 세션입니다.
    /// </summary>
    public class PlayerInputManager : IDisposable
    {
        private IEventBus _eventBus;
        private InputManager _inputManager;
        private IPauseManager _pauseManager;
        private EntityInputContext _entityInputContext;
        private Inventory _inventory;
        private IPlayerSkillManager _skillManager;
        private StageSceneUIManager _stageSceneUIManager;

        [Inject]
        public PlayerInputManager(InputManager inputManager,
            IEventBus eventBus,
            IPauseManager pauseManager,
            Inventory inventory,
            StageSceneUIManager stageSceneUIManager,
            IPlayerSkillManager skillManager)
        {
            _inputManager = inputManager;
            _eventBus = eventBus;
            _pauseManager = pauseManager;
            _inventory = inventory;
            _stageSceneUIManager = stageSceneUIManager;
            _skillManager = skillManager;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(OnPlayerObjectDestroyed);
            SubscribeToInput();
        }

        private void OnPlayerObjectSpawned(PlayerObjectSpawnEvent playerObjectSpawnEvent)
        {
            _entityInputContext = playerObjectSpawnEvent.player.EntityInputContext;
        }
        
        private void OnPlayerObjectDestroyed(PlayerObjectDestroyEvent playerObjectDestroyEvent)
        {
            _entityInputContext = null;
        }

        private void SubscribeToInput()
        {
            if (_inputManager == null) 
                return;
            
            _inputManager.OnMoveInputChanged += OnHandleMoveInput;
            _inputManager.OnDashPressed += OnDashInput;
            _inputManager.OnWeaponAttack += OnWeaponAttack;
            _inputManager.OnSkill1Pressed += OnSkill1Input;
            _inputManager.OnSkill2Pressed += OnSkill2Input;
            _inputManager.OnInteractPressed += OnInteractInput;
            _inputManager.OnEscapePressed += OnEscapeInput;
        }

        public void UnsubscribeFromInput()
        {
            if (_inputManager == null)
                return;

            _inputManager.OnMoveInputChanged -= OnHandleMoveInput;
            _inputManager.OnDashPressed -= OnDashInput;
            _inputManager.OnWeaponAttack -= OnWeaponAttack;
            _inputManager.OnSkill1Pressed -= OnSkill1Input;
            _inputManager.OnSkill2Pressed -= OnSkill2Input;
            _inputManager.OnInteractPressed -= OnInteractInput;
            _inputManager.OnEscapePressed -= OnEscapeInput;
        }

        private bool CanProcessGameInput()
        {
            return _entityInputContext != null && !_pauseManager.IsPaused;
        }

        private bool CanProcessEscapeInput()
        {
            return !_pauseManager.IsPaused;
        }

        private void OnHandleMoveInput(Vector2 input)
        {
            if (!CanProcessGameInput())
                return;

            _entityInputContext.MoveInput = input;
        }

        private void OnDashInput(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            _entityInputContext.DashInput = isPressed;
        }

        private void OnWeaponAttack(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(_inventory?.EquippedWeapon?.ActiveSkill, isPressed);
        }

        private void OnSkill1Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(_skillManager?.GetActiveSkill(0), isPressed);
        }

        private void OnSkill2Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(_skillManager?.GetActiveSkill(1), isPressed);
        }

        private void SkillInputInternal(Skill skill, bool isPressed)
        {
            if (isPressed == true)
            {
                _entityInputContext.SkillInput = skill;    
            }
            else if(_entityInputContext.SkillInput == skill)
            {
                _entityInputContext.SkillInput = null;
            }
        }
        
        private void OnInteractInput(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            _entityInputContext.InteractInput = isPressed;
        }

        private void OnEscapeInput(bool isPressed)
        {
            if (!isPressed || !CanProcessEscapeInput())
                return;

            if (!_stageSceneUIManager.IsInventoryActivated())
                _stageSceneUIManager.ShowInventory();
            else
                _stageSceneUIManager.HideInventory();
        }

        public void Dispose()
        {
            if (_inputManager == null) 
                return;

            UnsubscribeFromInput();
        }
    }
}
