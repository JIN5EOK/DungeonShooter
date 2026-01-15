using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{

    public interface IStageResourceProvider : IDisposable
    {
        Awaitable<TileBase> GetWallTile();
        Awaitable<TileBase> GetGroundTile();
        Awaitable<Enemy> GetRandomEnemy();
        Awaitable<GameObject> GetInstance(string address);
        Awaitable<T> GetAsset<T>(string address) where T : Object;
    }
    /// <summary>
    /// 현재 스테이지에 적절한 타일, 캐릭터를 제공하는 클래스
    /// </summary>
    public class StageResourceProvider : IStageResourceProvider
    {
        private readonly AddressablesScope _addressablesScope;
        private readonly StageConfig _stageConfig;

        private List<string> EnemyAddresses { get; set; }
        private IObjectResolver _resolver;
        private TaskCompletionSource<bool> _initializationTcs;
        private bool _isInitialized;
        [Inject]
        public StageResourceProvider(StageContext context, IObjectResolver resolver)
        {
            _addressablesScope = new AddressablesScope();
            _stageConfig = context.StageConfig;
            _resolver = resolver;
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        private async Awaitable InitializeAsync(TaskCompletionSource<bool> initializationTcs)
        {
            if (!string.IsNullOrEmpty(_stageConfig.StageEnemyLabel.labelString))
            {
                var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.StageEnemyLabel.labelString);
                await handle.Task;
                EnemyAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                Addressables.Release(handle);
            }
            initializationTcs.SetResult(true);
            _isInitialized = true;
        }
        
        /// <summary>
        /// 초기화가 완료될 때까지 대기합니다. 이미 초기화되어 있으면 즉시 반환합니다.
        /// </summary>
        private async Awaitable EnsureInitializedAsync()
        {
            if (_initializationTcs == null)
            {
                _initializationTcs = new TaskCompletionSource<bool>();
                await InitializeAsync(_initializationTcs);
            }

            await _initializationTcs.Task;
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
            await EnsureInitializedAsync();
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
        /// 주소에 해당하는 인스턴스를 생성하고 의존성 주입
        /// </summary>
        public async Awaitable<GameObject> GetInstance(string address)
        {
            var handle = _addressablesScope.InstantiateAsync(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 인스턴스 생성 실패: {address}");
                return null;
            }

            if (handle.Result == null)
            {
                Debug.LogWarning($"[{nameof(StageResourceProvider)}] 인스턴스가 null입니다: {address}");
                return null;
            }

            _resolver?.InjectGameObject(handle.Result);
            return handle.Result;
        }

        /// <summary>
        /// 주소에 해당하는 에셋을 가져옵니다.
        /// </summary>
        public async Awaitable<T> GetAsset<T>(string address) where T : Object
        {
            var handle = _addressablesScope.LoadAssetAsync<T>(address);
            await handle.Task;
            return handle.Result;
        }

        public void Dispose()
        {
            _addressablesScope?.Dispose();
        }
    }
}