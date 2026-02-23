using VContainer;

namespace DungeonShooter
{
    public interface IPlayerDataService
    {
        IEntityContext EntityContext { get; }
        IEntityStats StatContainer { get; }
    }

    /// <summary>
    /// 플레이어의 스탯/현재 스테이터스를 담당하며 EntityContext를 제공합니다.
    /// </summary>
    public class PlayerDataService : IPlayerDataService
    {
        public IEntityContext EntityContext { get; private set; }
        public IEntityStats StatContainer => EntityContext?.Stat;

        private StageContext _stageContext;
        private ITableRepository _tableRepository;

        [Inject]
        public void Initialize(
            StageContext stageContext,
            ITableRepository tableRepository)
        {
            _stageContext = stageContext;
            _tableRepository = tableRepository;

            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogError($"[{nameof(PlayerDataService)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            IEntityStats entityStats = new EntityStats();
            entityStats.Initialize(statsEntry);

            var statuses = new EntityStatuses(statsEntry);
            var skillContainer = new EntitySkills();
            EntityContext = new EntityContext(
                new EntityInputContext(),
                entityStats,
                statuses,
                skillContainer);
        }
    }
}
