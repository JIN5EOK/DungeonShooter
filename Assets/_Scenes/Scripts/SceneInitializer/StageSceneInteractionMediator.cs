using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬에서 노출된 이벤트와 구독자를 연결하는 중재자입니다.
    /// Register는 StageSceneLifeTimeScope 컨테이너 빌드 콜백에서, Unregister는 StageSceneInitializer.OnDestroy에서 호출합니다.
    /// </summary>
    public class StageSceneInteractionMediator
    {
        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IPlayerLevelService _playerLevelService;
        private readonly ISkillService _skillService;
        private readonly PlayerInputManager _playerInputManager;
        private readonly SkillLevelUpUI _skillLevelUpUI;
        private readonly ISkillSlotService _skillSlotService;
        private readonly IItemDropService _itemDropService;
        private readonly IGameResultService _gameResultService;
        private readonly IInventory _inventory;
        private readonly ObjectCullingManager _objectCullingManager;
        private readonly EntityManager _entityManager;

        [Inject]
        public StageSceneInteractionMediator(
            IPlayerFactory playerFactory,
            IEnemyFactory enemyFactory,
            IPlayerLevelService playerLevelService,
            ISkillService skillService,
            PlayerInputManager playerInputManager,
            SkillLevelUpUI skillLevelUpUI,
            ISkillSlotService skillSlotService,
            IItemDropService itemDropService,
            IGameResultService gameResultService,
            IInventory inventory,
            ObjectCullingManager objectCullingManager,
            EntityManager entityManager)
        {
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _playerLevelService = playerLevelService;
            _skillService = skillService;
            _playerInputManager = playerInputManager;
            _skillLevelUpUI = skillLevelUpUI;
            _skillSlotService = skillSlotService;
            _itemDropService = itemDropService;
            _gameResultService = gameResultService;
            _inventory = inventory;
            _objectCullingManager = objectCullingManager;
            _entityManager = entityManager;
        }

        public void Register()
        {
            _playerFactory.PlayerObjectSpawned += _playerInputManager.OnPlayerObjectSpawned;
            _playerFactory.PlayerObjectDestroyed += _playerInputManager.OnPlayerObjectDestroyed;

            _playerLevelService.OnLevelChanged += _skillLevelUpUI.OnPlayerLevelChanged;

            _skillService.OnSkillLeveledUp += _skillSlotService.OnSkillLevelChanged;

            _enemyFactory.EnemyDied += _itemDropService.OnEnemyDead;
            _enemyFactory.EnemyDied += _gameResultService.OnEnemyDead;

            _playerFactory.PlayerObjectSpawned += _inventory.OnPlayerObjectSpawned;
            _playerFactory.PlayerObjectDestroyed += _inventory.OnPlayerObjectDespawned;

            _enemyFactory.EnemySpawned += _objectCullingManager.OnEnemySpawnedForCulling;
            _playerFactory.PlayerObjectSpawned += _objectCullingManager.OnPlayerSpawnedForCulling;
            _playerFactory.PlayerObjectDestroyed += _objectCullingManager.OnPlayerDestroyedForCulling;

            _enemyFactory.EnemySpawned += _entityManager.OnEnemySpawned;
            _enemyFactory.EnemyDied += _entityManager.OnEnemyDestroyed;
            _playerFactory.PlayerDied += _entityManager.OnPlayerDead;
        }

        public void Unregister()
        {
            _playerFactory.PlayerObjectSpawned -= _playerInputManager.OnPlayerObjectSpawned;
            _playerFactory.PlayerObjectDestroyed -= _playerInputManager.OnPlayerObjectDestroyed;

            _playerLevelService.OnLevelChanged -= _skillLevelUpUI.OnPlayerLevelChanged;

            _skillService.OnSkillLeveledUp -= _skillSlotService.OnSkillLevelChanged;

            _enemyFactory.EnemyDied -= _itemDropService.OnEnemyDead;
            _enemyFactory.EnemyDied -= _gameResultService.OnEnemyDead;

            _playerFactory.PlayerObjectSpawned -= _inventory.OnPlayerObjectSpawned;
            _playerFactory.PlayerObjectDestroyed -= _inventory.OnPlayerObjectDespawned;

            _enemyFactory.EnemySpawned -= _objectCullingManager.OnEnemySpawnedForCulling;
            _playerFactory.PlayerObjectSpawned -= _objectCullingManager.OnPlayerSpawnedForCulling;
            _playerFactory.PlayerObjectDestroyed -= _objectCullingManager.OnPlayerDestroyedForCulling;

            _enemyFactory.EnemySpawned -= _entityManager.OnEnemySpawned;
            _enemyFactory.EnemyDied -= _entityManager.OnEnemyDestroyed;
            _playerFactory.PlayerDied -= _entityManager.OnPlayerDead;
        }
    }
}
