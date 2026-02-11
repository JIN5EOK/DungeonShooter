using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
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

    // TODO: 팩토리의 책임이 너무 비대해져서(UI 바인딩 및 컴포넌트까지 들고 있게 됨, 분리가 필요함)
    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성·이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;
        
        private readonly PlayerStatusController _playerStatusController;
        private readonly PlayerSkillController _playerSkillController;
        private readonly Inventory _inventory;
        private readonly PlayerInstanceManager _playerInstanceManager;

        [Inject]
        public PlayerFactory(StageContext context
            , ISceneResourceProvider sceneResourceProvider
            , ITableRepository tableRepository
            , PlayerStatusController playerStatusController
            , PlayerSkillController playerSkillController
            , Inventory inventory
            , PlayerInstanceManager playerInstanceManager)
        {
            _stageContext = context;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
            _playerStatusController = playerStatusController;
            _playerSkillController = playerSkillController;
            _inventory = inventory;
            _playerInstanceManager = playerInstanceManager;
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
            _playerInstanceManager.UnbindAndDestroy();
            
            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;
            var entity = playerInstance.AddComponent<EntityBase>();

            entity.gameObject.AddOrGetComponent<MovementComponent>();
            entity.gameObject.AddOrGetComponent<InteractComponent>();
            var healthComponent = entity.gameObject.AddOrGetComponent<HealthComponent>();
            entity.gameObject.AddOrGetComponent<DashComponent>();
            var cameraTrackComponent = _sceneResourceProvider.AddOrGetComponentWithInejct<CameraTrackComponent>(entity.gameObject);
            await cameraTrackComponent.AttachCameraAsync();
            
            healthComponent.OnDeath += () => Object.Destroy(entity.gameObject);

            entity.OnDestroyed += (entity) => _playerInstanceManager.UnbindAndDestroy();
            await _playerInstanceManager.BindAsync(entity);

            return entity;
        }
    }
}
