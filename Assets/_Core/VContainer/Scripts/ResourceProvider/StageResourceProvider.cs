using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using VContainer;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 현재 스테이지에 적절한 타일, 캐릭터를 제공하는 클래스
    /// </summary>
    public class StageResourceProvider : SceneResourceProvider, IStageResourceProvider
    {
        private readonly StageConfig _stageConfig;
        private List<string> EnemyAddresses { get; set; }
        [Inject]
        public StageResourceProvider(StageContext context, IObjectResolver resolver) : base(resolver)
        {
            _stageConfig = context.StageConfig;
            Initialize();
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        private void Initialize()
        {
            if (!string.IsNullOrEmpty(_stageConfig.StageEnemyLabel.labelString))
            {
                var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.StageEnemyLabel.labelString);
                handle.WaitForCompletion();
                EnemyAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                Addressables.Release(handle);
            }
        }
    
        /// <summary>
        /// Ground 타일을 가져옵니다.
        /// </summary>
        public async UniTask<TileBase> GetGroundTileAsync()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.GroundTile);
            await handle.Task;
            return GetGroundTileInternal(handle);
        }

        /// <summary>
        /// Ground 타일을 동기적으로 가져옵니다.
        /// </summary>
        public TileBase GetGroundTileSync()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.GroundTile);
            handle.WaitForCompletion();
            return GetGroundTileInternal(handle);
        }

        /// <summary>
        /// Ground 타일 로드 후 처리
        /// </summary>
        private TileBase GetGroundTileInternal(AsyncOperationHandle<TileBase> handle)
        {
            return handle.Result;
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async UniTask<Enemy> GetRandomEnemyAsync()
        {
            var enemyAddress = GetRandomEnemyAddress();
            if (enemyAddress == null)
            {
                return null;
            }

            var enemyInstance = await GetInstance(enemyAddress);
            return GetEnemyFromInstance(enemyInstance, enemyAddress);
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 동기적으로 가져옵니다.
        /// </summary>
        public Enemy GetRandomEnemySync()
        {
            var enemyAddress = GetRandomEnemyAddress();
            if (enemyAddress == null)
            {
                return null;
            }

            var enemyInstance = GetInstanceSync(enemyAddress);
            return GetEnemyFromInstance(enemyInstance, enemyAddress);
        }

        /// <summary>
        /// 랜덤 적 어드레스 선택
        /// </summary>
        private string GetRandomEnemyAddress()
        {
            if (EnemyAddresses == null || EnemyAddresses.Count == 0)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 적 어드레스 목록이 비어있습니다.");
                return null;
            }

            return EnemyAddresses[Random.Range(0, EnemyAddresses.Count)];
        }

        /// <summary>
        /// 인스턴스에서 Enemy 컴포넌트 추출 및 검증
        /// </summary>
        private Enemy GetEnemyFromInstance(GameObject enemyInstance, string enemyAddress)
        {
            if (enemyInstance == null)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 적 인스턴스 생성 실패: {enemyAddress}");
                return null;
            }

            var enemy = enemyInstance.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 프리팹에 Enemy 컴포넌트가 없습니다: {enemyAddress}");
                Object.Destroy(enemyInstance);
                return null;
            }

            return enemy;
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

            var playerInstance = await GetInstance(playerAddress);
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

            var playerInstance = GetInstanceSync(playerAddress);
            return GetPlayerFromInstance(playerInstance, playerAddress);
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress()
        {
            if (_stageConfig.PlayerPrefab == null || !_stageConfig.PlayerPrefab.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 플레이어 프리팹이 설정되지 않았습니다.");
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
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 플레이어 인스턴스 생성 실패: {playerAddress}");
                return null;
            }

            var player = playerInstance.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 프리팹에 Player 컴포넌트가 없습니다: {playerAddress}");
                Object.Destroy(playerInstance);
                return null;
            }

            return player;
        }

    }
}