using System;
using System.Linq;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 조작 관련 입력 세션입니다.
    /// </summary>
    public class PlayerInputManager : IDisposable
    {
        private InputManager _inputManager;
        private IPauseManager _pauseManager;
        private IEntityInputContext _entityInputContext;
        private IEntitySkills _entitySkills;

        [Inject]
        public PlayerInputManager(InputManager inputManager,
            IPauseManager pauseManager)
        {
            _inputManager = inputManager;
            _pauseManager = pauseManager;
            SubscribeToInput();
        }

        /// <summary>조작 입력을 전달할 엔티티를 지정합니다.</summary>
        public void BindControlledEntity(EntityBase entity)
        {
            _entityInputContext = entity != null ? entity.EntityContext.InputContext : null;
            _entitySkills = entity != null ? entity.EntityContext.Skill : null;
        }

        /// <summary><see cref="BindControlledEntity"/> 연결을 해제합니다.</summary>
        public void UnbindControlledEntity()
        {
            _entityInputContext = null;
            _entitySkills = null;
        }

        private void SubscribeToInput()
        {
            if (_inputManager == null) 
                return;
            
            _inputManager.OnMoveInputChanged += OnHandleMoveInput;
            _inputManager.OnWeaponAttack += OnWeaponAttack;
            _inputManager.OnSkill1Pressed += OnSkill1Input;
            _inputManager.OnSkill2Pressed += OnSkill2Input;
        }

        public void UnsubscribeFromInput()
        {
            if (_inputManager == null)
                return;

            _inputManager.OnMoveInputChanged -= OnHandleMoveInput;
            _inputManager.OnWeaponAttack -= OnWeaponAttack;
            _inputManager.OnSkill1Pressed -= OnSkill1Input;
            _inputManager.OnSkill2Pressed -= OnSkill2Input;
        }

        private bool CanProcessGameInput()
        {
            return _entityInputContext != null && !_pauseManager.IsPaused;
        }

        private void OnHandleMoveInput(Vector2 input)
        {
            if (_entityInputContext != null)
            {
                _entityInputContext.MoveInput = !CanProcessGameInput() ? Vector2.zero : input;    
            }
        }

        private void OnWeaponAttack(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(GetActiveSkill(0), isPressed);
        }

        private void OnSkill1Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(GetActiveSkill(0), isPressed);
        }

        private void OnSkill2Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(GetActiveSkill(1), isPressed);
        }

        private Skill GetActiveSkill(int index)
        {
            if (_entitySkills == null)
                return null;

            return _entitySkills
                .GetRegistedSkills()
                .Where(s => s?.SkillData != null && s.SkillData.IsActiveSkill)
                .ElementAtOrDefault(index);
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
        

        public void Dispose()
        {
            if (_inputManager == null) 
                return;

            UnsubscribeFromInput();
        }
    }
}
