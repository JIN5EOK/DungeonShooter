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
        public const int Count = 2;
        public const int Skill1Index = 0;
        public const int Skill2Index = 1;
        
        [Inject]
        public EntitySkillContainer SkillContainer { get; private set; }
        public EntityBase PlayerInstance { get; private set; }
        private readonly Skill[] _activeSkills = new Skill[Count];

        private ISkillFactory _skillFactory;
        private IEventBus _eventBus;
        [Inject]
        private void Construct(ISkillFactory skillFactory, IEventBus eventBus)
        {
            _skillFactory = skillFactory;
            _eventBus = eventBus;
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(OnPlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(OnPlayerObjectDestroyed);
            _eventBus.Subscribe<SkillLevelUpEvent>(OnSkillLevelChanged);
        }

        private void OnSkillLevelChanged(SkillLevelUpEvent skillLevelUpEvent)
        {
            // 비효율적.. 개선 필요할듯
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
        private void OnPlayerObjectSpawned(PlayerObjectSpawnEvent spawnEvent)
        {
            PlayerInstance = spawnEvent.player;
        }

        /// <summary>플레이어 오브젝트 파괴 이벤트 </summary>
        private void OnPlayerObjectDestroyed(PlayerObjectDestroyEvent destroyEvent)
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
            
            _activeSkills[Skill1Index] = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _activeSkills[Skill2Index] = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_activeSkills[Skill1Index] != null) 
                SkillContainer?.Regist(_activeSkills[Skill1Index]);
            if (_activeSkills[Skill2Index] != null) 
                SkillContainer?.Regist(_activeSkills[Skill2Index]);

            // 임시코드, 패시브 스킬 등록
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000301));
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000401));
        }
        
        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= Count)
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

            if (index < 0 || index >= Count)
                return;
            
            _activeSkills[index]?.Execute(PlayerInstance).Forget();
        }
        
        public void Dispose()
        {
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(OnPlayerObjectSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(OnPlayerObjectDestroyed);
            _eventBus.Unsubscribe<SkillLevelUpEvent>(OnSkillLevelChanged);
        }
    }
}
