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
        UniTask<EntityBase> GetPlayerAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetPlayerSync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
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
            await _playerManager.InitializePlayer(_playerConfigTableEntry, entity);
            return entity;
        }
    }
}
