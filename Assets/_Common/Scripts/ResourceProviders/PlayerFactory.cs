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
        private readonly StageConfig _stageConfig;
        private readonly ISceneResourceProvider _sceneResourceProvider;

        [Inject]
        public PlayerFactory(StageContext context, ISceneResourceProvider sceneResourceProvider)
        {
            _stageConfig = context.StageConfig;
            _sceneResourceProvider = sceneResourceProvider;
        }

        /// <summary>
        /// 플레이어 캐릭터를 가져옵니다
        /// TODO: 다양한 캐릭터 형태에 대응하도록 변경 필요
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
            if (_stageConfig.PlayerPrefab == null || !_stageConfig.PlayerPrefab.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[{nameof(PlayerFactory)}] 플레이어 프리팹이 설정되지 않았습니다.");
                return null;
            }

            return _stageConfig.PlayerPrefab.RuntimeKey.ToString();
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
