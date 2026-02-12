using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 고유 스킬을 담당합니다.
    /// SkillGroup 소유, 엔티티 바인딩, 스킬 실행/교체 로직을 담당합니다.
    /// </summary>
    public class PlayerSkillSession
    {
        public EntitySkillContainer SkillContainer { get; private set; } = new EntitySkillContainer();
        public EntityBase PlayerInstance => _playerInstance;
        public event Action<int, Skill> OnActiveSkillChanged;
        
        private readonly Skill[] _activeSkills = new Skill[PlayerSkillSlots.Count];
        private EntityBase _playerInstance;

        private ISkillFactory _skillFactory;
        
        [Inject]
        private void Construct(ISkillFactory skillFactory)
        {
            _skillFactory = skillFactory;
        }

        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= PlayerSkillSlots.Count)
            {
                LogHandler.LogWarning<PlayerSkillSession>($"GetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
                return null;
            }

            return _activeSkills[index];
        }

        /// <summary>
        /// 선택한 플레이어 정보로 스킬 세션을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(PlayerConfigTableEntry config)
        {
            if (config == null)
            {
                LogHandler.LogWarning<PlayerSkillSession>("PlayerConfigTableEntry가 null입니다.");
                return;
            }
            SkillContainer?.Clear();
            _activeSkills[PlayerSkillSlots.Skill1Index] = await _skillFactory.CreateSkillAsync(config.Skill1Id);
            _activeSkills[PlayerSkillSlots.Skill2Index] = await _skillFactory.CreateSkillAsync(config.Skill2Id);
            
            if (_activeSkills[PlayerSkillSlots.Skill1Index] != null) 
                SkillContainer?.Regist(_activeSkills[PlayerSkillSlots.Skill1Index]);
            if (_activeSkills[PlayerSkillSlots.Skill2Index] != null) 
                SkillContainer?.Regist(_activeSkills[PlayerSkillSlots.Skill2Index]);
            
            InvokeActiveSkillChanged(PlayerSkillSlots.Skill1Index);
            InvokeActiveSkillChanged(PlayerSkillSlots.Skill2Index);
            
            // 임시코드, 패시브 스킬 등록
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000301));
            SkillContainer?.Regist(await _skillFactory.CreateSkillAsync(14000401));
        }

        /// <summary>
        /// 플레이어 엔티티를 바인딩합니다.
        /// </summary>
        public void BindPlayerInstance(EntityBase entity)
        {
            if (entity == null) return;

            entity.SetSkillGroup(SkillContainer);
            _playerInstance = entity;
            entity.OnDestroyed += UnbindPlayerInstance;
        }

        /// <summary>
        /// 플레이어 엔티티 바인딩을 해제합니다.
        /// </summary>
        public void UnbindPlayerInstance(EntityBase entity)
        {
            if (_playerInstance != entity) return;
            _playerInstance = null;
        }

        /// <summary>
        /// 1번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill1(EntityBase caster)
        {
            if (caster == null) return;
            _activeSkills[PlayerSkillSlots.Skill1Index]?.Execute(caster).Forget();
        }

        /// <summary>
        /// 2번 액티브 스킬을 실행합니다.
        /// </summary>
        public void ExecuteActiveSkill2(EntityBase caster)
        {
            if (caster == null) return;
            _activeSkills[PlayerSkillSlots.Skill2Index]?.Execute(caster).Forget();
        }

        /// <summary>
        /// 기존 스킬을 다음 레벨 스킬로 교체합니다.
        /// </summary>
        public async UniTask ReplaceSkillAsync(Skill oldSkill, SkillTableEntry nextLevelEntry)
        {
            if (oldSkill == null || nextLevelEntry == null)
            {
                LogHandler.LogError<PlayerSkillSession>("파라미터가 올바르지 않습니다.");
                return;
            }

            var newSkill = await _skillFactory.CreateSkillAsync(nextLevelEntry.Id);
            if (newSkill == null)
            {
                LogHandler.LogWarning<PlayerSkillSession>($"다음 레벨 스킬이 없습니다: {nextLevelEntry.Id}");
                return;
            }

            SkillContainer.Unregist(oldSkill, true);
            SkillContainer.Regist(newSkill);

            if (oldSkill == _activeSkills[PlayerSkillSlots.Skill1Index])
            {
                _activeSkills[PlayerSkillSlots.Skill1Index] = newSkill;
                InvokeActiveSkillChanged(PlayerSkillSlots.Skill1Index);
            }
            else if (oldSkill == _activeSkills[PlayerSkillSlots.Skill2Index])
            {
                _activeSkills[PlayerSkillSlots.Skill2Index] = newSkill;
                InvokeActiveSkillChanged(PlayerSkillSlots.Skill2Index);
            }
        }

        private void InvokeActiveSkillChanged(int index)
        {
            OnActiveSkillChanged?.Invoke(index, GetActiveSkill(index));
        }
    }
}
