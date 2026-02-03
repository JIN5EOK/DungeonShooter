using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IEnemyFactory
    {
        UniTask<Enemy> GetRandomEnemyAsync();
        Enemy GetRandomEnemySync();
    }
    
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리
    /// </summary>
    public class EnemyFactory : IEnemyFactory
    {
        private readonly ITableRepository _tableRepository;
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private List<int> _enemyIds;

        [Inject]
        public EnemyFactory(ITableRepository tableRepository, StageContext stageContext, ISceneResourceProvider sceneResourceProvider)
        {
            _tableRepository = tableRepository;
            _stageContext = stageContext;
            _sceneResourceProvider = sceneResourceProvider;
            Initialize();
        }

        /// <summary>
        /// StageConfigTableEntry의 EnemyKeys(EnemyConfigTableEntry Id 목록)를 로드하여 저장합니다.
        /// </summary>
        private void Initialize()
        {
            var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
            if (stageConfigEntry == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                return;
            }

            if (stageConfigEntry.EnemyKeys != null && stageConfigEntry.EnemyKeys.Count > 0)
            {
                _enemyIds = new List<int>(stageConfigEntry.EnemyKeys);
            }
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 가져옵니다.
        /// </summary>
        public async UniTask<Enemy> GetRandomEnemyAsync()
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            if (enemyConfig == null)
            {
                return null;
            }

            var enemyInstance = await _sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey);
            return GetEnemyFromInstance(enemyInstance, enemyConfig);
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 동기적으로 가져옵니다.
        /// </summary>
        public Enemy GetRandomEnemySync()
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            if (enemyConfig == null)
            {
                return null;
            }

            var enemyInstance = _sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey);
            return GetEnemyFromInstance(enemyInstance, enemyConfig);
        }

        /// <summary>
        /// EnemyKeys에서 랜덤 ID를 선택하고, 해당 EnemyConfigTableEntry의 GameObjectKey(어드레스)를 반환합니다.
        /// </summary>
        private EnemyConfigTableEntry GetRandomEnemyTableConfig()
        {
            if (_enemyIds == null || _enemyIds.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 ID 목록이 비어있습니다.");
                return null;
            }

            var enemyId = _enemyIds[Random.Range(0, _enemyIds.Count)];
            var enemyEntry = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(enemyId);
            if (enemyEntry == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] EnemyConfigTableEntry를 찾을 수 없습니다. ID: {enemyId}");
                return null;
            }

            if (string.IsNullOrEmpty(enemyEntry.GameObjectKey))
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] EnemyConfigTableEntry에 GameObjectKey가 없습니다. ID: {enemyId}");
                return null;
            }

            return enemyEntry;
        }

        /// <summary>
        /// 인스턴스에서 Enemy 컴포넌트 추출 및 검증
        /// </summary>
        private Enemy GetEnemyFromInstance(GameObject enemyInstance, EnemyConfigTableEntry configTableEntry)
        {
            if (enemyInstance == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 인스턴스 생성 실패: {configTableEntry.GameObjectKey}");
                return null;
            }

            enemyInstance.layer = PhysicalLayers.Enemy.LayerIndex;
            var enemy = _sceneResourceProvider.AddOrGetComponentWithInejct<Enemy>(enemyInstance);
            enemy.Initialize(configTableEntry);
            return enemy;
        }
    }
}
