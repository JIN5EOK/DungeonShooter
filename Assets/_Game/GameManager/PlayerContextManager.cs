using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    public interface IPlayerContextManager
    {
        public IEntityContext EntityContext { get; }
        public InventoryModel InventoryModel { get; }
        public event Action<int, Skill> OnActiveSkillSlotChanged;
        public Skill GetActiveSkill(int index);
        public void ReplaceActiveSkillSlot(Skill beforeSkill, Skill afterSkill);
        public void Initialize(int playerConfigTableId);
        public UniTask InitializeSkillsAsync();
    }

    /// <summary>
    /// 플레이어의 스탯/현재 스테이터스를 담당하며 EntityContext를 제공합니다.
    /// 스킬 초기화는 InitializeSkillsAsync에서 수행하며, 이때 스킬 슬롯 서비스에 액티브 스킬을 등록합니다.
    /// </summary>
    public class PlayerContextManager : IPlayerContextManager
    {
        public event Action<int, Skill> OnActiveSkillSlotChanged;

        public IEntityContext EntityContext { get; private set; }
        public InventoryModel InventoryModel { get; } = new InventoryModel();
        private ITableRepository _tableRepository;
        private ISkillFactory _skillFactory;
        private PlayerConfigSo _playerConfigSo;
        private readonly Skill[] _activeSkillSlots = new Skill[Constants.SkillSlotMaxCount];
        
        [Inject]
        public PlayerContextManager(
            ITableRepository tableRepository,
            ISkillFactory skillFactory)
        {
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
        }

        public Skill GetActiveSkill(int index)
        {
            if (index < 0 || index >= Constants.SkillSlotMaxCount)
            {
                LogHandler.LogWarning<IPlayerContextManager>($"GetActiveSkill: 잘못된 인덱스 입니다. index: {index}");
                return null;
            }

            return _activeSkillSlots[index];
        }

        private void SetActiveSkillSlot(int index, Skill skill)
        {
            if (index < 0 || index >= Constants.SkillSlotMaxCount)
            {
                LogHandler.LogWarning<IPlayerContextManager>($"SetActiveSkillSlot: 잘못된 인덱스 입니다. index: {index}");
                return;
            }

            _activeSkillSlots[index] = skill;
            OnActiveSkillSlotChanged?.Invoke(index, skill);
        }

        public void ReplaceActiveSkillSlot(Skill beforeSkill, Skill afterSkill)
        {
            if (beforeSkill == null || afterSkill == null)
                return;

            for (var i = 0; i < _activeSkillSlots.Length; i++)
            {
                if (_activeSkillSlots[i] != beforeSkill)
                    continue;

                SetActiveSkillSlot(i, afterSkill);
            }
        }

        /// <summary>
        /// 스테이지 씬에서 선택된 PlayerConfig 테이블 ID를 반영합니다. <see cref="StageSceneInitializer"/>에서만 호출합니다.
        /// </summary>
        public void Initialize(int playerConfigTableId)
        {
            _playerConfigSo = _tableRepository.GetTableEntry<PlayerConfigSo>(playerConfigTableId);
            if (_playerConfigSo == null)
            {
                return;
            }

            var statsDto = _playerConfigSo.Stats;
            IEntityStats entityStats = new EntityStats();
            entityStats.Initialize(statsDto);

            var statuses = new EntityStatuses(statsDto);
            var skillContainer = new EntitySkills();
            EntityContext = new EntityContext(
                new EntityInputContext(),
                entityStats,
                statuses,
                skillContainer);
        }

        /// <summary>
        /// 선택한 플레이어 설정에 따라 스킬을 생성·등록하고, 액티브 슬롯 서비스에 슬롯을 등록합니다.
        /// </summary>
        public async UniTask InitializeSkillsAsync()
        {
            EntityContext?.Skill?.Clear();

            if (_playerConfigSo == null)
                return;

            var skill0 = await _skillFactory.CreateSkillAsync(_playerConfigSo.Skill1Ref);
            var skill1 = await _skillFactory.CreateSkillAsync(_playerConfigSo.Skill2Ref);

            if (skill0 != null)
                EntityContext?.Skill?.Regist(skill0);
            if (skill1 != null)
                EntityContext?.Skill?.Regist(skill1);

            SetActiveSkillSlot(0, skill0);
            SetActiveSkillSlot(1, skill1);

            foreach (var skillRef in _playerConfigSo?.Skills)
            {
                var skill = await _skillFactory.CreateSkillAsync(skillRef);
                EntityContext?.Skill?.Regist(skill);
            }
        }
    }
}
