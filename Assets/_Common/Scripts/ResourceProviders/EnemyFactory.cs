using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IEnemyFactory
    {
        UniTask<EntityBase> GetRandomEnemyAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetRandomEnemySync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        EntityBase GetEnemyByConfigIdSync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }
    
    /// <summary>
    /// 적 캐릭터를 생성하는 팩토리
    /// </summary>
    public class EnemyFactory : IEnemyFactory
    {
        private readonly ITableRepository _tableRepository;
        private readonly StageContext _stageContext;
        private readonly ISceneResourceProvider _sceneResourceProvider;
        private readonly IEventBus _eventBus;
        private List<int> _enemyIds;
        private LifetimeScope _sceneLifetimeScope;
        [Inject]
        public EnemyFactory(ITableRepository tableRepository, StageContext stageContext, ISceneResourceProvider sceneResourceProvider, IEventBus eventBus, LifetimeScope sceneLifeTimeScope)
        {
            _tableRepository = tableRepository;
            _stageContext = stageContext;
            _sceneResourceProvider = sceneResourceProvider;
            _eventBus = eventBus;
            _sceneLifetimeScope = sceneLifeTimeScope;
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
        public async UniTask<EntityBase> GetRandomEnemyAsync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            if (enemyConfig == null)
            {
                return null;
            }

            var enemyInstance = await _sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
            return GetEnemyFromInstance(enemyInstance, enemyConfig);
        }

        /// <summary>
        /// 스테이지에 맞는 랜덤 적을 동기적으로 가져옵니다.
        /// </summary>
        public EntityBase GetRandomEnemySync(Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = GetRandomEnemyTableConfig();
            if (enemyConfig == null)
            {
                return null;
            }

            var enemyInstance = _sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey,  position, rotation, parent, instantiateInWorldSpace);
            return GetEnemyFromInstance(enemyInstance, enemyConfig);
        }

        /// <summary>
        /// 지정한 EnemyConfigTableEntry ID로 적을 비동기 생성합니다.
        /// </summary>
        public async UniTask<EntityBase> GetEnemyByConfigIdAsync(int configId, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(configId);
            if (enemyConfig == null)
            {
                LogHandler.LogWarning<EnemyFactory>($"EnemyConfigTableEntry를 찾을 수 없습니다. ID: {configId}");
                return null;
            }

            var enemyInstance = await _sceneResourceProvider.GetInstanceAsync(enemyConfig.GameObjectKey,  position, rotation, parent, instantiateInWorldSpace);
            return GetEnemyFromInstance(enemyInstance, enemyConfig);
        }

        /// <summary>
        /// 지정한 EnemyConfigTableEntry ID로 적을 동기 생성합니다.
        /// </summary>
        public EntityBase GetEnemyByConfigIdSync(int configId,  Vector3 position = default, Quaternion rotation = default, Transform parent = null,  bool instantiateInWorldSpace = true)
        {
            var enemyConfig = _tableRepository.GetTableEntry<EnemyConfigTableEntry>(configId);
            if (enemyConfig == null)
            {
                LogHandler.LogWarning<EnemyFactory>($"EnemyConfigTableEntry를 찾을 수 없습니다. ID: {configId}");
                return null;
            }

            var enemyInstance = _sceneResourceProvider.GetInstanceSync(enemyConfig.GameObjectKey, position, rotation, parent, instantiateInWorldSpace);
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
        /// 인스턴스에 Enemy 컴포넌트를 붙이고, 스탯/컴포넌트/사망 시 Destroy 구독을 설정합니다.
        /// </summary>
        private EntityBase GetEnemyFromInstance(GameObject enemyInstance, EnemyConfigTableEntry configTableEntry)
        {
            if (enemyInstance == null)
            {
                Debug.LogWarning($"[{nameof(EnemyFactory)}] 적 인스턴스 생성 실패: {configTableEntry.GameObjectKey}");
                return null;
            }

            enemyInstance.tag = GameTags.Enemy;
            enemyInstance.layer = PhysicalLayers.Enemy.LayerIndex;
            
            // 씬 LifeTimeScope를 부모로 삼기
            EntityLifeTimeScope entityLifeTimeScope = null;
            using (LifetimeScope.EnqueueParent(_sceneLifetimeScope))
            {
                entityLifeTimeScope = enemyInstance.AddOrGetComponent<EntityLifeTimeScope>();    
            }
            
            var entity = entityLifeTimeScope.Container.Resolve<EntityBase>();
            var statsEntry = _tableRepository.GetTableEntry<EntityStatsTableEntry>(configTableEntry.StatsId);
            if (statsEntry != null)
            {
                var statGroup = new EntityStatGroup();
                statGroup.Initialize(statsEntry);
                entity.SetStatGroup(statGroup);
            }

            var movementCompoent = entityLifeTimeScope.Container.Resolve<MovementComponent>();
            var healthComponent = entityLifeTimeScope.Container.Resolve<HealthComponent>();
            healthComponent.OnDeath += () =>
            {
                _eventBus.Publish(new EnemyDestroyEvent { enemy = entity, enemyConfigTableEntry = configTableEntry });
                CoroutineManager.Delay(0.5f, () => entity.Destroy());
            };

            var aiBT = _sceneResourceProvider.GetAssetSync<AiBTBase>(configTableEntry.AIType);
            entityLifeTimeScope.Container.Resolve<AIComponent>().SetBT(aiBT);

            return entity;
        }
    }
}
