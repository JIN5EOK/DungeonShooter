using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public interface IActiveSkillSlotGroup
    {
        public event Action<int, Skill> OnActiveSkillSlotChanged;
        public Skill GetActiveSkill(int index);
    }
    
    public interface IPlayerSkillManager : IActiveSkillSlotGroup
    {
        public UniTask InitializeAsync(PlayerConfigTableEntry config);
        public EntitySkillContainer SkillContainer { get; }
    }
    
    /// <summary>
    /// 플레이어 고유 스킬을 담당합니다.
    /// </summary>
    public class PlayerSkillManager : IPlayerSkillManager, IDisposable
    {
        public event Action<int, Skill> OnActiveSkillSlotChanged;
        private readonly Skill[] _activeSkillSlots = new Skill[Constants.SkillSlotMaxCount];
        
        public EntitySkillContainer SkillContainer { get; private set; }
        
        private ISkillFactory _skillFactory;
        private IEventBus _eventBus;
        [Inject]
        private void Construct(ISkillFactory skillFactory, IEventBus eventBus, EntitySkillContainer skillContainer)
        {
            _skillFactory = skillFactory;
            _eventBus = eventBus;
            SkillContainer = skillContainer;
            _eventBus.Subscribe<SkillLevelUpEvent>(OnSkillLevelChanged);
        }

        /// <summary>
        /// 액티브 스킬 슬롯에 등록된 스킬의 레벨 변경처리
        /// </summary>
        private void OnSkillLevelChanged(SkillLevelUpEvent skillLevelUpEvent)
        {
            // 비효율적.. 개선 필요할듯
            for (var i = 0; i < _activeSkillSlots.Length; i++)
            {
                if (skillLevelUpEvent.beforeSkill == _activeSkillSlots[i])
                {
                    _activeSkillSlots[i] = skillLevelUpEvent.afterSkill;
                    OnActiveSkillSlotChanged?.Invoke(i, skillLevelUpEvent.afterSkill);
                }
            }
        }

        /// <summary>
        /// 선택한 플레이어 정보로 스킬 세션을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<IPlayerSkillManager>("PlayerConfigTableEntry가 null입니다.");
                return;
            }
            
            SkillContainer?.Clear();
            
            // 액티브 슬롯에 액티브 스킬 등록
            _activeSkillSlots[0] = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _activeSkillSlots[1] = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_activeSkillSlots[0] != null) 
                SkillContainer?.Regist(_activeSkillSlots[0]);
            if (_activeSkillSlots[1] != null) 
                SkillContainer?.Regist(_activeSkillSlots[1]);

            OnActiveSkillSlotChanged?.Invoke(0, _activeSkillSlots[0]);
            OnActiveSkillSlotChanged?.Invoke(1, _activeSkillSlots[1]);

            // 그 외 스킬 등록
            foreach (var acquirableSkillId in config.AcquirableSkills)
            {
                SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(acquirableSkillId));
            }
        }
        
        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= Constants.SkillSlotMaxCount)
            {
                LogHandler.LogWarning<IPlayerSkillManager>($"GetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
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
