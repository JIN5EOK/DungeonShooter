using System;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스킬 쿨다운 UI가 구독하는 뷰모델. 서비스 상태 변화를 구독해 노출한다.
    /// </summary>
    public interface ISkillSlotViewModel
    {
        public event Action<int, Skill> OnSkillSlotChanged;
        public Skill GetActiveSkill(int index);
    }
    
    /// <summary>
    /// IPlayerSkillManager의 상태 변화를 구독하여 스킬 쿨다운 HUD에 노출한다.
    /// </summary>
    public class SkillSlotViewModel : ISkillSlotViewModel
    {
        private readonly IPlayerSkillManager _playerSkillManager;

        public event Action<int, Skill> OnSkillSlotChanged;

        [Inject]
        public SkillSlotViewModel(IPlayerSkillManager playerSkillManager)
        {
            _playerSkillManager = playerSkillManager;
            _playerSkillManager.OnSkillSlotChanged += OnServiceSkillSlotChanged;
        }

        private void OnServiceSkillSlotChanged(int index, Skill skill)
        {
            OnSkillSlotChanged?.Invoke(index, skill);
        }

        public Skill GetActiveSkill(int index)
        {
            return _playerSkillManager.GetActiveSkill(index);
        }
    }
}