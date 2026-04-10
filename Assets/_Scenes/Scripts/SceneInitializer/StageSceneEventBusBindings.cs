using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬에서 IEventBus 구독을 한곳에 모읍니다.
    /// Register는 StageSceneLifeTimeScope 컨테이너 빌드 콜백에서, Unregister는 StageSceneInitializer.OnDestroy에서 호출합니다.
    /// </summary>
    public class StageSceneEventBusBindings
    {
        private readonly IEventBus _eventBus;
        private readonly PlayerInputManager _playerInputManager;
        private readonly SkillLevelUpUI _skillLevelUpUI;
        private readonly ISkillSlotService _skillSlotService;
        private readonly IItemDropService _itemDropService;
        private readonly IGameResultService _gameResultService;
        private readonly IInventory _inventory;
        private readonly ObjectCullingManager _objectCullingManager;
        private readonly EntityManager _entityManager;

        [Inject]
        public StageSceneEventBusBindings(
            IEventBus eventBus,
            PlayerInputManager playerInputManager,
            SkillLevelUpUI skillLevelUpUI,
            ISkillSlotService skillSlotService,
            IItemDropService itemDropService,
            IGameResultService gameResultService,
            IInventory inventory,
            ObjectCullingManager objectCullingManager,
            EntityManager entityManager)
        {
            _eventBus = eventBus;
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
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(_playerInputManager.OnPlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(_playerInputManager.OnPlayerObjectDestroyed);

            _eventBus.Subscribe<PlayerLevelChangeEvent>(_skillLevelUpUI.OnPlayerLevelChanged);

            _eventBus.Subscribe<SkillLevelUpEvent>(_skillSlotService.OnSkillLevelChanged);

            _eventBus.Subscribe<EnemyDeadEvent>(_itemDropService.OnEnemyDead);
            _eventBus.Subscribe<EnemyDeadEvent>(_gameResultService.OnEnemyDead);

            _eventBus.Subscribe<PlayerObjectSpawnEvent>(_inventory.OnPlayerObjectSpawned);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(_inventory.OnPlayerObjectDespawned);

            _eventBus.Subscribe<EnemySpawnedEvent>(_objectCullingManager.OnEnemySpawnedForCulling);
            _eventBus.Subscribe<PlayerObjectSpawnEvent>(_objectCullingManager.OnPlayerSpawnedForCulling);
            _eventBus.Subscribe<PlayerObjectDestroyEvent>(_objectCullingManager.OnPlayerDestroyedForCulling);

            _eventBus.Subscribe<EnemySpawnedEvent>(_entityManager.OnEnemySpawned);
            _eventBus.Subscribe<EnemyDeadEvent>(_entityManager.OnEnemyDestroyed);
            _eventBus.Subscribe<PlayerDeadEvent>(_entityManager.OnPlayerDead);
        }

        public void Unregister()
        {
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(_playerInputManager.OnPlayerObjectSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(_playerInputManager.OnPlayerObjectDestroyed);

            _eventBus.Unsubscribe<PlayerLevelChangeEvent>(_skillLevelUpUI.OnPlayerLevelChanged);

            _eventBus.Unsubscribe<SkillLevelUpEvent>(_skillSlotService.OnSkillLevelChanged);

            _eventBus.Unsubscribe<EnemyDeadEvent>(_itemDropService.OnEnemyDead);
            _eventBus.Unsubscribe<EnemyDeadEvent>(_gameResultService.OnEnemyDead);

            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(_inventory.OnPlayerObjectSpawned);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(_inventory.OnPlayerObjectDespawned);

            _eventBus.Unsubscribe<EnemySpawnedEvent>(_objectCullingManager.OnEnemySpawnedForCulling);
            _eventBus.Unsubscribe<PlayerObjectSpawnEvent>(_objectCullingManager.OnPlayerSpawnedForCulling);
            _eventBus.Unsubscribe<PlayerObjectDestroyEvent>(_objectCullingManager.OnPlayerDestroyedForCulling);

            _eventBus.Unsubscribe<EnemySpawnedEvent>(_entityManager.OnEnemySpawned);
            _eventBus.Unsubscribe<EnemyDeadEvent>(_entityManager.OnEnemyDestroyed);
            _eventBus.Unsubscribe<PlayerDeadEvent>(_entityManager.OnPlayerDead);
        }
    }
}
