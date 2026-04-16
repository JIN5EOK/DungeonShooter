using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        event Action<EntityBase, PlayerConfigSo, Vector3> PlayerSpawned;
        event Action<EntityBase, Vector3> PlayerDestroyed;
        event Action<EntityBase, PlayerConfigSo, Vector3> PlayerDied;
        public UniTask<EntityBase> GetPlayerAsync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        public EntityBase GetPlayerSync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성/이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        public event Action<EntityBase, PlayerConfigSo, Vector3> PlayerSpawned;
        public event Action<EntityBase, Vector3> PlayerDestroyed;
        public event Action<EntityBase, PlayerConfigSo, Vector3> PlayerDied;

        private int _playerConfigTableId = 12000001;
        private readonly IResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly IPlayerContextManager _playerContextManager;
        private readonly LifetimeScope _sceneLifetimeScope;
        [Inject]
        public PlayerFactory(
            IResourceProvider resourceProvider
            , ITableRepository tableRepository
            , LifetimeScope sceneLifetimeScope
            , IPlayerContextManager playerContextManager)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _playerContextManager = playerContextManager;
            _sceneLifetimeScope = sceneLifetimeScope;
        }

        /// <summary>
        /// 플레이어 캐릭터를 생성합니다
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var playerAddress = GetPlayerAddress(config);
            if (string.IsNullOrEmpty(playerAddress))
                return null;
            var playerInstance = await _resourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
            var entity = InitializePlayerInstance(playerInstance);
            return entity;
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 생성합니다.
        /// </summary>
        public EntityBase GetPlayerSync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var playerAddress = GetPlayerAddress(config);
            if (string.IsNullOrEmpty(playerAddress))
                return null;
            var playerInstance = _resourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
            var entity = InitializePlayerInstance(playerInstance);
            return entity;
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress(PlayerConfigSo config)
        {
            var address = GetAddressOrNull(config.GameObjectRef);
            return address;
        }

        /// <summary>
        /// Player 게임오브젝트 초기화, 컴포넌트 부착, 바인딩, UI 연동
        /// </summary>
        private EntityBase InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

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
            
            var interactNotice = _resourceProvider.GetInstanceSync(CommonAddresses.InteractNotice);
            interactComponent.SetInteractNotice(interactNotice);

            cameraTrackComponent.AttachCameraAsync().Forget();
            
            var config = _tableRepository.GetTableEntry<PlayerConfigSo>(_playerConfigTableId);
            
            entity.OnDestroyed += (self) =>
            {
                PlayerDestroyed?.Invoke(self, playerInstance.transform.position);
            };

            healthComponent.OnDeath += () =>
            {
                Object.Destroy(entity.gameObject);
                PlayerDied?.Invoke(entity, config, playerInstance.transform.position);
            };

            PlayerSpawned?.Invoke(entity, config, playerInstance.transform.position);
            return entity;
        }

        private static string GetAddressOrNull(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return null;
            var key = assetReference.RuntimeKey.ToString();
            return string.IsNullOrEmpty(key) ? null : key;
        }
    }
}
