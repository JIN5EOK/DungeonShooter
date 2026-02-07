using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IPlayerFactory
    {
        public event Action<EntityBase> OnPlayerCreated;
        UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<EntityBase> GetPlayerByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerByConfigIdSync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        public event Action<EntityBase> OnPlayerCreated;
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly PlayerManager _playerManager;
        private PlayerConfigTableEntry _playerConfigTableEntry;

        [Inject]
        public PlayerFactory(StageContext context
            , ISceneResourceProvider sceneResourceProvider
            , ITableRepository tableRepository
            , PlayerManager playerManager)
        {
            _stageContext = context;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
            _playerManager = playerManager;
            _playerConfigTableEntry = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
        }

        /// <summary>
        /// 플레이어 캐릭터를 가져옵니다
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress();
                var playerInstance = await _sceneResourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                return await InitializePlayerInstance(playerInstance);
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 가져옵니다.
        /// </summary>
        public EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            try
            {
                var playerAddress = GetPlayerAddress();
                var playerInstance = _sceneResourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
                return InitializePlayerInstance(playerInstance).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                LogHandler.LogException<PlayerFactory>(e, "플레이어를 불러오지 못했습니다.");
                return null;
            }
        }

        /// <summary>
        /// 지정한 PlayerConfigTableEntry ID로 플레이어를 비동기 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetPlayerByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(configId);
            if (config == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {configId}");
                return null;
            }

            var playerInstance = await _sceneResourceProvider.GetInstanceAsync(config.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
            return await InitializePlayerInstanceWithConfig(playerInstance, config);
        }

        /// <summary>
        /// 지정한 PlayerConfigTableEntry ID로 플레이어를 동기 생성합니다.
        /// </summary>
        public EntityBase GetPlayerByConfigIdSync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(configId);
            if (config == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {configId}");
                return null;
            }

            var playerInstance = _sceneResourceProvider.GetInstanceSync(config.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
            return InitializePlayerInstanceWithConfig(playerInstance, config).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 지정한 설정으로 플레이어 인스턴스를 초기화합니다.
        /// </summary>
        private async UniTask<EntityBase> InitializePlayerInstanceWithConfig(GameObject playerInstance, PlayerConfigTableEntry config)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>("플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;
            var entity = _sceneResourceProvider.AddOrGetComponentWithInejct<EntityBase>(playerInstance);
            await InitializePlayerObject(playerInstance, entity);
            await _playerManager.Initialize(config);
            await _playerManager.BindPlayerEntity(entity);
            OnPlayerCreated?.Invoke(entity);
            return entity;
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress()
        {
            if (_playerConfigTableEntry == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            if (string.IsNullOrEmpty(_playerConfigTableEntry.GameObjectKey))
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 게임오브젝트 키가 설정되지 않았습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            return _playerConfigTableEntry.GameObjectKey;
        }

        /// <summary>
        /// Player 게임오브젝트 초기화
        /// </summary>
        private async UniTask<EntityBase> InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }
            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;
            var entity = playerInstance.AddComponent<EntityBase>();
            await _playerManager.Initialize(_playerConfigTableEntry);
            await _playerManager.BindPlayerEntity(entity);
            await InitializePlayerObject(playerInstance, entity);
            OnPlayerCreated?.Invoke(entity);
            return entity;
        }

        /// <summary>
        /// 씬용 컴포넌트 추가 및 사망 시 Destroy 구독을 설정합니다.
        /// </summary>
        private async UniTask InitializePlayerObject(GameObject playerInstance, EntityBase entity)
        {
            playerInstance.AddOrGetComponent<MovementComponent>();
            playerInstance.AddOrGetComponent<InteractComponent>();
            var healthComponent = playerInstance.AddOrGetComponent<HealthComponent>();
            playerInstance.AddOrGetComponent<DashComponent>();
            var cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(playerInstance);
            await cameraTrackComponent.AttachCameraAsync();
            healthComponent.FullHeal();
            healthComponent.OnDeath += () => entity.Destroy();
        }
    }
}
