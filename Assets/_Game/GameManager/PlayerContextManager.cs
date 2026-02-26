using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public interface IPlayerContextManager
    {
        public IEntityContext EntityContext { get; }
        public InventoryModel InventoryModel { get; }
        public UniTask InitializeSkillsAsync();
    }

    /// <summary>
    /// 플레이어의 스탯/현재 스테이터스를 담당하며 EntityContext를 제공합니다.
    /// 스킬 초기화는 InitializeSkillsAsync에서 수행하며, 이때 스킬 슬롯 서비스에 액티브 스킬을 등록합니다.
    /// </summary>
    public class PlayerContextManager : IPlayerContextManager
    {
        public IEntityContext EntityContext { get; private set; }
        public InventoryModel InventoryModel { get; } = new InventoryModel();
        private ITableRepository _tableRepository;
        private ISkillFactory _skillFactory;
        private ISkillSlotService _skillSlotService;
        private PlayerConfigTableEntry _playerConfigTableEntry;
        [Inject]
        public void Initialize(
            StageContext stageContext,
            ITableRepository tableRepository,
            ISkillFactory skillFactory,
            ISkillSlotService skillSlotService)
        {
            
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
            _skillSlotService = skillSlotService;
            _playerConfigTableEntry = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(stageContext.PlayerConfigTableId);

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(_playerConfigTableEntry.StatsId);
            IEntityStats entityStats = new EntityStats();
            entityStats.Initialize(statsEntry);

            var statuses = new EntityStatuses(statsEntry);
            var skillContainer = new EntitySkills(_tableRepository);
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

            var skill0 = await _skillFactory.CreateSkillAsync(_playerConfigTableEntry.Skill1Id);
            var skill1 = await _skillFactory.CreateSkillAsync(_playerConfigTableEntry.Skill2Id);

            if (skill0 != null)
                EntityContext?.Skill?.Regist(skill0);
            if (skill1 != null)
                EntityContext?.Skill?.Regist(skill1);

            _skillSlotService.SetActiveSkill(0, skill0);
            _skillSlotService.SetActiveSkill(1, skill1);

            foreach (var acquirableSkillId in _playerConfigTableEntry.AcquirableSkills)
            {
                var skill = await _skillFactory.CreateSkillAsync(acquirableSkillId);
                EntityContext?.Skill?.Regist(skill);
            }
        }
    }
}
