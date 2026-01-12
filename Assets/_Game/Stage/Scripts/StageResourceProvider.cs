using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using Jin5eok;
using Object = UnityEngine.Object;

namespace DungeonShooter
{

    public interface IStageResourceProvider : IDisposable
    {
        Awaitable<TileBase> GetTopTile();
        Awaitable<TileBase> GetWallTile();
        Awaitable<TileBase> GetGroundTile();
        Awaitable<TileBase> GetTile(string address);
        Awaitable<Enemy> GetRandomEnemy();
        Awaitable<GameObject> GetGameObject(string address);
    }
    /// <summary>
    /// 현재 스테이지에 적절한 타일, 캐릭터를 제공하는 클래스
    /// </summary>
    public class StageResourceProvider : IStageResourceProvider
    {
        private readonly AddressablesScope _addressablesScope;
        private readonly StageConfig _stageConfig;
        private List<string> EnemyAddresses { get; set; }
        
        public StageResourceProvider(StageConfig config)
        {
            _addressablesScope = new AddressablesScope();
            _stageConfig = config;
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        public async Awaitable InitializeAsync()
        {
            if (!string.IsNullOrEmpty(_stageConfig.StageEnemyLabel.labelString))
            {
                var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.StageEnemyLabel.labelString);
                await handle.Task;
                EnemyAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                Addressables.Release(handle);
            }
        }
        
        
        /// <summary>
        /// Top 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetTopTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.TopTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// Wall 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetWallTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.WallTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// Ground 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetGroundTile()
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(_stageConfig.GroundTile);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async Awaitable<Enemy> GetRandomEnemy()
        {
            if (EnemyAddresses == null || EnemyAddresses.Count == 0)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 적 어드레스 목록이 초기화되지 않았습니다. InitializeAsync를 먼저 호출하세요.");
                return null;
            }

            // 랜덤으로 하나 선택
            var enemyAddress = EnemyAddresses[UnityEngine.Random.Range(0, EnemyAddresses.Count)];

            // 적 프리팹 로드
            var enemyPrefabHandle = _addressablesScope.LoadAssetAsync<GameObject>(enemyAddress);
            await enemyPrefabHandle.Task;

            if (enemyPrefabHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 적 프리팹 로드 실패: {enemyAddress}");
                return null;
            }

            var enemyPrefab = enemyPrefabHandle.Result;
            var enemyInstance = Object.Instantiate(enemyPrefab);
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
        /// 주소에 해당하는 게임 오브젝트를 생성합니다.
        /// </summary>
        public async Awaitable<GameObject> GetGameObject(string address)
        {
            var handle = _addressablesScope.InstantiateAsync(address);
            await handle.Task;
            return handle.Result;
        }

        /// <summary>
        /// 주소에 해당하는 타일을 가져옵니다.
        /// </summary>
        public async Awaitable<TileBase> GetTile(string address)
        {
            var handle = _addressablesScope.LoadAssetAsync<TileBase>(address);
            await handle.Task;
            return handle.Result;
        }

        public void Dispose()
        {
            _addressablesScope?.Dispose();
        }
    }
}