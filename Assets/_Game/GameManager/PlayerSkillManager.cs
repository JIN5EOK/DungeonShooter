using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 고유 스킬을 담당합니다.
    /// </summary>
    public class PlayerSkillManager : IDisposable
    {
        [Inject]
        public EntitySkillContainer SkillContainer { get; private set; }
        public EntityBase PlayerInstance { get; private set; }
        private readonly Skill[] _activeSkills = new Skill[PlayerSkillSlots.Count];

        private ISkillFactory _skillFactory;
        private IEventBus _eventBus;
        [Inject]
        private void Construct(ISkillFactory skillFactory, IEventBus eventBus)
        {
            _skillFactory = skillFactory;
            _eventBus = eventBus;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(PlayerObjectDestroyed);
            _eventBus.Subscribe<SkillLevelUpEvent>(SkillLevelUped);
        }

        private void SkillLevelUped(SkillLevelUpEvent skillLevelUpEvent)
        {
            for (var i = 0; i < _activeSkills.Length; i++)
            {
                var activeSkill = _activeSkills[i];
                if (skillLevelUpEvent.beforeSkill == activeSkill)
                {
                    _activeSkills[i] = skillLevelUpEvent.afterSkill;
                }
            }
        }
        
        /// <summary>플레이어 오브젝트 생성 이벤트 </summary>
        private void PlayerObjectSpawned(PlayerObjectSpawnEvent spawnEvent)
        {
            spawnEvent.player.SetSkillGroup(SkillContainer);
            PlayerInstance = spawnEvent.player;
        }

        /// <summary>플레이어 오브젝트 파괴 이벤트 </summary>
        private void PlayerObjectDestroyed(PlayerObjectDestroyEvent destroyEvent)
        {
            PlayerInstance = null;
        }

        /// <summary>
        /// 선택한 플레이어 정보로 스킬 세션을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerSkillManager>("PlayerConfigTableEntry가 null입니다.");
                return;
            }
            
            SkillContainer?.Clear();
            
            _activeSkills[PlayerSkillSlots.Skill1Index] = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _activeSkills[PlayerSkillSlots.Skill2Index] = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_activeSkills[PlayerSkillSlots.Skill1Index] != null) 
                SkillContainer?.Regist(_activeSkills[PlayerSkillSlots.Skill1Index]);
            if (_activeSkills[PlayerSkillSlots.Skill2Index] != null) 
                SkillContainer?.Regist(_activeSkills[PlayerSkillSlots.Skill2Index]);

            // 임시코드, 패시브 스킬 등록
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000301));
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000401));
        }
        
        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= PlayerSkillSlots.Count)
            {
                LogHandler.LogWarning<PlayerSkillManager>($"GetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
                return null;
            }

            return _activeSkills[index];
        }
        
        /// <summary>
        /// 지정한 인덱스의 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill(int index)
        {
            if (PlayerInstance == null) 
                return;

            if (index < 0 || index >= PlayerSkillSlots.Count)
                return;
            
            _activeSkills[index]?.Execute(PlayerInstance).Forget();
        }

        /// <summary>
        /// 기존 스킬을 다음 레벨 스킬로 교체합니다.
        /// </summary>
        public async UniTask ReplaceSkillAsync(Skill oldSkill, SkillTableEntry nextLevelEntry)
        {
            if (oldSkill == null || nextLevelEntry == null)
            {
                LogHandler.LogError<PlayerSkillManager>("파라미터가 올바르지 않습니다.");
                return;
            }

            var newSkill = await _skillFactory.CreateSkillAsync(nextLevelEntry.Id);
            if (newSkill == null)
            {
                LogHandler.LogWarning<PlayerSkillManager>($"다음 레벨 스킬이 없습니다: {nextLevelEntry.Id}");
                return;
            }

            _eventBus.Publish(new SkillLevelUpEvent() {beforeSkill = oldSkill, afterSkill = newSkill});
            
            SkillContainer.Unregist(oldSkill, true);
            SkillContainer.Regist(newSkill);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(PlayerObjectSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(PlayerObjectDestroyed);
            _eventBus.Unsubscribe<SkillLevelUpEvent>(SkillLevelUped);
        }
    }
}
