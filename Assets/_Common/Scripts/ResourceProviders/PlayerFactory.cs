using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IPlayerFactory
    {
        UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성/이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly PlayerStatusManager _playerStatusManager;
        private readonly IPlayerSkillManager _playerSkillManager;
        private readonly IEventBus _eventBus;
        private readonly LifetimeScope _sceneLifetimeScope;
        [Inject]
        public PlayerFactory(StageContext context
            , ISceneResourceProvider sceneResourceProvider
            , ITableRepository tableRepository
            , IEventBus eventBus
            , PlayerStatusManager playerStatusManager
            , IPlayerSkillManager playerSkillManager
            , LifetimeScope sceneLifetimeScope)
        {
            _stageContext = context;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
            _eventBus = eventBus;
            _playerStatusManager = playerStatusManager;
            _playerSkillManager = playerSkillManager;
            _sceneLifetimeScope = sceneLifetimeScope;
        }

        /// <summary>
        /// 플레이어 캐릭터를 가져옵니다. 이미 플레이어가 있으면 파괴한 뒤 새로 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress();
                var playerInstance = await _sceneResourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = await InitializePlayerInstance(playerInstance);
                return entity;
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 가져옵니다. 이미 플레이어가 있으면 파괴한 뒤 새로 생성합니다.
        /// </summary>
        public EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress();
                var playerInstance = _sceneResourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = InitializePlayerInstance(playerInstance).GetAwaiter().GetResult();
                return entity;
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            if (string.IsNullOrEmpty(config.GameObjectKey))
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 게임오브젝트 키가 설정되지 않았습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            return config.GameObjectKey;
        }

        /// <summary>
        /// Player 게임오브젝트 초기화, 컴포넌트 부착, 바인딩, UI 연동
        /// </summary>
        private async UniTask<EntityBase> InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            // 기존 플레이어가 있다면 파괴,바인딩 해제
            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;

            // 씬 LifeTimeScope를 부모로 삼기
            EntityLifeTimeScope entityLifeTimeScope = null;
            using (LifetimeScope.EnqueueParent(_sceneLifetimeScope))
            {
                entityLifeTimeScope = playerInstance.AddOrGetComponent<EntityLifeTimeScope>();    
            }
            
            var entity = entityLifeTimeScope.Container.Resolve<EntityBase>();
            var movementCompoent = entityLifeTimeScope.Container.Resolve<MovementComponent>();
            var interactComponent = entityLifeTimeScope.Container.Resolve<InteractComponent>();
            var dashComponent = entityLifeTimeScope.Container.Resolve<DashComponent>();
            var healthComponent = entityLifeTimeScope.Container.Resolve<HealthComponent>();
            var cameraTrackComponent = entityLifeTimeScope.Container.Resolve<CameraTrackComponent>();
            var stateMachine = entityLifeTimeScope.Container.Resolve<IEntityStateMachine>();

            var interactNotice = await _sceneResourceProvider.GetInstanceAsync(CommonAddresses.InteractNotice);
            interactComponent.SetInteractNotice(interactNotice);

            await cameraTrackComponent.AttachCameraAsync();
            
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            
            entity.OnDestroyed += (self) =>
            {
                _eventBus.Publish(new PlayerObjectDestroyEvent {player = self, position = playerInstance.transform.position});
            };

            healthComponent.OnDeath += () =>
            {
                Object.Destroy(entity.gameObject);
                _eventBus.Publish(new PlayerDeadEvent() {player = entity, position = playerInstance.transform.position, playerConfigTableEntry = config});
            };
            
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(config.StatsId);
            var context = new EntityContext(
                new EntityInputContext(),
                _playerStatusManager.StatContainer,
                new EntityStatus(statsEntry),
                _playerSkillManager.SkillContainer);
            entity.SetContext(context);

            _eventBus.Publish(new PlayerObjectSpawnEvent{ player = entity, playerConfigTableEntry = config, position = playerInstance.transform.position});
            return entity;
        }
    }
}
