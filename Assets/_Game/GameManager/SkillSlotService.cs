using System;
using VContainer;

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
    }

    /// <summary>
    /// 액티브 스킬 슬롯 상태를 보관하고, 슬롯 변경 시 UI 등에 알린다.
    /// </summary>
    public class SkillSlotService : ISkillSlotService, IDisposable
    {
        public event Action<int, Skill> OnSkillSlotChanged;

        private readonly Skill[] _activeSkillSlots = new Skill[Constants.SkillSlotMaxCount];
        private IEventBus _eventBus;

        [Inject]
        private void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<SkillLevelUpEvent>(OnSkillLevelChanged);
        }

        /// <summary>
        /// 액티브 스킬 슬롯에 등록된 스킬의 레벨 변경 처리
        /// </summary>
        private void OnSkillLevelChanged(SkillLevelUpEvent skillLevelUpEvent)
        {
            for (var i = 0; i < _activeSkillSlots.Length; i++)
            {
                if (skillLevelUpEvent.beforeSkill == _activeSkillSlots[i])
                {
                    _activeSkillSlots[i] = skillLevelUpEvent.afterSkill;
                    OnSkillSlotChanged?.Invoke(i, skillLevelUpEvent.afterSkill);
                }
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

        public void Dispose()
        {
            _eventBus.Unsubscribe<SkillLevelUpEvent>(OnSkillLevelChanged);
        }
    }
}
