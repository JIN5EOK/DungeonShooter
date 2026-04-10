using System;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 액티브 스킬 슬롯을 담당한다. 슬롯 변경 시 OnSkillSlotChanged를 발생시킨다.
    /// </summary>
    public interface ISkillSlotService
    {
        public event Action<int, Skill> OnSkillSlotChanged;
        public Skill GetActiveSkill(int index);
        public void SetActiveSkill(int index, Skill skill);
        public void ReplaceSkillSlot(Skill beforeSkill, Skill afterSkill);
    }

    /// <summary>
    /// 액티브 스킬 슬롯 상태를 보관하고, 슬롯 변경 시 UI 등에 알린다.
    /// </summary>
    public class SkillSlotService : ISkillSlotService
    {
        public event Action<int, Skill> OnSkillSlotChanged;

        private readonly Skill[] _activeSkillSlots = new Skill[Constants.SkillSlotMaxCount];

        /// <inheritdoc />
        public void ReplaceSkillSlot(Skill beforeSkill, Skill afterSkill)
        {
            if (beforeSkill == null || afterSkill == null)
                return;

            for (var i = 0; i < _activeSkillSlots.Length; i++)
            {
                if (beforeSkill != _activeSkillSlots[i])
                    continue;

                _activeSkillSlots[i] = afterSkill;
                OnSkillSlotChanged?.Invoke(i, afterSkill);
            }
        }

        /// <summary>
        /// 액티브 슬롯에 스킬을 등록한다. 스킬 생성·컨테이너 등록은 호출 측(PlayerDataService 등)에서 수행한다.
        /// </summary>
        public void SetActiveSkill(int index, Skill skill)
        {
            if (index < 0 || index >= Constants.SkillSlotMaxCount)
            {
                LogHandler.LogWarning<ISkillSlotService>($"SetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
                return;
            }

            _activeSkillSlots[index] = skill;
            OnSkillSlotChanged?.Invoke(index, skill);
        }

        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= Constants.SkillSlotMaxCount)
            {
                LogHandler.LogWarning<ISkillSlotService>($"GetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
                return null;
            }

            return _activeSkillSlots[index];
        }
    }
}
