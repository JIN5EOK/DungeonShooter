using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public interface IPlayerDataService
    {
        public IEntityContext EntityContext { get; }
    }

    public class PlayerDataService : IPlayerDataService
    {
        public IEntityContext EntityContext { get; private set; }

        private StageContext _stageContext;
        private ITableRepository _tableRepository;
        private PlayerStatusManager _playerStatusManager;
        private IPlayerSkillManager _playerSkillManager;

        [Inject]
        public async UniTask Initialize(
            StageContext stageContext,
            ITableRepository tableRepository,
            PlayerStatusManager playerStatusManager,
            IPlayerSkillManager playerSkillService)
        {
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _playerStatusManager = playerStatusManager;
            _playerSkillManager = playerSkillService;
            
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogError($"[{nameof(PlayerDataService)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }

            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            IEntityStats entityStats = new EntityStats();
            entityStats.Initialize(statsEntry);

            EntityContext = new EntityContext(
                new EntityInputContext(),
                entityStats,
                new EntityStatuses(statsEntry),
                _playerSkillManager.SkillContainer);
        }
    }
}
