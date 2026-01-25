using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;
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
        public async UniTask<TileBase> GetGroundTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.GroundTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async UniTask<Enemy> GetRandomEnemy()
        {
            if (EnemyAddresses == null || EnemyAddresses.Count == 0)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 적 어드레스 목록이 비어있습니다.");
                return null;
            }

            // 랜덤으로 하나 선택
            var enemyAddress = EnemyAddresses[Random.Range(0, EnemyAddresses.Count)];

            var enemyInstance = await GetInstance(enemyAddress);
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
        public async UniTask<Player> GetPlayer()
        {
            if (_stageConfig.PlayerPrefab == null || !_stageConfig.PlayerPrefab.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 플레이어 프리팹이 설정되지 않았습니다.");
                return null;
            }

            var playerAddress = _stageConfig.PlayerPrefab.RuntimeKey.ToString();
            var playerInstance = await GetInstance(playerAddress);
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