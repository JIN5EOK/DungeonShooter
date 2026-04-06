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
        UniTask<EntityBase> GetPlayerAsync(int playerConfigId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerSync(int playerConfigId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성/이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly IPlayerContextManager _playerContextManager;
        private readonly IEventBus _eventBus;
        private readonly LifetimeScope _sceneLifetimeScope;
        [Inject]
        public PlayerFactory(IResourceProvider resourceProvider
            , ITableRepository tableRepository
            , IEventBus eventBus
            , LifetimeScope sceneLifetimeScope
            , IPlayerContextManager playerContextManager)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _eventBus = eventBus;
            _playerContextManager = playerContextManager;
            _sceneLifetimeScope = sceneLifetimeScope;
        }

        /// <summary>
        /// 플레이어 캐릭터를 생성합니다
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(int playerConfigId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress(playerConfigId);
                var playerInstance = await _resourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = await InitializePlayerInstance(playerInstance, playerConfigId);
                return entity;
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 생성합니다.
        /// </summary>
        public EntityBase GetPlayerSync(int playerConfigId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress(playerConfigId);
                var playerInstance = _resourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                var entity = InitializePlayerInstance(playerInstance, playerConfigId).GetAwaiter().GetResult();
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
        private string GetPlayerAddress(int playerConfigId)
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(playerConfigId);
            if (config == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {playerConfigId}");
                return null;
            }

            if (string.IsNullOrEmpty(config.GameObjectKey))
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 게임오브젝트 키가 설정되지 않았습니다. ID: {playerConfigId}");
                return null;
            }

            return config.GameObjectKey;
        }

        /// <summary>
        /// Player 게임오브젝트 초기화, 컴포넌트 부착, 바인딩, UI 연동
        /// </summary>
        private async UniTask<EntityBase> InitializePlayerInstance(GameObject playerInstance, int playerConfigId)
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
            
            entity.SetContext(_playerContextManager.EntityContext);
            
            var movementComponent = entityLifeTimeScope.Container.Resolve<IMovementComponent>();
            var interactComponent = entityLifeTimeScope.Container.Resolve<IInteractComponent>();
            var dashComponent = entityLifeTimeScope.Container.Resolve<IDashComponent>();
            var healthComponent = entityLifeTimeScope.Container.Resolve<IHealthComponent>();
            var cameraTrackComponent = entityLifeTimeScope.Container.Resolve<ICameraTrackComponent>();
            var stateMachine = entityLifeTimeScope.Container.Resolve<IEntityStateMachine>();

            stateMachine.Initialize(
                entityLifeTimeScope.Container.Resolve<IdleState>(),
                entityLifeTimeScope.Container.Resolve<MoveState>(),
                entityLifeTimeScope.Container.Resolve<DashState>(),
                entityLifeTimeScope.Container.Resolve<SkillState>(),
                entityLifeTimeScope.Container.Resolve<InteractState>());
            
            var interactNotice = await _resourceProvider.GetInstanceAsync(CommonAddresses.InteractNotice);
            interactComponent.SetInteractNotice(interactNotice);

            await cameraTrackComponent.AttachCameraAsync();
            
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(playerConfigId);
            
            entity.OnDestroyed += (self) =>
            {
                _eventBus.Publish(new PlayerObjectDestroyEvent {player = self, position = playerInstance.transform.position});
            };

            healthComponent.OnDeath += () =>
            {
                Object.Destroy(entity.gameObject);
                _eventBus.Publish(new PlayerDeadEvent() {player = entity, position = playerInstance.transform.position, playerConfigTableEntry = config});
            };

            _eventBus.Publish(new PlayerObjectSpawnEvent{ player = entity, playerConfigTableEntry = config, position = playerInstance.transform.position});
            return entity;
        }
    }
}
