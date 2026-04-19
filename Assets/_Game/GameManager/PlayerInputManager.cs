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

            SkillInputInternal(GetActiveSkillEntryId(0), isPressed);
        }

        private void OnSkill1Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(GetActiveSkillEntryId(0), isPressed);
        }

        private void OnSkill2Input(bool isPressed)
        {
            if (!CanProcessGameInput())
                return;

            SkillInputInternal(GetActiveSkillEntryId(1), isPressed);
        }

        private int GetActiveSkillEntryId(int index)
        {
            if (_entitySkills == null)
                return 0;

            var skill = _entitySkills
                .GetSkills()
                .Where(s => s?.SkillData != null && s.SkillData.IsActiveSkill)
                .ElementAtOrDefault(index);
            return skill?.SkillTableEntrySo?.Id ?? 0;
        }

        private void SkillInputInternal(int skillEntryId, bool isPressed)
        {
            if (isPressed)
            {
                if (skillEntryId != 0)
                {
                    _entityInputContext.SkillInput = skillEntryId;
                }
            }
            else if (_entityInputContext.SkillInput == skillEntryId)
            {
                _entityInputContext.SkillInput = 0;
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
