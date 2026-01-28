using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly ITableRepository _tableRepository;

        [Inject]
        public PlayerFactory(StageContext context, ISceneResourceProvider sceneResourceProvider, ITableRepository tableRepository)
        {
            _stageContext = context;
            _sceneResourceProvider = sceneResourceProvider;
            _tableRepository = tableRepository;
        }

        /// <summary>
        /// 플레이어 캐릭터를 가져옵니다
        /// </summary>
        public async UniTask<Player> GetPlayerAsync()
        {
            var playerAddress = GetPlayerAddress();
            if (playerAddress == null)
            {
                return null;
            }

            var playerInstance = await _sceneResourceProvider.GetInstanceAsync(playerAddress);
            return GetPlayerFromInstance(playerInstance, playerAddress);
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 가져옵니다.
        /// </summary>
        public Player GetPlayerSync()
        {
            var playerAddress = GetPlayerAddress();
            if (playerAddress == null)
            {
                return null;
            }

            var playerInstance = _sceneResourceProvider.GetInstanceSync(playerAddress);
            return GetPlayerFromInstance(playerInstance, playerAddress);
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress()
        {
            var playerConfig = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (playerConfig == null)
            {
                Debug.LogWarning($"[{nameof(PlayerFactory)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            if (string.IsNullOrEmpty(playerConfig.GameObjectKey))
            {
                Debug.LogWarning($"[{nameof(PlayerFactory)}] 플레이어 게임오브젝트 키가 설정되지 않았습니다. ID: {_stageContext.PlayerConfigTableId}");
                return null;
            }

            return playerConfig.GameObjectKey;
        }

        /// <summary>
        /// 인스턴스에서 Player 컴포넌트 추출 및 검증
        /// </summary>
        private Player GetPlayerFromInstance(GameObject playerInstance, string playerAddress)
        {
            if (playerInstance == null)
            {
                Debug.LogWarning($"[{nameof(PlayerFactory)}] 플레이어 인스턴스 생성 실패: {playerAddress}");
                return null;
            }

            var player = playerInstance.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogWarning($"[{nameof(PlayerFactory)}] 프리팹에 Player 컴포넌트가 없습니다: {playerAddress}");
                Object.Destroy(playerInstance);
                return null;
            }

            return player;
        }
    }
}
